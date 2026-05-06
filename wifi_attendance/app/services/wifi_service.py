from typing import List, Optional, Dict, Tuple
from sqlalchemy.orm import Session
import numpy as np
from collections import defaultdict, Counter
from sklearn.preprocessing import StandardScaler, MinMaxScaler

from app.models.wifi_models import (
    WifiTrainingSample,
    WifiTrainingAccessPoint,
    StudentWifiAccessPoint,
    Classroom,
)

# ─── Konfigürasyon ───────────────────────────────────────────────────────────

CONFIDENCE_THRESHOLD = 0.60
MIN_COMMON_APS_BASE = 2  # Minimum başlangıç değeri
OVERLAP_WEIGHT_STRATEGY = "adaptive"  # "fixed" veya "adaptive"
FIXED_OVERLAP_WEIGHT = 1.0  # fixed stratejisi için

# RSSI Normalizasyon Stratejisi
RSSI_NORMALIZER_STRATEGY = "adaptive"  # "adaptive" veya "simple"


# ─── Geliştirilmiş RSSI Normalizasyonu ────────────────────────────────────────

class RSSSINormalizerSimple:
    """Basit, sabit formüla dayalı RSSI normalizasyonu"""
    
    @staticmethod
    def normalize(rssi: int) -> float:
        # -100dBm = 0, -30dBm = 1
        return max(0.0, min(1.0, (rssi + 100) / 70.0))


class RSSINormalizerAdaptive:
    """Veriye dayalı, adaptif RSSI normalizasyonu"""
    
    def __init__(self, strategy: str = "minmax"):
        self.strategy = strategy
        self.scaler = MinMaxScaler(feature_range=(0, 1)) if strategy == "minmax" else StandardScaler()
        self.fitted = False
        self.mean = None
        self.std = None
    
    def fit(self, rssi_values: List[int]) -> None:
        """RSSI değerlerinden istatistikleri öğren"""
        if len(rssi_values) > 0:
            rssi_array = np.array(rssi_values).reshape(-1, 1)
            
            if self.strategy == "minmax":
                self.scaler.fit(rssi_array)
            else:  # zscore
                self.mean = np.mean(rssi_values)
                self.std = np.std(rssi_values)
            
            self.fitted = True
    
    def normalize(self, rssi: int) -> float:
        """RSSI değerini normalize et"""
        if not self.fitted:
            return max(0.0, min(1.0, (rssi + 100) / 70.0))
        
        try:
            if self.strategy == "minmax":
                normalized = self.scaler.transform([[rssi]])[0][0]
            else:  # zscore
                normalized = (rssi - self.mean) / (self.std + 1e-10)
            
            return float(np.clip(normalized, 0.0, 1.0))
        except:
            return max(0.0, min(1.0, (rssi + 100) / 70.0))


def get_rssi_normalizer(db: Session) -> RSSINormalizerAdaptive:
    """Tüm eğitim verisinden RSSI normalizer'ı oluştur"""
    samples = db.query(WifiTrainingSample).all()
    rssi_values = []
    
    for sample in samples:
        for ap in sample.access_points:
            rssi_values.append(ap.rssi)
    
    normalizer = RSSINormalizerAdaptive(strategy="minmax")
    if rssi_values:
        normalizer.fit(rssi_values)
    
    return normalizer


def get_adaptive_min_common_aps(db: Session, default: int = 2) -> int:
    """
    Ortamın özelliklerine göre MIN_COMMON_APS'ı dinamik olarak hesapla.
    
    - Ağ yoğunluğu: Her öğrencinin ortalama gördüğü AP sayısı
    - Parmak izi benzerliği: Aynı sınıftaki örneklerin benzerlikleri
    """
    samples = db.query(WifiTrainingSample).all()
    
    if len(samples) < 5:
        return default
    
    # Her sample'da görülen AP sayılarını topla
    ap_counts = [len(sample.access_points) for sample in samples]
    avg_ap_count = np.mean(ap_counts)
    
    # Görülen AP sayılarının varyansı
    ap_variance = np.var(ap_counts)
    
    # Dinamik hesaplama:
    # - Yüksek varyans = çeşitli AP görüş = MIN_COMMON_APS arttır
    # - Düşük ortalama = az AP = MIN_COMMON_APS azalt
    
    if ap_variance > 20 and avg_ap_count > 8:
        # Heterojen ortam, daha sıkı kriter
        return max(3, min(4, int(avg_ap_count * 0.4)))
    elif avg_ap_count > 10:
        return max(2, int(avg_ap_count * 0.3))
    else:
        return default


