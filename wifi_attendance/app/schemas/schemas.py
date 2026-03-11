from pydantic import BaseModel, EmailStr, Field
from typing import List, Optional
from datetime import datetime


class AccessPointBase(BaseModel):
    bssid: str
    ssid: Optional[str] = None
    rssi: int = Field(..., le=0, ge=-110)
    frequency: Optional[int] = None
    channel: Optional[int] = None


class AccessPointOut(AccessPointBase):
    id: str
    class Config:
        from_attributes = True


class TrainingSampleCreate(BaseModel):
    classroom_id: str
    notes: Optional[str] = None
    access_points: List[AccessPointBase] = Field(..., min_length=1)


class TrainingSampleOut(BaseModel):
    id: str
    classroom_id: str
    scanned_at: datetime
    notes: Optional[str]
    access_points: List[AccessPointOut]
    class Config:
        from_attributes = True


class StudentScanCreate(BaseModel):
    session_id: str
    device_info: Optional[str] = None
    access_points: List[AccessPointBase] = Field(..., min_length=1)
    client_timestamp: datetime


class AttendanceCheckInResponse(BaseModel):
    success: bool
    message: str
    attendance_id: Optional[str] = None
    matched_classroom: Optional[str] = None
    confidence: Optional[float] = None


class SessionCreate(BaseModel):
    lecture_id: str
    classroom_id: Optional[str] = None
    started_at: datetime


class SessionOut(BaseModel):
    id: str
    lecture_id: str
    classroom_id: Optional[str]
    started_at: datetime
    ended_at: Optional[datetime]
    is_active: bool
    class Config:
        from_attributes = True


class ClassroomCreate(BaseModel):
    name: str
    building: Optional[str] = None
    floor: Optional[int] = None
    capacity: Optional[int] = None


class ClassroomOut(ClassroomCreate):
    id: str
    class Config:
        from_attributes = True


class Token(BaseModel):
    access_token: str
    token_type: str = "bearer"


class UserOut(BaseModel):
    id: str
    email: str
    full_name: str
    role: str
    student_no: Optional[str] = None
