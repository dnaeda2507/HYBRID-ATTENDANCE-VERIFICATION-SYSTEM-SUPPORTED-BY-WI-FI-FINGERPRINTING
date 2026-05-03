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
