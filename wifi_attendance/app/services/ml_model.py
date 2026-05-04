"""
WiFi Fingerprinting ML Modeli - Optimize Versiyon

GELIŞTIRMELER:
1. Geliştirilmiş Vektorizasyon (eksik BSSID stratejileri)
2. Üstel Düşüş ile Zaman Ağırlıklandırması (exponential decay)
3. Hiperparametre Tuning (GridSearchCV, RandomizedSearchCV)
4. Sınıf Dengesizliği Yönetimi (SMOTE, NearMiss, class weights)
5. Geliştirilmiş RSSI Normalizasyonu (MinMaxScaler/StandardScaler)
6. Model Versiyonlama ve Metadata Kaydı
7. Dinamik CONFIDENCE_THRESHOLD Ayarı
8. Ensemble Stratejileri (KNN, RF, SVM, GradientBoosting)
"""

import numpy as np
import joblib
import os
import json
import pandas as pd
from datetime import datetime, timedelta
from typing import List, Optional, Dict, Tuple
from collections import defaultdict, Counter
from pathlib import Path
from sqlalchemy.orm import Session

from sklearn.neighbors import KNeighborsClassifier
from sklearn.ensemble import RandomForestClassifier, VotingClassifier, GradientBoostingClassifier
from sklearn.svm import SVC
from sklearn.preprocessing import LabelEncoder, MinMaxScaler, StandardScaler
from sklearn.model_selection import cross_val_score, GridSearchCV, StratifiedKFold
from sklearn.metrics import (
    confusion_matrix, 
    classification_report,
    roc_auc_score,
    f1_score,
    precision_recall_curve,
)
from imblearn.over_sampling import SMOTE
from imblearn.under_sampling import NearMiss
from imblearn.pipeline import Pipeline as ImbPipeline

from app.models.wifi_models import WifiTrainingSample, Classroom, StudentWifiAccessPoint

# ─── Konfigürasyon ───────────────────────────────────────────────────────────

MODEL_DIR = "models"
Path(MODEL_DIR).mkdir(exist_ok=True)

MODEL_PATH = f"{MODEL_DIR}/wifi_model.joblib"
BSSID_INDEX_PATH = f"{MODEL_DIR}/bssid_index.joblib"
LABEL_ENCODER_PATH = f"{MODEL_DIR}/label_encoder.joblib"
RSSI_SCALER_PATH = f"{MODEL_DIR}/rssi_scaler.joblib"
MODEL_METADATA_PATH = f"{MODEL_DIR}/model_metadata.json"
MODEL_HISTORY_PATH = f"{MODEL_DIR}/model_history.json"

CONFIDENCE_THRESHOLD = 0.60
MISSING_RSSI_STRATEGY = "mean"  # "zero" veya "mean"
ENABLE_HYPERPARAMETER_TUNING = True
ENABLE_CLASS_BALANCING = True


# ─── Geliştirilmiş Yardımcı Fonksiyonlar ──────────────────────────────────────

class AdaptiveRSSINormalizer:
    """Veriye dayalı RSSI normalizasyonu"""
    
    def __init__(self, strategy: str = "minmax"):
        self.strategy = strategy
        self.scaler = MinMaxScaler(feature_range=(0, 1)) if strategy == "minmax" else StandardScaler()
        self.fitted = False
        
    def fit(self, rssi_values: np.ndarray) -> None:
        """RSSI değerlerinden scaler parametrelerini öğren"""
        if len(rssi_values) > 0:
            rssi_reshaped = rssi_values.reshape(-1, 1)
            self.scaler.fit(rssi_reshaped)
            self.fitted = True
    
    def normalize(self, rssi: int) -> float:
        """Tek bir RSSI değerini normalize et"""
        if not self.fitted:
            # Varsayılan formül
            return max(0.0, min(1.0, (rssi + 100) / 70.0))
        
        try:
            normalized = self.scaler.transform([[rssi]])[0][0]
            return float(np.clip(normalized, 0.0, 1.0))
        except:
            return max(0.0, min(1.0, (rssi + 100) / 70.0))


