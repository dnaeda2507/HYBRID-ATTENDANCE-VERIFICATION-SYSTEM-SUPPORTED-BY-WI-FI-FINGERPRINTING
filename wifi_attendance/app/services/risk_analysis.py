"""
Risk Analysis Service - Öğrenci risk profillerini oluşturma, analiz etme ve flaglama
"""

from sqlalchemy.orm import Session
from datetime import datetime, timedelta
from typing import Tuple, List, Dict
import json
from sqlalchemy import func

from app.models.wifi_models import (
    StudentRiskProfile,
    FlaggedStudent,
    StudentRiskHistory,
    PredictionSecurityAudit,
    SecurityEvent,
)


# ─── Risk Scoring Constants ──────────────────────────────────────────────────

RISK_THRESHOLDS = {
    "flag_trigger": 0.60,           # Bu değerin üzerine çıkan risk skoru → flag
    "critical_threshold": 0.75,     # Critical risk level
    "high_threshold": 0.60,         # High risk level
    "medium_threshold": 0.35,       # Medium risk level
}

# Flaglama tetikleyicileri (sayı tabanlı)
FLAG_TRIGGERS = {
    "consecutive_blocked": 3,           # 3 artarda blocked
    "blocked_in_last_7_days": 5,        # Son 7 günde 5+ blocked
    "suspicious_in_last_7_days": 10,    # Son 7 günde 10+ suspicious
    "invalid_ip_threshold": 4,          # 4+ invalid IP
    "unknown_bssid_threshold": 6,       # 6+ unknown BSSID
}

# Risk puanlaması
RISK_WEIGHTS = {
    "blocked_prediction": 0.15,         # Her blocked = 0.15 puan
    "suspicious_prediction": 0.08,      # Her suspicious = 0.08 puan
    "invalid_ip": 0.10,                 # Her invalid IP = 0.10 puan
    "unknown_bssid": 0.05,              # Her unknown BSSID = 0.05 puan
    "low_confidence": 0.03,             # Her low confidence = 0.03 puan
    "security_event": 0.07,             # Her security event = 0.07 puan
}


def calculate_student_risk_score(db: Session, student_id: str) -> Tuple[float, str, Dict]:
    """
    Öğrencinin risk skorunu hesapla.
    
    Returns: (risk_score: 0.0-1.0, risk_level, details_dict)
    """
    
    # Son 30 gün içindeki verileri çek
    thirty_days_ago = datetime.utcnow() - timedelta(days=30)
    
    audits = db.query(PredictionSecurityAudit).filter(
        PredictionSecurityAudit.student_id == student_id,
        PredictionSecurityAudit.prediction_timestamp >= thirty_days_ago
    ).all()
    
    events = db.query(SecurityEvent).filter(
        SecurityEvent.student_id == student_id,
        SecurityEvent.detected_at >= thirty_days_ago
    ).all()
    
    if not audits:
        return 0.0, "low", {"reason": "no_recent_data"}
    
    # Risk bileşenlerini hesapla
    blocked_count = sum(1 for a in audits if a.security_status == "blocked")
    suspicious_count = sum(1 for a in audits if a.security_status == "suspicious")
    invalid_ip_count = sum(1 for a in audits if not a.ip_validation_passed)
    unknown_bssid_count = sum(1 for a in audits if a.suspicious_bssids > 0)
    low_conf_count = sum(1 for a in audits if a.confidence_score < 0.60)
    
    # Risk skoru hesapla (0.0 - 1.0)
    risk_score = 0.0
    risk_score += blocked_count * RISK_WEIGHTS["blocked_prediction"]
    risk_score += suspicious_count * RISK_WEIGHTS["suspicious_prediction"]
    risk_score += invalid_ip_count * RISK_WEIGHTS["invalid_ip"]
    risk_score += unknown_bssid_count * RISK_WEIGHTS["unknown_bssid"]
    risk_score += low_conf_count * RISK_WEIGHTS["low_confidence"]
    risk_score += len(events) * RISK_WEIGHTS["security_event"]
    
    # Normalize (0.0 - 1.0 arası)
    risk_score = min(risk_score, 1.0)
    
    # Risk level belirle
    if risk_score >= RISK_THRESHOLDS["critical_threshold"]:
        risk_level = "critical"
    elif risk_score >= RISK_THRESHOLDS["high_threshold"]:
        risk_level = "high"
    elif risk_score >= RISK_THRESHOLDS["medium_threshold"]:
        risk_level = "medium"
    else:
        risk_level = "low"
    
    details = {
        "blocked_predictions": blocked_count,
        "suspicious_predictions": suspicious_count,
        "invalid_ip_events": invalid_ip_count,
        "unknown_bssid_events": unknown_bssid_count,
        "low_confidence_events": low_conf_count,
        "security_events": len(events),
        "total_predictions": len(audits),
        "average_security_score": round(
            sum(a.security_score for a in audits) / len(audits) if audits else 0, 3
        ),
    }
    
    return risk_score, risk_level, details


