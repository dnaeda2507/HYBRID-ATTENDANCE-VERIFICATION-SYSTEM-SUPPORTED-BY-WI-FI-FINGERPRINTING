import uuid
from datetime import datetime
from typing import Optional

from sqlalchemy import (
    Boolean, Column, DateTime, Float, ForeignKey,
    Integer, String, Text,
)
from sqlalchemy.orm import declarative_base, relationship

WifiBase = declarative_base()


def _uuid() -> str:
    return str(uuid.uuid4())


class Classroom(WifiBase):
    __tablename__ = "wifi_classrooms"

    id = Column(String(36), primary_key=True, default=_uuid)
    name = Column(String(255), nullable=False)
    building = Column(String(255), nullable=True)
    floor = Column(Integer, nullable=True)
    capacity = Column(Integer, nullable=True)

    training_samples = relationship("WifiTrainingSample", back_populates="classroom")
    wifi_sessions = relationship("WifiSession", back_populates="classroom")


class WifiTrainingSample(WifiBase):
    __tablename__ = "wifi_training_samples"

    id = Column(String(36), primary_key=True, default=_uuid)
    classroom_id = Column(String(36), ForeignKey("wifi_classrooms.id"), nullable=False)
    scanned_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    notes = Column(Text, nullable=True)

    classroom = relationship("Classroom", back_populates="training_samples")
    access_points = relationship("WifiTrainingAccessPoint", back_populates="training_sample")


class WifiTrainingAccessPoint(WifiBase):
    __tablename__ = "wifi_training_access_points"

    id = Column(String(36), primary_key=True, default=_uuid)
    training_sample_id = Column(String(36), ForeignKey("wifi_training_samples.id"), nullable=False)
    bssid = Column(String(50), nullable=False)
    ssid = Column(String(255), nullable=True)
    rssi = Column(Integer, nullable=False)
    frequency = Column(Integer, nullable=True)
    channel = Column(Integer, nullable=True)

    training_sample = relationship("WifiTrainingSample", back_populates="access_points")


class WifiSession(WifiBase):
    __tablename__ = "wifi_sessions"

    id = Column(String(36), primary_key=True, default=_uuid)
    cs_session_id = Column(String(50), nullable=False, unique=True, index=True)
    classroom_id = Column(String(36), ForeignKey("wifi_classrooms.id"), nullable=False)
    started_by = Column(String(36), nullable=True)
    is_active = Column(Boolean, default=True, nullable=False)
    started_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    ended_at = Column(DateTime, nullable=True)

    classroom = relationship("Classroom", back_populates="wifi_sessions")
    student_scans = relationship("StudentWifiScan", back_populates="wifi_session")


class StudentWifiScan(WifiBase):
    __tablename__ = "student_wifi_scans"

    id = Column(String(36), primary_key=True, default=_uuid)
    student_id = Column(String(36), nullable=False, index=True)
    wifi_session_id = Column(String(36), ForeignKey("wifi_sessions.id"), nullable=False)
    device_info = Column(String(500), nullable=True)
    scanned_at = Column(DateTime, nullable=False)
    matched_classroom_id = Column(String(36), nullable=True)
    confidence = Column(Float, nullable=True)

    wifi_session = relationship("WifiSession", back_populates="student_scans")
    access_points = relationship("StudentWifiAccessPoint", back_populates="scan")


class StudentWifiAccessPoint(WifiBase):
    __tablename__ = "student_wifi_access_points"

    id = Column(String(36), primary_key=True, default=_uuid)
    scan_id = Column(String(36), ForeignKey("student_wifi_scans.id"), nullable=False)
    bssid = Column(String(50), nullable=False)
    ssid = Column(String(255), nullable=True)
    rssi = Column(Integer, nullable=False)
    frequency = Column(Integer, nullable=True)
    channel = Column(Integer, nullable=True)

    scan = relationship("StudentWifiScan", back_populates="access_points")


# ─── Security Models ──────────────────────────────────────────────────────────