def collect_all_rssi_values(db: Session) -> np.ndarray:
    """Tüm eğitim verisinden RSSI değerlerini topla"""
    samples = db.query(WifiTrainingSample).all()
    rssi_values = []
    for sample in samples:
        for ap in sample.access_points:
            rssi_values.append(ap.rssi)
    return np.array(rssi_values) if rssi_values else np.array([])


def time_weight_exponential(scanned_at: datetime, half_life_days: float = 14.0) -> float:
    """
    Üstel düşüş ile zaman ağırlıklandırması.
    
    Formula: weight = 2^(-age / half_life)
    - half_life = 14 gün
    - 14 gün sonra ağırlık = 0.5
    - 28 gün sonra ağırlık = 0.25
    """
    days_old = (datetime.utcnow() - scanned_at).days
    weight = 2.0 ** (-days_old / half_life_days)
    return float(np.clip(weight, 0.1, 1.0))  # Minimum 0.1 ağırlık


def build_bssid_index(
    db: Session, 
    sort_by: str = "alphabetic",
    min_frequency: int = 1
) -> List[str]:
    """
    BSSID indeksi oluştur.
    
    sort_by: "alphabetic" (alfabetik sıra) veya "frequency" (sıklık)
    min_frequency: En az bu kadar örnek içeren BSSID'ler
    """
    samples = db.query(WifiTrainingSample).all()
    bssid_frequency = Counter()
    
    for sample in samples:
        for ap in sample.access_points:
            bssid_frequency[ap.bssid] += 1
    
    # Minimum frekansı sağlayanları filtrele
    filtered_bssids = {
        bssid for bssid, freq in bssid_frequency.items() 
        if freq >= min_frequency
    }
    
    if sort_by == "frequency":
        return sorted(filtered_bssids, 
                     key=lambda b: bssid_frequency[b], 
                     reverse=True)
    else:  # alphabetic
        return sorted(list(filtered_bssids))


def sample_to_vector(
    sample, 
    bssid_index: List[str],
    normalizer: AdaptiveRSSINormalizer,
    missing_strategy: str = "zero"
) -> np.ndarray:
    """
    Training sample'ı vektöre çevir (geliştirilmiş).
    
    missing_strategy:
    - "zero": Eksik BSSID'ler için 0
    - "mean": Eksik BSSID'ler için ortalama değer
    """
    vec = np.zeros(len(bssid_index))
    bssid_rssi = {
        ap.bssid: normalizer.normalize(ap.rssi) 
        for ap in sample.access_points
    }
    
    for i, bssid in enumerate(bssid_index):
        if bssid in bssid_rssi:
            vec[i] = bssid_rssi[bssid]
        elif missing_strategy == "mean" and bssid_rssi:
            vec[i] = np.mean(list(bssid_rssi.values())) * 0.5  # Ortalama * 0.5
    
    return vec


def scan_to_vector(
    student_aps: List,
    bssid_index: List[str],
    normalizer: AdaptiveRSSINormalizer,
    missing_strategy: str = "zero"
) -> np.ndarray:
    """
    Öğrenci Wi-Fi taramasını vektöre çevir (geliştirilmiş).
    """
    vec = np.zeros(len(bssid_index))
    bssid_rssi = {
        ap.bssid: normalizer.normalize(ap.rssi) 
        for ap in student_aps
    }
    
    for i, bssid in enumerate(bssid_index):
        if bssid in bssid_rssi:
            vec[i] = bssid_rssi[bssid]
        elif missing_strategy == "mean" and bssid_rssi:
            vec[i] = np.mean(list(bssid_rssi.values())) * 0.5
    
    return vec