def should_flag_student(db: Session, student_id: str) -> Tuple[bool, str, List[str]]:
    """
    Öğrencinin flaglanması gerekip gerekmediğini kontrol et.
    
    Returns: (should_flag, primary_reason, [detailed_reasons])
    """
    
    thirty_days_ago = datetime.utcnow() - timedelta(days=30)
    seven_days_ago = datetime.utcnow() - timedelta(days=7)
    
    audits = db.query(PredictionSecurityAudit).filter(
        PredictionSecurityAudit.student_id == student_id,
        PredictionSecurityAudit.prediction_timestamp >= thirty_days_ago
    ).all()
    
    if not audits:
        return False, "", []
    
    reasons = []
    
    # 1. Ardışık blocked kontrol
    recent_audits = db.query(PredictionSecurityAudit).filter(
        PredictionSecurityAudit.student_id == student_id,
    ).order_by(PredictionSecurityAudit.prediction_timestamp.desc()).limit(10).all()
    
    consecutive_blocked = 0
    for audit in recent_audits:
        if audit.security_status == "blocked":
            consecutive_blocked += 1
        else:
            break
    
    if consecutive_blocked >= FLAG_TRIGGERS["consecutive_blocked"]:
        reasons.append(f"Ardışık {consecutive_blocked} blocked tahmin")
    
    # 2. Son 7 günde blocked sayısı
    blocked_7d = sum(1 for a in audits 
                     if a.security_status == "blocked" and 
                     a.prediction_timestamp >= seven_days_ago)
    
    if blocked_7d >= FLAG_TRIGGERS["blocked_in_last_7_days"]:
        reasons.append(f"Son 7 günde {blocked_7d} blocked tahmin")
    
    # 3. Son 7 günde suspicious sayısı
    suspicious_7d = sum(1 for a in audits 
                        if a.security_status == "suspicious" and 
                        a.prediction_timestamp >= seven_days_ago)
    
    if suspicious_7d >= FLAG_TRIGGERS["suspicious_in_last_7_days"]:
        reasons.append(f"Son 7 günde {suspicious_7d} suspicious tahmin")
    
    # 4. Invalid IP sayısı
    invalid_ip = sum(1 for a in audits if not a.ip_validation_passed)
    
    if invalid_ip >= FLAG_TRIGGERS["invalid_ip_threshold"]:
        reasons.append(f"Toplam {invalid_ip} invalid IP hatası")
    
    # 5. Unknown BSSID sayısı
    unknown_bssid = sum(1 for a in audits if a.suspicious_bssids > 0)
    
    if unknown_bssid >= FLAG_TRIGGERS["unknown_bssid_threshold"]:
        reasons.append(f"Toplam {unknown_bssid} bilinmeyen BSSID")
    
    # 6. Risk score eşiği
    risk_score, _, _ = calculate_student_risk_score(db, student_id)
    
    if risk_score >= RISK_THRESHOLDS["flag_trigger"]:
        reasons.append(f"Risk skoru: {round(risk_score, 3)} (eşik: {RISK_THRESHOLDS['flag_trigger']})")
    
    should_flag = len(reasons) > 0
    primary_reason = reasons[0] if reasons else ""
    
    return should_flag, primary_reason, reasons


