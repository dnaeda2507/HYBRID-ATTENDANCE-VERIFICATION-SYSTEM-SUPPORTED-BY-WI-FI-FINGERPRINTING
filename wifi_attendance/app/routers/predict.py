from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session
from typing import List, Optional, Dict, Any
from pydantic import BaseModel
from datetime import datetime

from app.core.database import get_db
from app.core.security import verify_internal_token

router = APIRouter(prefix="/predict", tags=["Prediction"])


class AccessPointInput(BaseModel):
    bssid: str
    rssi: int
    ssid: Optional[str] = None
    frequency: Optional[int] = None


class PredictRequest(BaseModel):
    access_points: List[AccessPointInput]
    session_id: Optional[int] = None
    student_id: Optional[str] = None


class SecurityMetadata(BaseModel):
    ip_address: Optional[str] = None
    ip_valid: bool = False
    ip_in_network: bool = False
    bssid_count: int = 0
    suspicious_bssids: int = 0
    security_score: float = 0.0
    security_status: str = "unknown"
    risk_factors: List[str] = []


class PredictResponse(BaseModel):
    matched: bool
    classroom_name: Optional[str] = None
    classroom_id: Optional[str] = None
    confidence: float
    message: str
    security: Optional[SecurityMetadata] = None  # Yeni: Güvenlik metadata'sı
    is_suspicious: bool = False  # Anlaşmazlık varsa true


@router.post("", response_model=PredictResponse, summary="WiFi konumu tahmin et (güvenlik ile)")
def predict_location(
    payload: PredictRequest,
    request: Request,
    db: Session = Depends(get_db),
    auto_retrain: bool = False,
    _=Depends(verify_internal_token),
):
    """
    C# backend bu endpoint'i çağırır.
    
    Özellikler:
    - ML model + Fallback (cosine similarity)
    - ✅ IP Doğrulama (Okul ağında mı?)
    - ✅ BSSID Whitelist Kontrol
    - ✅ Security Scoring
    - ✅ Şüpheli Aktivite Logging
    - Geliştirilmiş RSSI normalizasyonu
    - Model metadata'sı ile tahmin
    
    Öğrencinin WiFi taramasını alır, hangi derslikte olduğunu tahmin eder.
    Yoklama kaydetmez — sadece tahmin döner.
    """
    from app.services.ml_model import predict_classroom, get_model_info
    from app.services.model_retraining import auto_retrain_if_needed
    from app.services.security_validation import (
        validate_ip_address,
        log_prediction_audit,
    )

    # Client IP'sini çek
    client_ip = getattr(request.state, "client_ip", None)
    if not client_ip:
        client_ip = request.client.host if request.client else "unknown"

    # AP objelerini oluştur
    ap_objects = [
        type('AP', (), {
            'bssid': ap.bssid,
            'ssid': ap.ssid,
            'rssi': ap.rssi,
            'frequency': ap.frequency,
            'channel': None,
        })()
        for ap in payload.access_points
    ]

    if not ap_objects:
        return PredictResponse(
            matched=False,
            confidence=0.0,
            message="AP listesi boş",
            security=SecurityMetadata(),
        )

    # ─── Security Validation ────────────────────────────────────────────────

    student_id = payload.student_id or "unknown"
    
    # 1. IP Doğrulama
    ip_valid, ip_metadata = validate_ip_address(db, student_id, client_ip)
    
    # 2. BSSID'leri çek
    bssids = [ap.bssid for ap in payload.access_points]

    # ─── ML Prediction ────────────────────────────────────────────────────────

    from app.services.ml_model import predict_with_model_breakdown
    
    # AP objeleri listesini kullan (zaten yukarıda oluşturuldu varsayalım)
    ap_objects = [
        type('AP', (), {
            'bssid': ap.bssid,
            'ssid': ap.ssid,
            'rssi': ap.rssi,
            'frequency': ap.frequency,
            'channel': None,
        })()
        for ap in payload.access_points
    ]

    breakdown = predict_with_model_breakdown(ap_objects, db)
    matched_classroom_id = breakdown["ensemble"]["classroom_id"]
    confidence = breakdown["ensemble"]["confidence"]
    agreement_rate = breakdown.get("agreement_rate")
    
    # Anlaşmazlık durumu (yarıdan az veya eşit uyum varsa şüpheli)
    is_suspicious_model = False
    if agreement_rate is not None and agreement_rate <= 0.5:
        is_suspicious_model = True
    
    # Model metadata'sı ekle
    model_info = get_model_info()
    
    # Otomatik retrain kontrol et (opsiyonel)
    if auto_retrain:
        retrain_check = auto_retrain_if_needed(db)
        if retrain_check["should_retrain"]:
            model_info = get_model_info()  # Güncellenmiş metadata

    # ─── Security Audit Logging ─────────────────────────────────────────────
    
    audit = log_prediction_audit(
        db=db,
        student_id=student_id,
        ip_address=client_ip,
        ip_valid=ip_valid,
        ip_metadata=ip_metadata,
        bssids=bssids,
        matched_classroom_id=matched_classroom_id,
        confidence_score=confidence,
    )

    # ─── Risk Profile Update (Tarihsel Risk Analizi) ────────────────────────
    
    from app.services.risk_analysis import update_or_create_risk_profile
    
    try:
        risk_profile = update_or_create_risk_profile(db, student_id)
        
        # Eğer öğrenci flaglandıysa warning ekle
        if risk_profile.is_flagged:
            audit_message = f"{audit.security_status.upper()} [RISK: {risk_profile.risk_level.upper()}]"
        else:
            audit_message = f"{audit.security_status.upper()}"
    except Exception as e:
        print(f"Risk profili güncelleme hatası: {e}")
        audit_message = f"{audit.security_status.upper()}"

    # ─── Response Oluştur ───────────────────────────────────────────────────

    security_meta = SecurityMetadata(
        ip_address=client_ip,
        ip_valid=ip_metadata.get("in_network", False),
        ip_in_network=ip_metadata.get("in_network", False),
        bssid_count=audit.bssid_count,
        suspicious_bssids=audit.suspicious_bssids,
        security_score=round(audit.security_score, 3),
        security_status=audit.security_status,
        risk_factors=audit.risk_factors.split(", ") if audit.risk_factors else [],
    )

    if matched_classroom_id and audit.is_valid_prediction:
        from app.models.wifi_models import Classroom
        classroom = db.query(Classroom).filter(
            Classroom.id == matched_classroom_id
        ).first()
        return PredictResponse(
            matched=True,
            classroom_name=classroom.name if classroom else matched_classroom_id,
            classroom_id=matched_classroom_id,
            confidence=round(confidence, 3),
            message=f"✓ Doğrulandı [Model: {model_info.get('model_type', 'KNN')} | Sec: {audit_message}]",
            security=security_meta,
            is_suspicious=is_suspicious_model,
        )
    else:
        confidence_threshold = model_info.get('confidence_threshold', 0.60)
        reason = "IP doğrulanamadı" if not ip_valid else \
                 "ML güveni yetersiz" if confidence < confidence_threshold else \
                 "Bilinmeyen BSSID'ler" if audit.suspicious_bssids > 0 else \
                 "Bilinmeyen sebep"
        
        return PredictResponse(
            matched=False,
            confidence=round(confidence, 3),
            message=f"✗ Doğrulanmadı [{reason}] (Sec: {audit_message})",
            security=security_meta,
            is_suspicious=is_suspicious_model,
        )