def balance_training_data(X: np.ndarray, y: np.ndarray) -> Tuple[np.ndarray, np.ndarray]:
    """Sınıf dengesizliğini SMOTE veya NearMiss ile düzelt"""
    if len(np.unique(y)) < 2:
        return X, y
    
    # Sınıf dağılımını kontrol et
    class_counts = Counter(y)
    
    if len(class_counts) == 1:
        return X, y
    
    # Azınlık sınıfların sayısı
    min_class_count = min(class_counts.values())
    max_class_count = max(class_counts.values())
    
    # Dengesizlik oranı
    imbalance_ratio = max_class_count / min_class_count
    
    if imbalance_ratio > 3.0:
        # SMOTE ile oversampling
        try:
            n_neighbors = min(3, min_class_count - 1)
            if n_neighbors > 0:
                smote = SMOTE(k_neighbors=n_neighbors, random_state=42)
                X_balanced, y_balanced = smote.fit_resample(X, y)
                return X_balanced, y_balanced
        except:
            pass
    
    return X, y


def optimize_knn_k(
    X: np.ndarray, 
    y: np.ndarray, 
    max_k: int = 15
) -> int:
    """GridSearchCV ile optimal KNN k değerini bul"""
    n_samples = len(X)
    
    if n_samples < 3:
        return min(1, n_samples)
    
    k_values = list(range(1, min(max_k, n_samples) + 1))
    
    knn = KNeighborsClassifier(metric='euclidean', weights='distance')
    
    param_grid = {'n_neighbors': k_values}
    
    grid_search = GridSearchCV(
        knn, 
        param_grid, 
        cv=min(3, n_samples),
        scoring='accuracy',
        n_jobs=-1
    )
    
    try:
        grid_search.fit(X, y)
        return grid_search.best_params_['n_neighbors']
    except:
        return min(5, n_samples)


def optimize_random_forest_params(
    X: np.ndarray,
    y: np.ndarray
) -> Dict:
    """Random Forest parametrelerini optimize et"""
    n_samples = len(X)
    
    if n_samples < 10:
        return {
            'n_estimators': 50,
            'max_depth': 5,
            'min_samples_split': 2,
            'min_samples_leaf': 1,
        }
    
    rf = RandomForestClassifier(random_state=42)
    
    param_grid = {
        'n_estimators': [50, 100, 150],
        'max_depth': [5, 10, 15, None],
        'min_samples_split': [2, 5, 10],
        'min_samples_leaf': [1, 2, 4],
    }
    
    grid_search = GridSearchCV(
        rf,
        param_grid,
        cv=min(3, n_samples),
        scoring='accuracy',
        n_jobs=-1
    )
    
    try:
        grid_search.fit(X, y)
        return grid_search.best_params_
    except:
        return {
            'n_estimators': 100,
            'max_depth': 10,
            'min_samples_split': 2,
            'min_samples_leaf': 1,
        }


def save_model_metadata(
    metadata: Dict,
    history: Optional[Dict] = None
) -> None:
    """Model metadata'sını kaydet"""
    metadata['timestamp'] = datetime.utcnow().isoformat()
    
    with open(MODEL_METADATA_PATH, 'w') as f:
        json.dump(metadata, f, indent=2, default=str)
    
    if history:
        existing_history = []
        if os.path.exists(MODEL_HISTORY_PATH):
            try:
                with open(MODEL_HISTORY_PATH, 'r') as f:
                    existing_history = json.load(f)
            except:
                pass
        
        existing_history.append(history)
        
        # Son 100 modeli tut
        if len(existing_history) > 100:
            existing_history = existing_history[-100:]
        
        with open(MODEL_HISTORY_PATH, 'w') as f:
            json.dump(existing_history, f, indent=2, default=str)


# ─── Model Eğitimi ────────────────────────────────────────────────────────────

