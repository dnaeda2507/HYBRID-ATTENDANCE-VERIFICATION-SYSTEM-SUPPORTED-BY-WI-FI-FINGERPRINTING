"""
Security Validation Service

IP Doğrulama, BSSID Validasyon ve Güvenlik Skorlama
"""

import ipaddress
from typing import Tuple, Optional, Dict, List
from datetime import datetime
from sqlalchemy.orm import Session

from app.models.wifi_models import (
    NetworkConfiguration,
    BSSIDWhitelist,
    IPValidationLog,
    SecurityEvent,
    PredictionSecurityAudit,
)


# ─── Konfigürasyon ───────────────────────────────────────────────────────────

# Varsayılan okul ağı CIDR range'leri
DEFAULT_NETWORK_RANGES = [
    "10.10.0.0/16",      # Kampüs Ağı
    "192.168.1.0/24",    # Yönetim Ağı
    "172.16.0.0/12",     # Özel Ağ
]

SUSPICIOUS_IP_RANGES = [
    "10.0.0.0/8",
    "172.16.0.0/12",
    "192.168.0.0/16",
]

# VPN/Proxy tespit parametreleri
VPN_COMMON_PORTS = [1194, 1723, 500, 4500, 6881, 8443]
PROXY_COMMON_PORTS = [3128, 8080, 8888, 9090]


# ─── Utility Fonksiyonları ────────────────────────────────────────────────────

def is_ip_in_network(ip_str: str, cidr_str: str) -> bool:
    """IP'nin verilen CIDR range'inde olup olmadığını kontrol et"""
    try:
        ip = ipaddress.ip_address(ip_str)
        network = ipaddress.ip_network(cidr_str)
        return ip in network
    except ValueError:
        return False


def is_private_ip(ip_str: str) -> bool:
    """Private IP adresi kontrolü"""
    try:
        ip = ipaddress.ip_address(ip_str)
        return ip.is_private
    except ValueError:
        return False


def is_loopback_ip(ip_str: str) -> bool:
    """Loopback IP kontrolü"""
    try:
        ip = ipaddress.ip_address(ip_str)
        return ip.is_loopback
    except ValueError:
        return False


# ─── IP Doğrulama ────────────────────────────────────────────────────────────

def validate_ip_address(
    db: Session,
    student_id: str,
    ip_address: str,
) -> Tuple[bool, Dict]:
    """
    IP adresini doğrula.
    
    Döner:
    - (is_valid, metadata_dict)
    """
    result = {
        "ip": ip_address,
        "is_valid": False,
        "in_network": False,
        "is_vpn_suspected": False,
        "is_proxy_suspected": False,
        "subnet_name": None,
        "network_config_id": None,
        "validation_result": "unknown",
    }
    
    # Loopback = localhost/Docker iç trafiği — VPN değil, ama okul ağında da sayılmaz
    if is_loopback_ip(ip_address):
        result["validation_result"] = "loopback"
        result["is_vpn_suspected"] = False   # ✅ Düzeltildi: loopback ≠ VPN
        result["is_valid"] = False
        return False, result
    
    # Okul ağında mı kontrol et
    configs = db.query(NetworkConfiguration).filter(
        NetworkConfiguration.is_active == True
    ).all()
    
    for config in configs:
        if is_ip_in_network(ip_address, config.cidr_range):
            result["in_network"] = True
            result["subnet_name"] = config.name
            result["network_config_id"] = config.id
            result["validation_result"] = "passed"
            result["is_valid"] = True
            break
    
    # Private IP ama okul ağında değil = şüpheli
    if is_private_ip(ip_address) and not result["in_network"]:
        result["is_vpn_suspected"] = True
        result["validation_result"] = "suspicious_private_ip"
        result["is_valid"] = False
    
    # Public IP = VPN/Proxy şüphesi
    if not is_private_ip(ip_address):
        result["is_proxy_suspected"] = True
        result["is_vpn_suspected"] = True
        result["validation_result"] = "public_ip_suspected_vpn"
        result["is_valid"] = False
    
    # Log kaydet
    log = IPValidationLog(
        student_id=student_id,
        request_ip=ip_address,
        network_config_id=result["network_config_id"],
        ip_in_network=result["in_network"],
        subnet_name=result["subnet_name"],
        is_vpn_suspected=result["is_vpn_suspected"],
        is_proxy_suspected=result["is_proxy_suspected"],
        validation_result=result["validation_result"],
    )
    db.add(log)
    db.commit()
    db.refresh(log)
    
    return result["is_valid"], result


