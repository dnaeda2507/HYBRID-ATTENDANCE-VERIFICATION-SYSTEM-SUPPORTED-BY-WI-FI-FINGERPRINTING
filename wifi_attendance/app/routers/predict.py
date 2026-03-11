from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from typing import List, Optional
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


class PredictResponse(BaseModel):
    matched: bool
    classroom_name: Optional[str] = None
    classroom_id: Optional[str] = None
    confidence: float
    message: str


@router.post("", response_model=PredictResponse, summary="WiFi konumu tahmin et")
def predict_location(
    payload: PredictRequest,
    db: Session = Depends(get_db),
    _=Depends(verify_internal_token),
):
    """
    C# backend bu endpoint'i çağırır.
    Öğrencinin WiFi taramasını alır, hangi derslikte olduğunu tahmin eder.
    Yoklama kaydetmez — sadece tahmin döner.
    """
    from app.services.ml_model import predict_classroom

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
        )

    matched_classroom_id, confidence = predict_classroom(ap_objects, db)

    if matched_classroom_id:
        from app.models.wifi_models import Classroom
        classroom = db.query(Classroom).filter(
            Classroom.id == matched_classroom_id
        ).first()
        return PredictResponse(
            matched=True,
            classroom_name=classroom.name if classroom else matched_classroom_id,
            classroom_id=matched_classroom_id,
            confidence=round(confidence, 3),
            message="Konum doğrulandı ✓",
        )
    else:
        return PredictResponse(
            matched=False,
            confidence=round(confidence, 3),
            message=f"Konum doğrulanamadı. Skor: {round(confidence, 3)}, gereken: 0.60",
        )