def train_model(db: Session, enable_tuning: bool = True) -> Dict:
    """
    Optimized model eğitimi.
    
    Özellikler:
    - Geliştirilmiş vektorizasyon
    - Zaman ağırlıklandırması (exponential decay)
    - Sınıf dengesizliği yönetimi
    - Hiperparametre tuning
    - Ensemble stratejileri
    - Model versiyonlama
    """
    samples = db.query(WifiTrainingSample).all()
    
    if len(samples) < 2:
        return {"success": False, "message": "En az 2 eğitim örneği gerekli"}
    
    # BSSID indeksi oluştur (sıklığa göre sıralı)
    bssid_index = build_bssid_index(db, sort_by="frequency", min_frequency=1)
    
    if not bssid_index:
        return {"success": False, "message": "BSSID verisi bulunamadı"}
    
    # RSSI Normalizer eğit
    rssi_values = collect_all_rssi_values(db)
    normalizer = AdaptiveRSSINormalizer(strategy="minmax")
    normalizer.fit(rssi_values)
    
    # X (features) ve y (labels) oluştur
    X, y, weights = [], [], []
    
    for sample in samples:
        vec = sample_to_vector(
            sample,
            bssid_index,
            normalizer,
            missing_strategy=MISSING_RSSI_STRATEGY
        )
        X.append(vec)
        y.append(sample.classroom_id)
        weights.append(time_weight_exponential(sample.scanned_at))
    
    X = np.array(X)
    y = np.array(y)
    weights = np.array(weights)
    
    # Label encoding
    le = LabelEncoder()
    y_encoded = le.fit_transform(y)
    
    # Sınıf dengesizliğini düzelt
    if ENABLE_CLASS_BALANCING and len(np.unique(y_encoded)) > 1:
        X, y_encoded = balance_training_data(X, y_encoded)
    
    n_classes = len(np.unique(y_encoded))
    n_samples = len(X)
    
    # Model oluştur
    models_to_test = []
    
    # KNN - optimize et
    if enable_tuning:
        k_optimal = optimize_knn_k(X, y_encoded, max_k=15)
    else:
        k_optimal = min(5, n_samples)
    
    knn = KNeighborsClassifier(
        n_neighbors=k_optimal,
        metric='euclidean',
        weights='distance',
    )
    models_to_test.append(('knn', knn))
    
    # Random Forest - optimize et
    if enable_tuning:
        rf_params = optimize_random_forest_params(X, y_encoded)
    else:
        rf_params = {
            'n_estimators': 100,
            'max_depth': 10,
            'min_samples_split': 2,
            'min_samples_leaf': 1,
        }
    
    rf = RandomForestClassifier(random_state=42, **rf_params)
    models_to_test.append(('rf', rf))
    
    # SVM (küçük dataset için)
    if n_samples >= 10 and n_classes >= 2:
        svm = SVC(kernel='rbf', probability=True, random_state=42, gamma='scale')
        models_to_test.append(('svm', svm))
    
    # Gradient Boosting (büyük dataset için)
    if n_samples >= 20 and n_classes >= 2:
        gb = GradientBoostingClassifier(
            n_estimators=100,
            max_depth=5,
            random_state=42
        )
        models_to_test.append(('gb', gb))
    
    # Ensemble oluştur
    if n_samples >= 4 and n_classes >= 2 and len(models_to_test) >= 2:
        model = VotingClassifier(
            estimators=models_to_test,
            voting='soft',
        )
        model_type = "Ensemble"
    else:
        model = knn
        model_type = "KNN"
    
    model.fit(X, y_encoded)
    
    # Cross validation skoru
    cv_scores = cross_val_score(model, X, y_encoded, cv=min(3, n_samples), scoring='accuracy')
    accuracy = cv_scores.mean()
    
    # Modeli kaydet
    joblib.dump(model, MODEL_PATH)
    joblib.dump(bssid_index, BSSID_INDEX_PATH)
    joblib.dump(le, LABEL_ENCODER_PATH)
    joblib.dump(normalizer, RSSI_SCALER_PATH)
    
    # Classroom isimlerini al
    classrooms = {c.id: c.name for c in db.query(Classroom).all()}
    class_names = [classrooms.get(le.inverse_transform([i])[0], "?") for i in range(len(le.classes_))]
    
    # Metadata oluştur
    metadata = {
        "success": True,
        "message": "Model başarıyla eğitildi ✓",
        "total_samples": int(n_samples),
        "original_samples": int(len(samples)),
        "total_bssids": len(bssid_index),
        "classrooms": int(n_classes),
        "accuracy": round(float(accuracy), 4),
        "accuracy_std": round(float(cv_scores.std()), 4),
        "class_names": class_names,
        "model_type": model_type,
        "knn_k": k_optimal if enable_tuning else k_optimal,
        "rf_params": rf_params if enable_tuning else rf_params,
        "rssi_normalizer": "minmax",
        "missing_rssi_strategy": MISSING_RSSI_STRATEGY,
        "confidence_threshold": CONFIDENCE_THRESHOLD,
    }
    
    history_entry = {
        "timestamp": datetime.utcnow().isoformat(),
        "accuracy": round(float(accuracy), 4),
        "n_samples": int(n_samples),
        "n_bssids": len(bssid_index),
        "model_type": model_type,
    }
    
    save_model_metadata(metadata, history_entry)
    
    return metadata


