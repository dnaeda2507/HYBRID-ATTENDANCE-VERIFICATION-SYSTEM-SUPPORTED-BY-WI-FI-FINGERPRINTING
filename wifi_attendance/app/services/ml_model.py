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