# ─── BSSID Validasyon ────────────────────────────────────────────────────────

def validate_bssids(
    db: Session,
    bssids: List[str],
) -> Tuple[int, int, List[str], bool]:
    """
    BSSID'leri whitelist'le kontrol et.
    
    Döner:
    - (toplam_bssid, whitelist_bssid, suspicious_list, all_whitelisted)
    """
    suspicious_bssids = []
    whitelist_count = 0
    
    for bssid in bssids:
        whitelist_entry = db.query(BSSIDWhitelist).filter(
            BSSIDWhitelist.bssid == bssid.upper()
        ).first()
        
        if whitelist_entry and whitelist_entry.is_legitimate:
            whitelist_count += 1
        else:
            suspicious_bssids.append(bssid)
    
    all_whitelisted = (len(bssids) > 0) and (whitelist_count == len(bssids))
    
    return len(bssids), whitelist_count, suspicious_bssids, all_whitelisted


# ─── Güvenlik Skoru Hesaplama ────────────────────────────────────────────────

def calculate_security_score(
    ip_valid: bool,
    ip_metadata: Dict,
    bssid_total: int,
    bssid_whitelisted: int,
    confidence_score: float,
    student_id: str = None,
) -> Tuple[float, str, List[str]]:
    """
    Comprehensive güvenlik skoru hesapla (0.0 - 1.0).
    
    Faktörler:
    - IP validasyon: %40
    - BSSID whitelist: %30
    - ML confidence: %20
    - Zaman/Pattern: %10
    
    Döner:
    - (security_score, status, risk_factors)
    """
    score = 0.0
    risk_factors = []
    
    # 1. IP Validasyon (%40)
    if ip_valid and ip_metadata.get("in_network"):
        score += 0.40
    elif ip_valid:
        score += 0.20
        risk_factors.append("IP okulun ağında değil")
    else:
        if ip_metadata.get("is_vpn_suspected"):
            risk_factors.append("VPN kullanımı şüphesi")
            score -= 0.10
        if ip_metadata.get("is_proxy_suspected"):
            risk_factors.append("Proxy kullanımı şüphesi")
            score -= 0.10
    
    # 2. BSSID Whitelist (%30)
    if bssid_total > 0:
        bssid_ratio = bssid_whitelisted / bssid_total
        score += bssid_ratio * 0.30
        
        if bssid_ratio < 0.5:
            risk_factors.append(f"Bilinmeyen BSSID'ler ({bssid_total - bssid_whitelisted}/{bssid_total})")
    
    # 3. ML Confidence (%20)
    score += (confidence_score * 0.20)
    if confidence_score < 0.60:
        risk_factors.append("Düşük ML confidence skoru")
    
    # 4. Zaman/Pattern (%10)
    # İleri: Öğrencinin geçmiş pattern'leri analiz et
    score += 0.10  # Varsayılan ek puan
    
    # Score'u 0-1 aralığına tut
    score = max(0.0, min(1.0, score))
    
    # Status belirle
    if score >= 0.85:
        status = "trusted"
    elif score >= 0.70:
        status = "verified"
    elif score >= 0.50:
        status = "suspicious"
    else:
        status = "blocked"
    
    return score, status, risk_factors


# ─── Şüpheli Aktivite Logging ────────────────────────────────────────────────

def log_security_event(
    db: Session,
    event_type: str,
    severity: str,
    student_id: str,
    description: str = None,
    ip_validation_id: str = None,
    bssid_involved: str = None,
    ip_involved: str = None,
) -> SecurityEvent:
    """Güvenlik olayını kaydet"""
    event = SecurityEvent(
        event_type=event_type,
        severity=severity,
        student_id=student_id,
        ip_validation_id=ip_validation_id,
        description=description,
        bssid_involved=bssid_involved,
        ip_involved=ip_involved,
    )
    db.add(event)
    db.commit()
    db.refresh(event)
    return event