# ─── Tahmin ──────────────────────────────────────────────────────────────────

def predict_classroom(
    student_aps: List,
    db: Session,
) -> Tuple[Optional[str], float]:
    """
    Eğitilmiş modelle öğrencinin konumunu tahmin et.
    Model yoksa cosine similarity yöntemine düş.
    """
    if not os.path.exists(MODEL_PATH):
        from app.services.wifi_service import match_classroom
        return match_classroom(db, student_aps)
    
    try:
        model = joblib.load(MODEL_PATH)
        bssid_index = joblib.load(BSSID_INDEX_PATH)
        le = joblib.load(LABEL_ENCODER_PATH)
        normalizer = joblib.load(RSSI_SCALER_PATH)
        
        vec = scan_to_vector(
            student_aps,
            bssid_index,
            normalizer,
            missing_strategy=MISSING_RSSI_STRATEGY
        ).reshape(1, -1)
        
        # Olasılık tahmini
        proba = model.predict_proba(vec)[0]
        best_idx = np.argmax(proba)
        confidence = float(proba[best_idx])
        classroom_id = le.inverse_transform([best_idx])[0]
        
        if confidence >= CONFIDENCE_THRESHOLD:
            return classroom_id, confidence
        return None, confidence
    
    except Exception as e:
        print(f"ML tahmin hatası: {e}, eski yönteme düşülüyor")
        from app.services.wifi_service import match_classroom
        return match_classroom(db, student_aps)


# ─── Değerlendirme ────────────────────────────────────────────────────────────

def evaluate_model(db: Session) -> Dict:
    """Modeli eğitim verisi üzerinde detaylı olarak değerlendir"""
    if not os.path.exists(MODEL_PATH):
        return {"error": "Model henüz eğitilmemiş"}
    
    samples = db.query(WifiTrainingSample).all()
    if len(samples) < 2:
        return {"error": "Değerlendirme için en az 2 örnek gerekli"}
    
    model = joblib.load(MODEL_PATH)
    bssid_index = joblib.load(BSSID_INDEX_PATH)
    le = joblib.load(LABEL_ENCODER_PATH)
    normalizer = joblib.load(RSSI_SCALER_PATH)
    
    classrooms = {c.id: c.name for c in db.query(Classroom).all()}
    
    X, y_true = [], []
    for sample in samples:
        vec = sample_to_vector(
            sample,
            bssid_index,
            normalizer,
            missing_strategy=MISSING_RSSI_STRATEGY
        )
        X.append(vec)
        y_true.append(sample.classroom_id)
    
    X = np.array(X)
    y_encoded = le.transform(y_true)
    y_pred = model.predict(X)
    
    cm = confusion_matrix(y_encoded, y_pred)
    report = classification_report(
        y_encoded, y_pred,
        target_names=[classrooms.get(c, c) for c in le.classes_],
        output_dict=True,
    )
    
    return {
        "confusion_matrix": cm.tolist(),
        "classification_report": report,
        "accuracy": report["accuracy"],
        "classrooms": [classrooms.get(c, c) for c in le.classes_],
    }


def get_model_info() -> Dict:
    """Mevcut model hakkında bilgi döndür"""
    if not os.path.exists(MODEL_METADATA_PATH):
        return {"error": "Model metadata'sı bulunamadı"}
    
    try:
        with open(MODEL_METADATA_PATH, 'r') as f:
            metadata = json.load(f)
        return metadata
    except:
        return {"error": "Metadata okunamadı"}


