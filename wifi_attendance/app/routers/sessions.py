from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from datetime import datetime
from pydantic import BaseModel
from typing import Optional
import httpx

from app.core.database import get_db
from app.core.security import require_role
from app.models.wifi_models import WifiSession, Classroom

router = APIRouter(prefix="/sessions", tags=["Sessions"])

CS_BACKEND_URL = "https://localhost:9001"


class StartSessionRequest(BaseModel):
    cs_session_id: int          # C#'tan gelen sessionId
    classroom_id: str           # FastAPI'deki derslik ID'si
    cs_token: Optional[str] = None  # C#'ın ürettiği session token'ı


class CreateAndStartRequest(BaseModel):
    """Öğretmen tek istekte hem C#'ta session açar hem WiFi yoklamayı başlatır"""
    course_id: int
    classroom_id: str
    start_time: Optional[str] = None
    end_time: Optional[str] = None


@router.post("/create-and-start", summary="C#'ta session aç ve WiFi yoklamasını başlat")
def create_and_start_session(
    payload: CreateAndStartRequest,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    """
    Öğretmen bu endpoint'i çağırır:
    1. C# backend'de session oluşturur
    2. Dönen sessionId ile FastAPI'de WiFi yoklamasını başlatır
    """
    # 1. C# backend'de session oluştur
    try:
        response = httpx.post(
            f"{CS_BACKEND_URL}/api/sessions/create",
            json={
                "courseId": payload.course_id,
                "startTime": payload.start_time,
                "endTime": payload.end_time,
            },
            headers={"Authorization": f"Bearer {current_user['token']}"},
            verify=False,
            timeout=5.0,
        )
        cs_data = response.json()
    except httpx.ConnectError:
        raise HTTPException(status_code=503, detail="C# backend'e ulaşılamıyor")

    if response.status_code != 200 or not cs_data.get("succeeded"):
        raise HTTPException(
            status_code=400,
            detail=f"C# session oluşturulamadı: {cs_data.get('message', 'Bilinmeyen hata')}"
        )

    session_data = cs_data.get("data", {})
    cs_session_id = session_data.get("sessionId")
    cs_token = session_data.get("token")

    if not cs_session_id:
        raise HTTPException(status_code=500, detail="C#'tan sessionId alınamadı")

    # 2. Derslik var mı?
    classroom = db.query(Classroom).filter(Classroom.id == payload.classroom_id).first()
    if not classroom:
        raise HTTPException(status_code=404, detail="Derslik bulunamadı")

    # 3. Zaten aktif WiFi session var mı?
    existing = db.query(WifiSession).filter(
        WifiSession.cs_session_id == str(cs_session_id),
        WifiSession.is_active == True,
    ).first()
    if existing:
        raise HTTPException(status_code=409, detail="Bu oturum için zaten aktif WiFi yoklaması var")

    # 4. FastAPI'de WiFi session başlat
    wifi_session = WifiSession(
        cs_session_id=str(cs_session_id),
        classroom_id=payload.classroom_id,
        started_at=datetime.utcnow(),
        started_by=current_user["id"],
        is_active=True,
    )
    db.add(wifi_session)
    db.commit()
    db.refresh(wifi_session)

    return {
        "message": "Session oluşturuldu ve WiFi yoklaması başlatıldı ✓",
        "cs_session_id": cs_session_id,
        "cs_token": cs_token,
        "wifi_session_id": wifi_session.id,
        "classroom": classroom.name,
        "started_at": wifi_session.started_at,
    }


@router.post("/start", summary="Mevcut C# session için WiFi yoklamasını başlat")
def start_wifi_session(
    payload: StartSessionRequest,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    """C# session zaten varsa sadece WiFi yoklamasını başlat"""
    classroom = db.query(Classroom).filter(Classroom.id == payload.classroom_id).first()
    if not classroom:
        raise HTTPException(status_code=404, detail="Derslik bulunamadı")

    existing = db.query(WifiSession).filter(
        WifiSession.cs_session_id == str(payload.cs_session_id),
        WifiSession.is_active == True,
    ).first()
    if existing:
        raise HTTPException(status_code=409, detail="Bu oturum için zaten aktif WiFi yoklaması var")

    wifi_session = WifiSession(
        cs_session_id=str(payload.cs_session_id),
        classroom_id=payload.classroom_id,
        started_at=datetime.utcnow(),
        started_by=current_user["id"],
        is_active=True,
    )
    db.add(wifi_session)
    db.commit()
    db.refresh(wifi_session)

    return {
        "message": "WiFi yoklaması başlatıldı ✓",
        "cs_session_id": payload.cs_session_id,
        "wifi_session_id": wifi_session.id,
        "classroom": classroom.name,
        "started_at": wifi_session.started_at,
    }


@router.post("/stop", summary="WiFi yoklamasını durdur")
def stop_wifi_session(
    cs_session_id: int,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    wifi_session = db.query(WifiSession).filter(
        WifiSession.cs_session_id == str(cs_session_id),
        WifiSession.is_active == True,
    ).first()
    if not wifi_session:
        raise HTTPException(status_code=404, detail="Aktif WiFi oturumu bulunamadı")

    wifi_session.is_active = False
    wifi_session.ended_at = datetime.utcnow()
    db.commit()

    return {
        "message": "WiFi yoklaması durduruldu ✓",
        "cs_session_id": cs_session_id,
        "ended_at": wifi_session.ended_at,
    }