def build_classroom_fingerprints(
    db: Session,
    classroom_id: str,
    normalizer: Optional[RSSINormalizerAdaptive] = None,
) -> Dict[str, float]:
    """
    Bir sınıf için Wi-Fi parmak izini oluştur.
    
    Her BSSID için ortalama (normalize edilmiş) RSSI değerini hesapla.
    """
    samples = (
        db.query(WifiTrainingSample)
        .filter(WifiTrainingSample.classroom_id == classroom_id)
        .all()
    )

    if not samples:
        return {}
    
    if normalizer is None:
        normalizer = RSSINormalizerSimple()

    bssid_rssi_lists: Dict[str, List[float]] = defaultdict(list)

    for sample in samples:
        for ap in sample.access_points:
            if isinstance(normalizer, RSSINormalizerAdaptive):
                rssi_normalized = normalizer.normalize(ap.rssi)
            else:
                rssi_normalized = normalizer.normalize(ap.rssi)
            
            bssid_rssi_lists[ap.bssid].append(rssi_normalized)

    return {bssid: float(np.mean(values)) for bssid, values in bssid_rssi_lists.items()}


def cosine_similarity(
    vec_a: Dict[str, float], 
    vec_b: Dict[str, float],
    min_common_aps: int = 2,
) -> float:
    """
    İki RSSI vektörü arasında cosine benzerliğini hesapla.
    
    overlap_weight: Ortak BSSID'lerin toplam BSSID'lere oranı
    """
    common = set(vec_a.keys()) & set(vec_b.keys())

    if len(common) < min_common_aps:
        return 0.0

    a_vals = np.array([vec_a[b] for b in common])
    b_vals = np.array([vec_b[b] for b in common])

    dot = np.dot(a_vals, b_vals)
    norm = np.linalg.norm(a_vals) * np.linalg.norm(b_vals)

    if norm == 0:
        return 0.0

    raw_sim = dot / norm
    
    # Overlap ağırlıklandırması
    all_bssids = set(vec_a.keys()) | set(vec_b.keys())
    overlap_ratio = len(common) / len(all_bssids)
    
    # Adaptif ağırlıklandırma: daha fazla ortak BSSID'e daha yüksek ağırlık
    if OVERLAP_WEIGHT_STRATEGY == "adaptive":
        # 30% ortak = 0.9, 50% ortak = 1.0, 70% ortak = 1.0
        overlap_weight = min(1.0, 0.5 + overlap_ratio)
    else:  # fixed
        overlap_weight = FIXED_OVERLAP_WEIGHT

    return float(raw_sim * overlap_weight)


def match_classroom(
    db: Session,
    student_aps: List[StudentWifiAccessPoint],
    session_classroom_id: Optional[str] = None,
    normalizer: Optional[RSSINormalizerAdaptive] = None,
) -> Tuple[Optional[str], float]:
    """
    Öğrencinin Wi-Fi taramasıyla en uyumlu sınıfı bul.
    """
    
    if normalizer is None:
        if RSSI_NORMALIZER_STRATEGY == "adaptive":
            normalizer = get_rssi_normalizer(db)
        else:
            normalizer = RSSINormalizerSimple()

    student_vec: Dict[str, float] = {}
    for ap in student_aps:
        if isinstance(normalizer, RSSINormalizerAdaptive):
            student_vec[ap.bssid] = normalizer.normalize(ap.rssi)
        else:
            student_vec[ap.bssid] = normalizer.normalize(ap.rssi)

    if not student_vec:
        return None, 0.0

    classrooms = db.query(Classroom).all()

    if not classrooms:
        return None, 0.0

    # Dinamik MIN_COMMON_APS hesapla
    min_common_aps = get_adaptive_min_common_aps(db, default=MIN_COMMON_APS_BASE)

    best_classroom_id = None
    best_score = 0.0

    # Session sınıfı varsa önce onu kontrol et
    if session_classroom_id:
        fp = build_classroom_fingerprints(db, session_classroom_id, normalizer)
        score = cosine_similarity(student_vec, fp, min_common_aps)
        if score >= CONFIDENCE_THRESHOLD:
            return session_classroom_id, score

    # Tüm sınıfları tara
    for classroom in classrooms:
        fp = build_classroom_fingerprints(db, classroom.id, normalizer)
        if not fp:
            continue
        score = cosine_similarity(student_vec, fp, min_common_aps)
        if score > best_score:
            best_score = score
            best_classroom_id = classroom.id

    if best_score >= CONFIDENCE_THRESHOLD:
        return best_classroom_id, best_score

    return None, best_score