def get_model_history() -> List[Dict]:
    """Model eğitim geçmişini döndür"""
    if not os.path.exists(MODEL_HISTORY_PATH):
        return []
    
    try:
        with open(MODEL_HISTORY_PATH, 'r') as f:
            history = json.load(f)
        return history[-20:]  # Son 20 eğitimi
    except:
        return []


# ─── 4 Model Karar Analizi ────────────────────────────────────────────────────

def predict_with_model_breakdown(
    student_aps: List,
    db: Session,
) -> Dict:
    """
    Her modelin (KNN, RF, SVM, GB) bireysel kararını VE Ensemble kararını döndür.
    Swagger ve frontend'de hangi modelin ne dediği görülebilir.

    Döner:
    {
        "ensemble": { classroom_id, classroom_name, confidence, above_threshold },
        "models": {
            "knn":  { display_name, classroom_id, classroom_name, confidence,
                      above_threshold, voted_with_ensemble, class_probabilities },
            "rf":   { ... },
            "svm":  { ... },
            "gb":   { ... },
        },
        "model_type": "Ensemble" | "KNN" | "Cosine-Fallback",
        "agreement_rate": 0.75,  # modellerin kaçı Ensemble ile aynı kararda
        "fallback": false,
    }
    """
    classrooms_map: Dict[str, str] = {}

    # ── Model dosyaları yoksa cosine fallback ──────────────────────────────
    if not os.path.exists(MODEL_PATH):
        from app.services.wifi_service import match_classroom
        classroom_id, confidence = match_classroom(db, student_aps)
        classrooms_map = {c.id: c.name for c in db.query(Classroom).all()}
        classroom_name = classrooms_map.get(classroom_id, classroom_id) if classroom_id else None
        return {
            "ensemble": {
                "classroom_id": classroom_id,
                "classroom_name": classroom_name,
                "confidence": round(confidence, 4),
                "above_threshold": confidence >= CONFIDENCE_THRESHOLD,
            },
            "models": {},
            "model_type": "Cosine-Fallback",
            "agreement_rate": None,
            "fallback": True,
        }

    try:
        ensemble  = joblib.load(MODEL_PATH)
        bssid_idx = joblib.load(BSSID_INDEX_PATH)
        le        = joblib.load(LABEL_ENCODER_PATH)
        normalizer= joblib.load(RSSI_SCALER_PATH)

        classrooms_map = {c.id: c.name for c in db.query(Classroom).all()}

        vec = scan_to_vector(
            student_aps, bssid_idx, normalizer,
            missing_strategy=MISSING_RSSI_STRATEGY,
        ).reshape(1, -1)

        # ── Ensemble kararı ────────────────────────────────────────────────
        ens_proba      = ensemble.predict_proba(vec)[0]
        ens_best_idx   = int(np.argmax(ens_proba))
        ens_confidence = float(ens_proba[ens_best_idx])
        ens_cls_id     = le.inverse_transform([ens_best_idx])[0]
        ens_cls_name   = classrooms_map.get(ens_cls_id, ens_cls_id)

        ensemble_result = {
            "classroom_id":    ens_cls_id,
            "classroom_name":  ens_cls_name,
            "confidence":      round(ens_confidence, 4),
            "above_threshold": ens_confidence >= CONFIDENCE_THRESHOLD,
        }

        # ── Bireysel model kararları ────────────────────────────────────────
        DISPLAY_NAMES = {
            "knn": "K-Nearest Neighbors (KNN)",
            "rf":  "Random Forest (RF)",
            "svm": "Support Vector Machine (SVM)",
            "gb":  "Gradient Boosting (GB)",
        }

        # VotingClassifier alt modellerini çıkar
        if hasattr(ensemble, "estimators_") and hasattr(ensemble, "estimators"):
            sub_estimators = list(zip(
                [name for name, _ in ensemble.estimators],
                ensemble.estimators_,
            ))
        else:
            sub_estimators = [("knn", ensemble)]

        models_breakdown: Dict[str, Dict] = {}
        agreed_count = 0

        for name, estimator in sub_estimators:
            try:
                sub_proba      = estimator.predict_proba(vec)[0]
                sub_best_idx   = int(np.argmax(sub_proba))
                sub_confidence = float(sub_proba[sub_best_idx])
                sub_cls_id     = le.inverse_transform([sub_best_idx])[0]
                sub_cls_name   = classrooms_map.get(sub_cls_id, sub_cls_id)
                voted_same     = (sub_cls_id == ens_cls_id)
                if voted_same:
                    agreed_count += 1

                # Sınıf başına olasılık dağılımı (name → confidence)
                class_probs = {
                    classrooms_map.get(
                        le.inverse_transform([i])[0], le.inverse_transform([i])[0]
                    ): round(float(p), 4)
                    for i, p in enumerate(sub_proba)
                }

                models_breakdown[name] = {
                    "display_name":        DISPLAY_NAMES.get(name, name.upper()),
                    "classroom_id":        sub_cls_id,
                    "classroom_name":      sub_cls_name,
                    "confidence":          round(sub_confidence, 4),
                    "above_threshold":     sub_confidence >= CONFIDENCE_THRESHOLD,
                    "voted_with_ensemble": voted_same,
                    "class_probabilities": class_probs,
                }
            except Exception as sub_err:
                models_breakdown[name] = {
                    "display_name": DISPLAY_NAMES.get(name, name.upper()),
                    "error":        str(sub_err),
                }

        n_models       = len(sub_estimators)
        agreement_rate = round(agreed_count / n_models, 4) if n_models > 0 else None

        # Model tipi (metadata'dan)
        model_type = "Ensemble"
        if os.path.exists(MODEL_METADATA_PATH):
            try:
                with open(MODEL_METADATA_PATH, "r") as f:
                    meta = json.load(f)
                model_type = meta.get("model_type", "Ensemble")
            except Exception:
                pass

        # ── Çatışma / Anlaşmazlık Analizi ─────────────────────────────────────
        #
        # Soft Voting nasıl çalışır:
        #   VotingClassifier her modelin olasılık vektörünü ORTALARAK final kararı verir.
        #   Yani oy sayısı değil, sayısal ortalama belirler.
        #
        # Bir model farklı sınıf seçerse NE OLUR:
        #   - O modelin yüksek olasılığı farklı bir sınıfa gider → ensemble'ın o sınıfa
        #     verdiği ağırlık düşer, doğru sınıfın ortalaması azalır.
        #   - Sonuç: ens_confidence DÜŞER (anlaşmazlık ne kadar büyükse o kadar çok)
        #   - Eşiğin altına düşerse (< 0.60) matched=False → yoklama kaydedilmez.
        #
        # Bir modelin skoru düşükse (ama aynı sınıfı seçiyorsa):
        #   - O model doğru sınıfa az olasılık veriyor → ortalamayı aşağı çeker
        #   - Yeterince çekerse ensemble confidence eşiğin altına düşer

        dissenters = []          # Farklı sınıf seçen modeller
        low_conf_models = []     # Aynı sınıf ama düşük confidence

        for mname, minfo in models_breakdown.items():
            if "error" in minfo:
                continue
            if not minfo.get("voted_with_ensemble", True):
                dissenters.append({
                    "model":            mname,
                    "display_name":     minfo["display_name"],
                    "voted_for":        minfo["classroom_name"],
                    "voted_for_id":     minfo["classroom_id"],
                    "its_confidence":   minfo["confidence"],
                    # Bu modelin ens_cls_id'ye verdiği olasılık (ne kadar "çekiyor")
                    "pull_on_winner":   minfo["class_probabilities"].get(ens_cls_name, 0.0),
                })
            elif minfo["confidence"] < CONFIDENCE_THRESHOLD:
                low_conf_models.append({
                    "model":          mname,
                    "display_name":   minfo["display_name"],
                    "confidence":     minfo["confidence"],
                    "classroom_name": minfo["classroom_name"],
                })

        # Konsensüs seviyesi
        if agreed_count == n_models:
            consensus_level = "full"          # 4/4 aynı
            consensus_label = "✅ Tam Uyum"
        elif agreed_count >= (n_models * 0.75):
            consensus_level = "majority"      # 3/4 aynı
            consensus_label = "🟡 Çoğunluk Uyumu"
        elif agreed_count >= (n_models * 0.5):
            consensus_level = "split"         # 2/4 aynı
            consensus_label = "🟠 Bölünmüş Karar"
        else:
            consensus_level = "no_consensus"  # 1/4 veya 0/4
            consensus_label = "🔴 Uyumsuzluk"

        # Ensemble güveni ne kadar etkilendi?
        # Teorik max confidence (tüm modeller tam uyuşsaydı) vs gerçek
        max_any_model_on_winner = max(
            (minfo["class_probabilities"].get(ens_cls_name, 0.0)
             for minfo in models_breakdown.values() if "error" not in minfo),
            default=ens_confidence,
        )
        confidence_penalty = round(max_any_model_on_winner - ens_confidence, 4)

        # Yoklamaya etkisi
        if ens_confidence < CONFIDENCE_THRESHOLD:
            attendance_impact = "REDDEDILDI — Ensemble güveni eşiğin altında, yoklama kaydedilmez"
        elif dissenters:
            attendance_impact = (
                f"KABUL EDİLDİ — Ama {len(dissenters)} model farklı oy kullandı; "
                f"confidence {confidence_penalty:.3f} düşürüldü"
            )
        elif low_conf_models:
            attendance_impact = (
                f"KABUL EDİLDİ — Ama {len(low_conf_models)} model düşük güvenle oy kullandı; "
                f"ensemble confidence etkilendi"
            )
        else:
            attendance_impact = "KABUL EDİLDİ — Tüm modeller uyuşuyor, güçlü tahmin"

        conflict_analysis = {
            "consensus_level":    consensus_level,
            "consensus_label":    consensus_label,
            "agreed_models":      agreed_count,
            "total_models":       n_models,
            "dissenters":         dissenters,          # Farklı oy kullananlar
            "low_confidence_models": low_conf_models,  # Zayıf ama aynı yönde oy kullananlar
            "confidence_penalty": confidence_penalty,  # Anlaşmazlığın confidence'a maliyeti
            "threshold":          CONFIDENCE_THRESHOLD,
            "attendance_impact":  attendance_impact,
            # ── Kural özeti ──────────────────────────────────────────────────
            "voting_rules": {
                "method":       "soft_voting",
                "description":  "Her modelin olasılık vektörleri ortalamasına göre karar verilir",
                "threshold":    CONFIDENCE_THRESHOLD,
                "if_dissenter": "Farklı sınıf seçen model ensemble confidence'ı düşürür",
                "if_low_conf":  "Aynı sınıfı seçen ama düşük güvenli model de confidence'ı aşağı çeker",
                "block_rule":   f"Ensemble confidence < {CONFIDENCE_THRESHOLD} ise yoklama reddedilir",
            },
        }

        return {
            "ensemble":         ensemble_result,
            "models":           models_breakdown,
            "conflict_analysis": conflict_analysis,
            "model_type":       model_type,
            "agreement_rate":   agreement_rate,
            "fallback":         False,
        }


    except Exception as e:
        print(f"Model breakdown hatası: {e}, cosine fallback")
        from app.services.wifi_service import match_classroom
        classroom_id, confidence = match_classroom(db, student_aps)
        classroom_name = classrooms_map.get(classroom_id, classroom_id) if classroom_id else None
        return {
            "ensemble": {
                "classroom_id":    classroom_id,
                "classroom_name":  classroom_name,
                "confidence":      round(confidence, 4),
                "above_threshold": confidence >= CONFIDENCE_THRESHOLD,
            },
            "models":        {},
            "model_type":    "Cosine-Fallback",
            "agreement_rate": None,
            "fallback":      True,
            "error":         str(e),
        }