# ─── 4 Model Karar Görünürlüğü ───────────────────────────────────────────────

class ModelDecision(BaseModel):
    display_name: str
    classroom_id: Optional[str] = None
    classroom_name: Optional[str] = None
    confidence: float
    above_threshold: bool
    voted_with_ensemble: bool
    class_probabilities: Dict[str, float] = {}
    error: Optional[str] = None


class ModelBreakdownResponse(BaseModel):
    ensemble: Dict[str, Any]
    models: Dict[str, Any]
    conflict_analysis: Optional[Dict[str, Any]] = None  # Anlaşmazlık & konsensüs analizi
    model_type: str
    agreement_rate: Optional[float] = None
    fallback: bool
    error: Optional[str] = None


@router.post(
    "/breakdown",
    response_model=ModelBreakdownResponse,
    summary="4 Modelin Bireysel Kararlarını Gör (KNN / RF / SVM / GB + Ensemble)",
    description="""
Her tahmin için KNN, Random Forest, SVM ve Gradient Boosting modellerinin
bireysel kararlarını gösterir.

**Alanlar:**
- `ensemble`: Tüm modellerin oylamasıyla oluşan final karar
- `models.knn / rf / svm / gb`: Her modelin ayrı kararı, confidence'ı ve hangi sınıfa oy verdiği
- `class_probabilities`: Her modelin derslik başına verdiği olasılık dağılımı
- `agreement_rate`: Modellerin yüzde kaçı aynı kararda (1.0 = tüm modeller uyuşuyor)
- `voted_with_ensemble`: Bu model final kararla aynı mı oyladı?
""",
)
def predict_breakdown(
    payload: PredictRequest,
    request: Request,
    db: Session = Depends(get_db),
    _=Depends(verify_internal_token),
):
    from app.services.ml_model import predict_with_model_breakdown

    # AP objelerini oluştur
    ap_objects = [
        type('AP', (), {
            'bssid': ap.bssid,
            'ssid': ap.ssid,
            'rssi': ap.rssi,
            'frequency': ap.frequency,
            'channel': None,
        })()
        for ap in payload.access_points
    ]

    if not ap_objects:
        return ModelBreakdownResponse(
            ensemble={"classroom_id": None, "confidence": 0.0, "above_threshold": False},
            models={},
            model_type="N/A",
            agreement_rate=None,
            fallback=True,
            error="AP listesi boş",
        )

    result = predict_with_model_breakdown(ap_objects, db)
    return ModelBreakdownResponse(**result)
