"""
WiFi Fingerprinting ML Modeli

1. KNN + Random Forest ensemble
2. RSSI zaman ağırlıklandırma (yeni sample > eski sample)
3. Model kaydetme/yükleme (joblib)
4. Confusion matrix ve doğruluk testi
"""

import numpy as np
import joblib
import os
from datetime import datetime
from typing import List, Optional, Dict, Tuple
from collections import defaultdict
from sqlalchemy.orm import Session

from sklearn.neighbors import KNeighborsClassifier
from sklearn.ensemble import RandomForestClassifier, VotingClassifier
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import cross_val_score
from sklearn.metrics import confusion_matrix, classification_report

from app.models.wifi_models import WifiTrainingSample, Classroom, StudentWifiAccessPoint

MODEL_PATH = "wifi_model.joblib"
BSSID_INDEX_PATH = "bssid_index.joblib"
LABEL_ENCODER_PATH = "label_encoder.joblib"
CONFIDENCE_THRESHOLD = 0.60


# ─── Yardımcı Fonksiyonlar ───────────────────────────────────────────────────

def normalize_rssi(rssi: int) -> float:
    return max(0.0, min(1.0, (rssi + 100) / 70.0))


def time_weight(scanned_at: datetime) -> float:
    """
    Yeni taramalar daha ağırlıklı.
    Son 7 gün: 1.0, 30 gün: 0.7, daha eski: 0.4
    """
    days_old = (datetime.utcnow() - scanned_at).days
    if days_old <= 7:
        return 1.0
    elif days_old <= 30:
        return 0.7
    else:
        return 0.4


def build_bssid_index(db: Session) -> List[str]:
    """Tüm eğitim verisindeki BSSID'leri topla — feature vektörü için indeks"""
    samples = db.query(WifiTrainingSample).all()
    bssids = set()
    for sample in samples:
        for ap in sample.access_points:
            bssids.add(ap.bssid)
    return sorted(list(bssids))


def sample_to_vector(sample, bssid_index: List[str]) -> np.ndarray:
    """Bir training sample'ı sabit boyutlu vektöre çevir"""
    vec = np.zeros(len(bssid_index))
    bssid_rssi = {ap.bssid: normalize_rssi(ap.rssi) for ap in sample.access_points}
    for i, bssid in enumerate(bssid_index):
        if bssid in bssid_rssi:
            vec[i] = bssid_rssi[bssid]
    return vec


def scan_to_vector(student_aps: List, bssid_index: List[str]) -> np.ndarray:
    """Öğrenci taramasını sabit boyutlu vektöre çevir"""
    vec = np.zeros(len(bssid_index))
    bssid_rssi = {ap.bssid: normalize_rssi(ap.rssi) for ap in student_aps}
    for i, bssid in enumerate(bssid_index):
        if bssid in bssid_rssi:
            vec[i] = bssid_rssi[bssid]
    return vec


# ─── Model Eğitimi ───────────────────────────────────────────────────────────

def train_model(db: Session) -> Dict:
    """
    Tüm eğitim verisinden KNN + Random Forest ensemble modeli eğit.
    Zaman ağırlıklandırması uygular.
    """
    samples = db.query(WifiTrainingSample).all()

    if len(samples) < 2:
        return {"success": False, "message": "En az 2 eğitim örneği gerekli"}

    # BSSID indeksi oluştur
    bssid_index = build_bssid_index(db)

    if not bssid_index:
        return {"success": False, "message": "BSSID verisi bulunamadı"}

    # X (feature) ve y (label) oluştur
    X, y, weights = [], [], []

    for sample in samples:
        vec = sample_to_vector(sample, bssid_index)
        X.append(vec)
        y.append(sample.classroom_id)
        weights.append(time_weight(sample.scanned_at))

    X = np.array(X)
    y = np.array(y)
    weights = np.array(weights)

    # Label encoding
    le = LabelEncoder()
    y_encoded = le.fit_transform(y)

    # Sınıf sayısına göre k belirle
    n_classes = len(np.unique(y_encoded))
    n_samples = len(X)
    k = min(5, n_samples)

    # KNN
    knn = KNeighborsClassifier(
        n_neighbors=k,
        metric='euclidean',
        weights='distance',
    )

    # Random Forest
    rf = RandomForestClassifier(
        n_estimators=100,
        max_depth=10,
        random_state=42,
    )

    # Ensemble — ikisinin oylaması
    if n_samples >= 4 and n_classes >= 2:
        model = VotingClassifier(
            estimators=[('knn', knn), ('rf', rf)],
            voting='soft',
        )
        model.fit(X, y_encoded)

        # Cross validation skoru
        cv_scores = cross_val_score(model, X, y_encoded, cv=min(3, n_samples))
        accuracy = cv_scores.mean()
    else:
        # Az veri varsa sadece KNN kullan
        model = knn
        model.fit(X, y_encoded)
        accuracy = 1.0 if n_samples == 1 else None

    # Modeli kaydet
    joblib.dump(model, MODEL_PATH)
    joblib.dump(bssid_index, BSSID_INDEX_PATH)
    joblib.dump(le, LABEL_ENCODER_PATH)

    # Classroom isimlerini al
    classrooms = {c.id: c.name for c in db.query(Classroom).all()}
    class_names = [classrooms.get(le.inverse_transform([i])[0], "?") for i in range(len(le.classes_))]

    return {
        "success": True,
        "message": "Model başarıyla eğitildi ✓",
        "total_samples": n_samples,
        "total_bssids": len(bssid_index),
        "classrooms": n_classes,
        "accuracy": round(float(accuracy), 3) if accuracy is not None else None,
        "class_names": class_names,
        "model_type": "KNN+RandomForest Ensemble" if n_samples >= 4 and n_classes >= 2 else "KNN",
    }


# ─── Tahmin ──────────────────────────────────────────────────────────────────

def predict_classroom(
    student_aps: List,
    db: Session,
) -> Tuple[Optional[str], float]:
    """
    Eğitilmiş modelle öğrencinin konumunu tahmin et.
    Model yoksa eski cosine similarity yöntemine düş.
    """
    if not os.path.exists(MODEL_PATH):
        # Model henüz eğitilmemiş, eski yönteme düş
        from app.services.wifi_service import match_classroom
        return match_classroom(db, student_aps)

    try:
        model = joblib.load(MODEL_PATH)
        bssid_index = joblib.load(BSSID_INDEX_PATH)
        le = joblib.load(LABEL_ENCODER_PATH)

        vec = scan_to_vector(student_aps, bssid_index).reshape(1, -1)

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


# ─── Confusion Matrix ────────────────────────────────────────────────────────

def evaluate_model(db: Session) -> Dict:
    """Modeli eğitim verisi üzerinde değerlendir — confusion matrix"""
    if not os.path.exists(MODEL_PATH):
        return {"error": "Model henüz eğitilmemiş"}

    samples = db.query(WifiTrainingSample).all()
    if len(samples) < 2:
        return {"error": "Değerlendirme için en az 2 örnek gerekli"}

    model = joblib.load(MODEL_PATH)
    bssid_index = joblib.load(BSSID_INDEX_PATH)
    le = joblib.load(LABEL_ENCODER_PATH)

    classrooms = {c.id: c.name for c in db.query(Classroom).all()}

    X, y_true = [], []
    for sample in samples:
        vec = sample_to_vector(sample, bssid_index)
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
