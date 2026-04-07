from typing import List, Optional, Dict, Tuple
from sqlalchemy.orm import Session
import numpy as np
from collections import defaultdict

from app.models.wifi_models import (
    WifiTrainingSample,
    WifiTrainingAccessPoint,
    StudentWifiAccessPoint,
    Classroom,
)

CONFIDENCE_THRESHOLD = 0.60
MIN_COMMON_APS = 2


def normalize_rssi(rssi: int) -> float:
    return max(0.0, min(1.0, (rssi + 100) / 70.0))


def build_classroom_fingerprints(
    db: Session,
    classroom_id: str
) -> Dict[str, float]:
    samples = (
        db.query(WifiTrainingSample)
        .filter(WifiTrainingSample.classroom_id == classroom_id)
        .all()
    )

    if not samples:
        return {}

    bssid_rssi_lists: Dict[str, List[float]] = defaultdict(list)

    for sample in samples:
        for ap in sample.access_points:
            bssid_rssi_lists[ap.bssid].append(normalize_rssi(ap.rssi))

    return {bssid: float(np.mean(values)) for bssid, values in bssid_rssi_lists.items()}


def cosine_similarity(vec_a: Dict[str, float], vec_b: Dict[str, float]) -> float:
    common = set(vec_a.keys()) & set(vec_b.keys())

    if len(common) < MIN_COMMON_APS:
        return 0.0

    a_vals = np.array([vec_a[b] for b in common])
    b_vals = np.array([vec_b[b] for b in common])

    dot  = np.dot(a_vals, b_vals)
    norm = np.linalg.norm(a_vals) * np.linalg.norm(b_vals)

    if norm == 0:
        return 0.0

    raw_sim = dot / norm
    all_bssids = set(vec_a.keys()) | set(vec_b.keys())
    overlap_weight = len(common) / len(all_bssids)

    return float(raw_sim * overlap_weight)


def match_classroom(
    db: Session,
    student_aps: List[StudentWifiAccessPoint],
    session_classroom_id: Optional[str] = None,
) -> Tuple[Optional[str], float]:

    student_vec: Dict[str, float] = {
        ap.bssid: normalize_rssi(ap.rssi) for ap in student_aps
    }

    if not student_vec:
        return None, 0.0

    classrooms = db.query(Classroom).all()

    if not classrooms:
        return None, 0.0

    best_classroom_id = None
    best_score = 0.0

    if session_classroom_id:
        fp = build_classroom_fingerprints(db, session_classroom_id)
        score = cosine_similarity(student_vec, fp)
        if score >= CONFIDENCE_THRESHOLD:
            return session_classroom_id, score

    for classroom in classrooms:
        fp = build_classroom_fingerprints(db, classroom.id)
        if not fp:
            continue
        score = cosine_similarity(student_vec, fp)
        if score > best_score:
            best_score = score
            best_classroom_id = classroom.id

    if best_score >= CONFIDENCE_THRESHOLD:
        return best_classroom_id, best_score

    return None, best_score