class NetworkConfiguration(WifiBase):
    __tablename__ = "wifi_network_configurations"

    id = Column(String(36), primary_key=True, default=_uuid)
    name = Column(String(255), nullable=False)
    cidr_range = Column(String(50), nullable=False)
    building = Column(String(255), nullable=True)
    description = Column(Text, nullable=True)
    is_active = Column(Boolean, default=True, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class BSSIDWhitelist(WifiBase):
    __tablename__ = "wifi_bssid_whitelist"

    id = Column(String(36), primary_key=True, default=_uuid)
    bssid = Column(String(50), nullable=False, unique=True, index=True)
    ssid = Column(String(255), nullable=True)
    location = Column(String(255), nullable=True)
    building = Column(String(255), nullable=True)
    is_legitimate = Column(Boolean, default=True, nullable=False)
    added_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class IPValidationLog(WifiBase):
    __tablename__ = "wifi_ip_validation_logs"

    id = Column(String(36), primary_key=True, default=_uuid)
    student_id = Column(String(36), nullable=False, index=True)
    request_ip = Column(String(50), nullable=False)
    network_config_id = Column(String(36), nullable=True)
    ip_in_network = Column(Boolean, default=False, nullable=False)
    subnet_name = Column(String(255), nullable=True)
    is_vpn_suspected = Column(Boolean, default=False, nullable=False)
    is_proxy_suspected = Column(Boolean, default=False, nullable=False)
    validation_result = Column(String(100), nullable=True)
    validated_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class SecurityEvent(WifiBase):
    __tablename__ = "wifi_security_events"

    id = Column(String(36), primary_key=True, default=_uuid)
    event_type = Column(String(100), nullable=False)
    severity = Column(String(50), nullable=False)
    student_id = Column(String(36), nullable=True, index=True)
    ip_validation_id = Column(String(36), nullable=True)
    description = Column(Text, nullable=True)
    bssid_involved = Column(String(500), nullable=True)
    ip_involved = Column(String(50), nullable=True)
    is_resolved = Column(Boolean, default=False, nullable=False)
    detected_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class PredictionSecurityAudit(WifiBase):
    __tablename__ = "wifi_prediction_security_audits"

    id = Column(String(36), primary_key=True, default=_uuid)
    student_id = Column(String(36), nullable=False, index=True)
    ip_address = Column(String(50), nullable=False)
    ip_validation_passed = Column(Boolean, default=False, nullable=False)
    bssid_count = Column(Integer, default=0, nullable=False)
    all_bssids_whitelisted = Column(Boolean, default=False, nullable=False)
    suspicious_bssids = Column(Integer, default=0, nullable=False)
    matched_classroom_id = Column(String(36), nullable=True)
    confidence_score = Column(Float, default=0.0, nullable=False)
    security_score = Column(Float, default=0.0, nullable=False)
    security_status = Column(String(50), nullable=True)
    risk_factors = Column(Text, nullable=True)
    is_valid_prediction = Column(Boolean, default=False, nullable=False)
    prediction_timestamp = Column(DateTime, default=datetime.utcnow, nullable=False)


# ─── Risk Analysis Models ─────────────────────────────────────────────────────

class StudentRiskProfile(WifiBase):
    __tablename__ = "wifi_student_risk_profiles"

    id = Column(String(36), primary_key=True, default=_uuid)
    student_id = Column(String(36), nullable=False, unique=True, index=True)
    risk_score = Column(Float, default=0.0, nullable=False)
    risk_level = Column(String(50), default="low", nullable=False)
    total_predictions = Column(Integer, default=0, nullable=False)
    total_security_events = Column(Integer, default=0, nullable=False)
    blocked_predictions = Column(Integer, default=0, nullable=False)
    suspicious_predictions = Column(Integer, default=0, nullable=False)
    invalid_ip_count = Column(Integer, default=0, nullable=False)
    unknown_bssid_count = Column(Integer, default=0, nullable=False)
    low_confidence_count = Column(Integer, default=0, nullable=False)
    average_security_score = Column(Float, default=0.0, nullable=False)
    average_confidence = Column(Float, default=0.0, nullable=False)
    is_flagged = Column(Boolean, default=False, nullable=False)
    flag_reason = Column(Text, nullable=True)
    flagged_at = Column(DateTime, nullable=True)
    unflagged_at = Column(DateTime, nullable=True)
    last_prediction_at = Column(DateTime, nullable=True)
    last_suspicious_event_at = Column(DateTime, nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class FlaggedStudent(WifiBase):
    __tablename__ = "wifi_flagged_students"

    id = Column(String(36), primary_key=True, default=_uuid)
    risk_profile_id = Column(String(36), ForeignKey("wifi_student_risk_profiles.id"), nullable=False)
    student_id = Column(String(36), nullable=False, index=True)
    flag_reason = Column(Text, nullable=True)
    severity = Column(String(50), nullable=True)
    suspicious_event_count = Column(Integer, default=0, nullable=False)
    avg_risk_score_at_flag = Column(Float, default=0.0, nullable=False)
    is_active = Column(Boolean, default=True, nullable=False)
    resolution_status = Column(String(100), nullable=True)
    resolved_at = Column(DateTime, nullable=True)
    flagged_at = Column(DateTime, default=datetime.utcnow, nullable=False)


class StudentRiskHistory(WifiBase):
    __tablename__ = "wifi_student_risk_history"

    id = Column(String(36), primary_key=True, default=_uuid)
    risk_profile_id = Column(String(36), ForeignKey("wifi_student_risk_profiles.id"), nullable=False)
    student_id = Column(String(36), nullable=False, index=True)
    risk_score = Column(Float, default=0.0, nullable=False)
    risk_level = Column(String(50), nullable=True)
    total_events = Column(Integer, default=0, nullable=False)
    blocked_count = Column(Integer, default=0, nullable=False)
    suspicious_count = Column(Integer, default=0, nullable=False)
    average_security_score = Column(Float, default=0.0, nullable=False)
    reason_for_record = Column(String(255), nullable=True)
    recorded_at = Column(DateTime, default=datetime.utcnow, nullable=False)
