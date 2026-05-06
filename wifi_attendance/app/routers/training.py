from fastapi import APIRouter, Depends, UploadFile
from sqlalchemy.orm import Session
from typing import List, Optional
from pydantic import BaseModel

from app.core.database import get_db
from app.core.security import verify_internal_token, get_current_user, require_role
from app.models.wifi_models import Classroom, WifiTrainingSample, WifiTrainingAccessPoint

router = APIRouter(prefix="/training", tags=["Training"])


class ClassroomCreate(BaseModel):
    name: str
    building: Optional[str] = None
    floor: Optional[int] = None
    capacity: Optional[int] = None


class APInput(BaseModel):
    bssid: str
    rssi: int
    ssid: Optional[str] = None
    frequency: Optional[int] = None


class TrainingSampleCreate(BaseModel):
    classroom_id: str
    access_points: List[APInput]
    notes: Optional[str] = None


@router.post("/classrooms", summary="Derslik ekle")
def create_classroom(
    payload: ClassroomCreate,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    classroom = Classroom(**payload.dict())
    db.add(classroom)
    db.commit()
    db.refresh(classroom)
    return classroom


@router.get("/classrooms", summary="Tüm derslikler")
def list_classrooms(db: Session = Depends(get_db), _=Depends(get_current_user)):
    return db.query(Classroom).all()


@router.post("/samples", summary="Eğitim örneği ekle")
def add_sample(
    payload: TrainingSampleCreate,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    from app.models.wifi_models import WifiTrainingAccessPoint
    sample = WifiTrainingSample(
        classroom_id=payload.classroom_id,
        notes=payload.notes,  # scanned_by alanı modelde yok — kaldırıldı
    )
    db.add(sample)
    db.flush()
    for ap in payload.access_points:
        db.add(WifiTrainingAccessPoint(
            training_sample_id=sample.id,  # ✅ Düzeltildi: sample_id → training_sample_id
            bssid=ap.bssid,
            ssid=ap.ssid,
            rssi=ap.rssi,
            frequency=ap.frequency,
        ))
    db.commit()
    db.refresh(sample)
    return {"id": sample.id, "classroom_id": sample.classroom_id, "message": "Örnek eklendi ✓"}


@router.post("/upload-csv", summary="CSV ile toplu eğitim verisi yükle (auto-retrain)")
async def upload_csv(
    file: UploadFile,
    format: str = "custom",
    auto_retrain: bool = True,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """
    CSV ile toplu veri yükle ve opsiyonel olarak modeli otomatik retrain et.
    
    Parametreler:
    - auto_retrain: True = veri yüklendikten sonra modeli otomatik retrain et
    """
    from app.services.csv_import import import_from_custom_csv, import_from_simple_csv
    from app.services.model_retraining import auto_retrain_if_needed
    
    content = (await file.read()).decode("utf-8")
    if format == "simple":
        result = import_from_simple_csv(content, db, scanned_by=current_user["id"])
    else:
        result = import_from_custom_csv(content, db, scanned_by=current_user["id"])
    
    # Auto-whitelist detected BSSIDs
    if result.get("success") and "detected_bssids" in result:
        from app.services.security_validation import add_bssid_to_whitelist
        for bssid in result["detected_bssids"]:
            try:
                add_bssid_to_whitelist(
                    db=db,
                    bssid=bssid,
                    ssid="eduroam",  # Varsayılan
                    location="Auto-Imported",
                    building="Campus"
                )
            except Exception:
                pass # Zaten varsa hata vermesin
    
    # Auto-retrain
    if auto_retrain and result.get("success") and result.get("created_samples", 0) > 0:
        from app.services.model_retraining import auto_retrain_if_needed
        retrain_result = auto_retrain_if_needed(db)
        result["auto_retrain"] = retrain_result["should_retrain"]
        result["retrain_reason"] = retrain_result["reason"]
        if retrain_result["result"]:
            result["model"] = retrain_result["result"]
    
    return result


@router.post("/train", summary="Modeli eğit (optimize versiyon)")
def train(
    enable_tuning: bool = True,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """
    Optimized model eğitimi.
    
    Parametreler:
    - enable_tuning: True = hiperparametre tuning, False = hızlı eğitim
    """
    from app.services.ml_model import train_model
    return train_model(db, enable_tuning=enable_tuning)


@router.get("/train/info", summary="Model bilgisi")
def get_model_info(
    db: Session = Depends(get_db),
    _=Depends(get_current_user),
):
    """Eğitilmiş modelin metadata'sını döndür"""
    from app.services.ml_model import get_model_info
    return get_model_info()


@router.get("/train/history", summary="Model eğitim geçmişi")
def get_model_history(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """Son 20 model eğitiminin geçmişini döndür"""
    from app.services.ml_model import get_model_history
    history = get_model_history()
    return {
        "total_models": len(history),
        "latest": history[-1] if history else None,
        "history": history,
    }

@router.get("/retrain/status", summary="Otomatik retrain durumu")
def retrain_status(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """Otomatik retraining durumunu kontrol et"""
    from app.services.model_retraining import get_retrain_status
    return get_retrain_status(db)


@router.post("/retrain/trigger", summary="Otomatik retrain'i manuel tetikle")
def trigger_retrain(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """Koşullar sağlanırsa modeli otomatik retrain et"""
    from app.services.model_retraining import auto_retrain_if_needed
    return auto_retrain_if_needed(db)


@router.get("/retrain/history", summary="Otomatik retrain geçmişi")
def retrain_history(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    """Otomatik retraining geçmişini döndür"""
    from app.services.model_retraining import get_retrain_history
    history = get_retrain_history()
    return {
        "total_retrain_events": len(history),
        "latest": history[-1] if history else None,
        "history": history,
    }

@router.get("/evaluate", summary="Model doğruluk testi")
def evaluate(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    from app.services.ml_model import evaluate_model
    return evaluate_model(db)


@router.get("/classrooms/{classroom_id}/fingerprint", summary="Derslik fingerprint'i")
def fingerprint(
    classroom_id: str,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    from app.services.wifi_service import build_classroom_fingerprints
    fp = build_classroom_fingerprints(db, classroom_id)
    if not fp:
        from fastapi import HTTPException
        raise HTTPException(status_code=404, detail="Eğitim verisi yok")
    return {"classroom_id": classroom_id, "ap_count": len(fp), "fingerprint": fp}


# ─── Güvenlik Yönetimi Endpoints ─────────────────────────────────────────────

class NetworkConfigInput(BaseModel):
    name: str
    cidr_range: str
    building: Optional[str] = None
    description: Optional[str] = None


class BSSIDWhitelistInput(BaseModel):
    bssid: str
    ssid: Optional[str] = None
    location: Optional[str] = None
    building: Optional[str] = None


@router.post("/security/networks", summary="Ağ konfigürasyonu ekle")
def add_network(
    payload: NetworkConfigInput,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "ItStaff")),
):
    """Okul ağına yeni bir CIDR range ekle"""
    from app.models.wifi_models import NetworkConfiguration
    
    config = NetworkConfiguration(**payload.dict())
    db.add(config)
    db.commit()
    db.refresh(config)
    return {"id": config.id, "message": "Ağ konfigürasyonu eklendi ✓"}


@router.get("/security/networks", summary="Tüm ağ konfigürasyonları")
def list_networks(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "ItStaff", "Teacher")),
):
    """Tüm ağ konfigürasyonlarını listele"""
    from app.models.wifi_models import NetworkConfiguration
    
    configs = db.query(NetworkConfiguration).all()
    return {
        "total": len(configs),
        "networks": [{
            "id": c.id,
            "name": c.name,
            "cidr_range": c.cidr_range,
            "building": c.building,
            "is_active": c.is_active,
        } for c in configs]
    }


@router.post("/security/bssid-whitelist", summary="BSSID whitelist'e ekle")
def add_bssid_whitelist(
    payload: BSSIDWhitelistInput,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "ItStaff")),
):
    """Bilinen ve güvenilir BSSID'i whitelist'e ekle"""
    from app.services.security_validation import add_bssid_to_whitelist
    
    whitelist = add_bssid_to_whitelist(
        db=db,
        bssid=payload.bssid,
        ssid=payload.ssid,
        location=payload.location,
        building=payload.building,
    )
    return {"bssid": whitelist.bssid, "message": "BSSID whitelist'e eklendi ✓"}


@router.get("/security/bssid-whitelist", summary="BSSID whitelist")
def list_bssid_whitelist(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "ItStaff", "Teacher")),
):
    """Whitelist'teki tüm BSSID'leri listele"""
    from app.models.wifi_models import BSSIDWhitelist
    
    whitelisted = db.query(BSSIDWhitelist).filter(
        BSSIDWhitelist.is_legitimate == True
    ).all()
    
    return {
        "total": len(whitelisted),
        "bssids": [{
            "bssid": b.bssid,
            "ssid": b.ssid,
            "location": b.location,
            "building": b.building,
            "added_at": b.added_at.isoformat(),
        } for b in whitelisted]
    }


@router.get("/security/events", summary="Güvenlik olayları")
def security_events(
    db: Session = Depends(get_db),
    limit: int = 50,
    severity: Optional[str] = None,
    _=Depends(require_role("Admin", "ItStaff")),
):
    """Şüpheli aktiviteleri ve güvenlik olaylarını listele"""
    from app.models.wifi_models import SecurityEvent
    
    query = db.query(SecurityEvent).order_by(SecurityEvent.detected_at.desc())
    
    if severity:
        query = query.filter(SecurityEvent.severity == severity)
    
    events = query.limit(limit).all()
    
    return {
        "total": len(events),
        "events": [{
            "id": e.id,
            "event_type": e.event_type,
            "severity": e.severity,
            "student_id": e.student_id,
            "description": e.description,
            "detected_at": e.detected_at.isoformat(),
            "is_resolved": e.is_resolved,
        } for e in events]
    }


@router.get("/security/audit-logs", summary="Tahmin security audit logs")
def audit_logs(
    db: Session = Depends(get_db),
    limit: int = 50,
    security_status: Optional[str] = None,
    _=Depends(require_role("Admin", "ItStaff", "Teacher")),
):
    """Her tahmin için security audit log'larını listele"""
    from app.models.wifi_models import PredictionSecurityAudit
    
    query = db.query(PredictionSecurityAudit).order_by(
        PredictionSecurityAudit.prediction_timestamp.desc()
    )
    
    if security_status:
        query = query.filter(PredictionSecurityAudit.security_status == security_status)
    
    logs = query.limit(limit).all()
    
    return {
        "total": len(logs),
        "logs": [{
            "id": l.id,
            "student_id": l.student_id,
            "ip_address": l.ip_address,
            "ip_valid": l.ip_validation_passed,
            "security_score": round(l.security_score, 3),
            "security_status": l.security_status,
            "matched_classroom": l.matched_classroom_id,
            "confidence": round(l.confidence_score, 3),
            "is_valid": l.is_valid_prediction,
            "timestamp": l.prediction_timestamp.isoformat(),
        } for l in logs]
    }


@router.get("/security/statistics", summary="Güvenlik istatistikleri")
def security_statistics(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "ItStaff")),
):
    """Güvenlik istatistiklerini döndür"""
    from app.models.wifi_models import (
        PredictionSecurityAudit,
        SecurityEvent,
        BSSIDWhitelist,
        NetworkConfiguration,
    )
    from sqlalchemy import func
    
    # Tahmin istatistikleri
    total_predictions = db.query(func.count(PredictionSecurityAudit.id)).scalar()
    valid_predictions = db.query(func.count(PredictionSecurityAudit.id)).filter(
        PredictionSecurityAudit.is_valid_prediction == True
    ).scalar()
    
    # Güvenlik olayları
    total_events = db.query(func.count(SecurityEvent.id)).scalar()
    high_severity = db.query(func.count(SecurityEvent.id)).filter(
        SecurityEvent.severity.in_(["high", "critical"])
    ).scalar()
    
    # BSSID ve Network
    whitelisted_bssids = db.query(func.count(BSSIDWhitelist.id)).filter(
        BSSIDWhitelist.is_legitimate == True
    ).scalar()
    network_ranges = db.query(func.count(NetworkConfiguration.id)).filter(
        NetworkConfiguration.is_active == True
    ).scalar()
    
    return {
        "predictions": {
            "total": total_predictions or 0,
            "valid": valid_predictions or 0,
            "invalid": (total_predictions or 0) - (valid_predictions or 0),
            "success_rate": round((valid_predictions or 0) / (total_predictions or 1) * 100, 2),
        },
        "security": {
            "total_events": total_events or 0,
            "high_severity": high_severity or 0,
        },
        "configuration": {
            "whitelisted_bssids": whitelisted_bssids or 0,
            "network_ranges": network_ranges or 0,
        }
    }