def update_or_create_risk_profile(db: Session, student_id: str) -> StudentRiskProfile:
    """
    Öğrencinin risk profili'ni güncelle veya oluştur.
    Tarihsel verileri analiz ederek tüm metrikleri hesapla.
    """
    
    # Var olan profili bul
    profile = db.query(StudentRiskProfile).filter(
        StudentRiskProfile.student_id == student_id
    ).first()
    
    if not profile:
        profile = StudentRiskProfile(student_id=student_id)
        db.add(profile)
        db.flush()
    
    # Tarihsel verileri topla
    audits = db.query(PredictionSecurityAudit).filter(
        PredictionSecurityAudit.student_id == student_id
    ).all()
    
    events = db.query(SecurityEvent).filter(
        SecurityEvent.student_id == student_id
    ).all()
    
    # Metrikleri hesapla
    profile.total_predictions = len(audits)
    profile.total_security_events = len(events)
    profile.blocked_predictions = sum(1 for a in audits if a.security_status == "blocked")
    profile.suspicious_predictions = sum(1 for a in audits if a.security_status == "suspicious")
    profile.invalid_ip_count = sum(1 for a in audits if not a.ip_validation_passed)
    profile.unknown_bssid_count = sum(1 for a in audits if a.suspicious_bssids > 0)
    profile.low_confidence_count = sum(1 for a in audits if a.confidence_score < 0.60)
    
    # Ortalamalar
    if audits:
        profile.average_security_score = sum(a.security_score for a in audits) / len(audits)
        profile.average_confidence = sum(a.confidence_score for a in audits) / len(audits)
    
    # Risk skoru ve level
    risk_score, risk_level, details = calculate_student_risk_score(db, student_id)
    profile.risk_score = risk_score
    profile.risk_level = risk_level
    
    # Son aktiviteler
    if audits:
        latest_audit = max(audits, key=lambda x: x.prediction_timestamp)
        profile.last_prediction_at = latest_audit.prediction_timestamp
    
    if events:
        latest_event = max(events, key=lambda x: x.detected_at)
        profile.last_suspicious_event_at = latest_event.detected_at
    
    # Otomatik flaglama kontrolü
    should_flag, primary_reason, reasons = should_flag_student(db, student_id)
    
    if should_flag and not profile.is_flagged:
        # Flag'le
        profile.is_flagged = True
        profile.flag_reason = primary_reason
        profile.flagged_at = datetime.utcnow()
        
        # FlaggedStudent kaydı oluştur
        flagged_record = FlaggedStudent(
            risk_profile_id=profile.id,
            student_id=student_id,
            flag_reason=primary_reason,
            severity="high" if risk_score >= 0.70 else "warning",
            suspicious_event_count=len(events),
            avg_risk_score_at_flag=risk_score,
        )
        db.add(flagged_record)
    
    elif not should_flag and profile.is_flagged:
        # Flag'i kaldır (risk azaldıysa)
        profile.is_flagged = False
        profile.unflagged_at = datetime.utcnow()
        
        # Aktif FlaggedStudent kaydını kapat
        active_flag = db.query(FlaggedStudent).filter(
            FlaggedStudent.risk_profile_id == profile.id,
            FlaggedStudent.is_active == True
        ).first()
        
        if active_flag:
            active_flag.is_active = False
            active_flag.resolution_status = "resolved"
            active_flag.resolved_at = datetime.utcnow()
    
    profile.updated_at = datetime.utcnow()
    
    db.commit()
    db.refresh(profile)
    
    # Risk history'ye kayıt et
    history = StudentRiskHistory(
        risk_profile_id=profile.id,
        student_id=student_id,
        risk_score=risk_score,
        risk_level=risk_level,
        total_events=len(events),
        blocked_count=profile.blocked_predictions,
        suspicious_count=profile.suspicious_predictions,
        average_security_score=profile.average_security_score,
        reason_for_record="auto_analysis",
    )
    db.add(history)
    db.commit()
    
    return profile


def analyze_all_students(db: Session) -> Dict:
    """
    Tüm öğrencilerin risk profillerini güncelle.
    Sistem canlıya geçtiğinde veya scheduled job'da çalışır.
    """
    
    # Tüm unique student'ları bul
    unique_students = db.query(
        PredictionSecurityAudit.student_id
    ).distinct().all()
    
    results = {
        "total_analyzed": 0,
        "newly_flagged": 0,
        "unflagged": 0,
        "flagged_students": [],
    }
    
    for (student_id,) in unique_students:
        profile = update_or_create_risk_profile(db, student_id)
        results["total_analyzed"] += 1
        
        if profile.is_flagged:
            results["flagged_students"].append({
                "student_id": student_id,
                "risk_score": round(profile.risk_score, 3),
                "risk_level": profile.risk_level,
                "reason": profile.flag_reason,
                "flagged_at": profile.flagged_at.isoformat(),
            })
    
    return results


def get_risk_summary() -> Dict:
    """
    Sistem çapında risk özetini döndür.
    """
    from app.core.database import SessionLocal
    
    db = SessionLocal()
    
    try:
        profiles = db.query(StudentRiskProfile).all()
        
        critical_count = sum(1 for p in profiles if p.risk_level == "critical")
        high_count = sum(1 for p in profiles if p.risk_level == "high")
        medium_count = sum(1 for p in profiles if p.risk_level == "medium")
        low_count = sum(1 for p in profiles if p.risk_level == "low")
        
        flagged_count = sum(1 for p in profiles if p.is_flagged)
        
        total_risk_score = sum(p.risk_score for p in profiles) / len(profiles) if profiles else 0
        
        return {
            "total_students": len(profiles),
            "risk_distribution": {
                "critical": critical_count,
                "high": high_count,
                "medium": medium_count,
                "low": low_count,
            },
            "flagged_students": flagged_count,
            "average_system_risk": round(total_risk_score, 3),
            "last_analyzed_at": datetime.utcnow().isoformat(),
        }
    finally:
        db.close()