# ─── Tahmin Audit Kaydı ──────────────────────────────────────────────────────

def log_prediction_audit(
    db: Session,
    student_id: str,
    ip_address: str,
    ip_valid: bool,
    ip_metadata: Dict,
    bssids: List[str],
    matched_classroom_id: Optional[str],
    confidence_score: float,
) -> PredictionSecurityAudit:
    """
    Her tahmin için security audit kaydı oluştur.
    """
    # BSSID doğrulama
    bssid_total, bssid_whitelisted, suspicious_bssids, all_whitelisted = validate_bssids(
        db, bssids
    )
    
    # Security score hesapla
    security_score, security_status, risk_factors = calculate_security_score(
        ip_valid=ip_valid,
        ip_metadata=ip_metadata,
        bssid_total=bssid_total,
        bssid_whitelisted=bssid_whitelisted,
        confidence_score=confidence_score,
        student_id=student_id,
    )
    
    # İsteğin geçerli olup olmadığını belirle
    is_valid = ip_valid and (bssid_whitelisted > 0) and (confidence_score >= 0.60)
    
    # Audit kaydı oluştur
    audit = PredictionSecurityAudit(
        student_id=student_id,
        
        # IP Doğrulama
        ip_address=ip_address,
        ip_validation_passed=ip_valid,
        
        # BSSID Doğrulama
        bssid_count=bssid_total,
        all_bssids_whitelisted=all_whitelisted,
        suspicious_bssids=len(suspicious_bssids),
        
        # Model Tahmini
        matched_classroom_id=matched_classroom_id,
        confidence_score=confidence_score,
        
        # Güvenlik Skoru
        security_score=security_score,
        security_status=security_status,
        risk_factors=str(risk_factors),
        
        # İstatistik
        is_valid_prediction=is_valid,
    )
    db.add(audit)
    db.commit()
    db.refresh(audit)
    
    # Şüpheli aktiviteleri event log'la
    if not ip_valid:
        log_security_event(
            db,
            event_type="invalid_ip",
            severity="high" if ip_metadata.get("is_vpn_suspected") else "medium",
            student_id=student_id,
            description=ip_metadata.get("validation_result"),
            ip_involved=ip_address,
        )
    
    if len(suspicious_bssids) > 0:
        log_security_event(
            db,
            event_type="suspicious_bssids",
            severity="medium" if len(suspicious_bssids) <= 2 else "high",
            student_id=student_id,
            description=f"{len(suspicious_bssids)} bilinmeyen BSSID tespit edildi",
            bssid_involved=", ".join(suspicious_bssids[:3]),
        )
    
    return audit


# ─── Initialization ──────────────────────────────────────────────────────────

def initialize_default_networks(db: Session) -> None:
    """Varsayılan ağ konfigürasyonlarını oluştur"""
    for cidr in DEFAULT_NETWORK_RANGES:
        existing = db.query(NetworkConfiguration).filter(
            NetworkConfiguration.cidr_range == cidr
        ).first()
        
        if not existing:
            config = NetworkConfiguration(
                name=f"Ağ-{cidr}",
                cidr_range=cidr,
                is_active=True,
            )
            db.add(config)
    
    db.commit()


def add_bssid_to_whitelist(
    db: Session,
    bssid: str,
    ssid: str = None,
    location: str = None,
    building: str = None,
) -> BSSIDWhitelist:
    """Whitelist'e BSSID ekle"""
    existing = db.query(BSSIDWhitelist).filter(
        BSSIDWhitelist.bssid == bssid.upper()
    ).first()
    
    if existing:
        return existing
    
    whitelist_entry = BSSIDWhitelist(
        bssid=bssid.upper(),
        ssid=ssid,
        location=location,
        building=building,
        is_legitimate=True,
    )
    db.add(whitelist_entry)
    db.commit()
    db.refresh(whitelist_entry)
    return whitelist_entry
