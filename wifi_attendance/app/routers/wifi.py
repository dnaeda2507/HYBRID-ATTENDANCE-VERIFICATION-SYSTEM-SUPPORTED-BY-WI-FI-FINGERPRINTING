from fastapi import APIRouter, Depends, HTTPException, File, UploadFile
from sqlalchemy.orm import Session
from typing import List
from datetime import datetime

from app.core.database import get_db
from app.core.security import get_current_user, require_role
from app.models.wifi_models import (
    Classroom, WifiTrainingSample, WifiTrainingAccessPoint, WifiSession
)
from app.schemas.schemas import (
    AutoCheckInRequest,
    TrainingSampleCreate, TrainingSampleOut,
    StudentScanCreate, AttendanceCheckInResponse,
    ClassroomCreate, ClassroomOut,
)
from app.services.attendance_service import check_in_with_wifi, get_session_attendance
from app.services.wifi_service import build_classroom_fingerprints

router = APIRouter(prefix="/wifi", tags=["WiFi"])


@router.post("/classrooms", response_model=ClassroomOut, summary="Derslik ekle")
def create_classroom(
    payload: ClassroomCreate,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    classroom = Classroom(**payload.dict())
    db.add(classroom)
    db.commit()
    db.refresh(classroom)
    return classroom


@router.get("/classrooms", response_model=List[ClassroomOut], summary="Tüm derslikler")
def list_classrooms(db: Session = Depends(get_db), _=Depends(get_current_user)):
    return db.query(Classroom).all()


@router.post("/sessions/start", summary="Öğretmen WiFi yoklamasını başlatır")
def start_wifi_session(
    cs_session_id: str,
    classroom_id: str,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    existing = db.query(WifiSession).filter(
        WifiSession.cs_session_id == cs_session_id,
        WifiSession.is_active == True,
    ).first()
    if existing:
        raise HTTPException(status_code=409, detail="Bu oturum için zaten aktif bir WiFi yoklaması var")

    classroom = db.query(Classroom).filter(Classroom.id == classroom_id).first()
    if not classroom:
        raise HTTPException(status_code=404, detail="Derslik bulunamadı")

    wifi_session = WifiSession(
        cs_session_id=cs_session_id,
        classroom_id=classroom_id,
        started_at=datetime.utcnow(),
        started_by=current_user["id"],
        is_active=True,
    )
    db.add(wifi_session)
    db.commit()
    db.refresh(wifi_session)

    return {
        "message": "WiFi yoklaması başlatıldı ✓",
        "wifi_session_id": wifi_session.id,
        "cs_session_id": cs_session_id,
        "classroom": classroom.name,
        "started_at": wifi_session.started_at,
    }


@router.post("/sessions/stop", summary="Öğretmen WiFi yoklamasını durdurur")
def stop_wifi_session(
    cs_session_id: str,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    wifi_session = db.query(WifiSession).filter(
        WifiSession.cs_session_id == cs_session_id,
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


@router.post("/training/samples", response_model=TrainingSampleOut, summary="WiFi eğitim verisi ekle")
def add_training_sample(
    payload: TrainingSampleCreate,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    classroom = db.query(Classroom).filter(Classroom.id == payload.classroom_id).first()
    if not classroom:
        raise HTTPException(status_code=404, detail="Derslik bulunamadı")

    sample = WifiTrainingSample(
        classroom_id=payload.classroom_id,
        scanned_by=current_user["id"],
        notes=payload.notes,
    )
    db.add(sample)
    db.flush()

    for ap in payload.access_points:
        db.add(WifiTrainingAccessPoint(
            sample_id=sample.id,
            **ap.dict(),
        ))

    db.commit()
    db.refresh(sample)
    return sample


@router.get("/training/classrooms/{classroom_id}/fingerprint", summary="Derslik fingerprint'ini göster")
def get_classroom_fingerprint(
    classroom_id: str,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    fp = build_classroom_fingerprints(db, classroom_id)
    if not fp:
        raise HTTPException(status_code=404, detail="Bu derslik için eğitim verisi yok")
    return {
        "classroom_id": classroom_id,
        "ap_count": len(fp),
        "fingerprint": fp,
    }


@router.get("/training/samples", response_model=List[TrainingSampleOut], summary="Tüm eğitim örnekleri")
def list_training_samples(
    classroom_id: str = None,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    q = db.query(WifiTrainingSample)
    if classroom_id:
        q = q.filter(WifiTrainingSample.classroom_id == classroom_id)
    return q.all()


@router.post("/attendance/check-in", response_model=AttendanceCheckInResponse, summary="WiFi ile yoklama al")
def wifi_check_in(
    payload: StudentScanCreate,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Student")),
):
    return check_in_with_wifi(db, current_user["id"], current_user["token"], payload)


@router.get("/attendance/sessions/{session_id}", summary="Oturumun WiFi yoklama listesi")
def session_attendance(
    session_id: str,
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    records = get_session_attendance(db, session_id)
    return {
        "cs_session_id": session_id,
        "total_scans": len(records),
        "verified": sum(1 for r in records if r.matched_classroom_id),
        "records": [
            {
                "student_id": r.student_id,
                "matched_classroom_id": r.matched_classroom_id,
                "confidence": r.confidence,
                "scanned_at": r.scanned_at,
            }
            for r in records
        ],
    }


@router.get("/debug/cs-sessions", summary="C# session listesi (test için)")
def get_cs_sessions(db: Session = Depends(get_db), _=Depends(get_current_user)):
    from sqlalchemy import text
    try:
        result = db.execute(text("SELECT TOP 10 Id, StartTime, EndTime, IsActive FROM Sessions ORDER BY Id DESC")).fetchall()
        return [{"id": str(r.Id), "start": str(r.StartTime), "end": str(r.EndTime), "active": r.IsActive} for r in result]
    except Exception as e:
        return {"error": str(e)}


@router.get("/debug/cs-sessions2", summary="C# session kolonları")
def get_cs_sessions2(db: Session = Depends(get_db), _=Depends(get_current_user)):
    from sqlalchemy import text
    try:
        result = db.execute(text("SELECT TOP 5 * FROM Sessions ORDER BY Id DESC")).fetchall()
        if result:
            return {"columns": list(result[0]._mapping.keys()), "rows": [dict(r._mapping) for r in result]}
        return {"message": "Kayıt yok"}
    except Exception as e:
        return {"error": str(e)}


@router.post("/model/train", summary="ML modelini eğit")
def train_wifi_model(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    from app.services.ml_model import train_model
    result = train_model(db)
    return result


@router.get("/model/evaluate", summary="Model doğruluk testi — confusion matrix")
def evaluate_wifi_model(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    from app.services.ml_model import evaluate_model
    return evaluate_model(db)


@router.post("/training/import-csv", summary="CSV ile toplu eğitim verisi yükle")
def import_csv(
    file: bytes = File(...),
    format: str = "custom",
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    from app.services.csv_import import import_from_custom_csv, import_from_simple_csv
    from fastapi import File

    content = file.decode("utf-8")

    if format == "simple":
        result = import_from_simple_csv(content, db, scanned_by=current_user["id"])
    else:
        result = import_from_custom_csv(content, db, scanned_by=current_user["id"])

    if result.get("success") and result.get("created_samples", 0) > 0:
        from app.services.ml_model import train_model
        train_result = train_model(db)
        result["model_retrained"] = train_result.get("success", False)
        result["model_accuracy"] = train_result.get("accuracy")

    return result


@router.post("/training/upload-csv", summary="CSV dosyası yükle ve eğit")
async def upload_csv(
    file: UploadFile,
    format: str = "custom",
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "SuperAdmin", "Teacher")),
):
    from app.services.csv_import import import_from_custom_csv, import_from_simple_csv

    content = (await file.read()).decode("utf-8")

    if format == "simple":
        result = import_from_simple_csv(content, db, scanned_by=current_user["id"])
    else:
        result = import_from_custom_csv(content, db, scanned_by=current_user["id"])

    if result.get("success") and result.get("created_samples", 0) > 0:
        from app.services.ml_model import train_model
        train_result = train_model(db)
        result["model_retrained"] = train_result.get("success", False)
        result["model_accuracy"] = train_result.get("accuracy")

    return result


@router.get("/sessions/active/{cs_lecture_id}", summary="Derse ait aktif WiFi session'ı getir")
def get_active_session(
    cs_lecture_id: str,
    db: Session = Depends(get_db),
    _=Depends(get_current_user),
):
    from sqlalchemy import text
    # cs_session_id'si bu lecture'a ait aktif session'ı bul
    wifi_session = db.query(WifiSession).filter(
        WifiSession.cs_session_id == cs_lecture_id,
        WifiSession.is_active == True,
    ).first()

    if not wifi_session:
        return {"active": False, "session_id": None}

    classroom = db.query(Classroom).filter(
        Classroom.id == wifi_session.classroom_id
    ).first()

    return {
        "active": True,
        "session_id": wifi_session.cs_session_id,
        "wifi_session_id": wifi_session.id,
        "classroom": classroom.name if classroom else None,
        "started_at": wifi_session.started_at,
    }


@router.post("/attendance/auto-check-in", response_model=AttendanceCheckInResponse, summary="Otomatik WiFi yoklama - session ID gerekmez")
def auto_wifi_check_in(
    payload: AutoCheckInRequest,
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Student")),
):
    from app.services.attendance_service import auto_check_in_with_wifi
    return auto_check_in_with_wifi(db, current_user["id"], current_user["token"], payload)
