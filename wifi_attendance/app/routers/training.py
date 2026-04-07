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
        scanned_by=current_user["id"],
        notes=payload.notes,
    )
    db.add(sample)
    db.flush()
    for ap in payload.access_points:
        db.add(WifiTrainingAccessPoint(
            sample_id=sample.id,
            bssid=ap.bssid,
            ssid=ap.ssid,
            rssi=ap.rssi,
            frequency=ap.frequency,
        ))
    db.commit()
    db.refresh(sample)
    return {"id": sample.id, "classroom_id": sample.classroom_id, "message": "Örnek eklendi ✓"}


@router.post("/upload-csv", summary="CSV ile toplu eğitim verisi yükle")
async def upload_csv(
    file: UploadFile,
    format: str = "custom",
    db: Session = Depends(get_db),
    current_user: dict = Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
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


@router.post("/train", summary="Modeli eğit")
def train(
    db: Session = Depends(get_db),
    _=Depends(require_role("Admin", "Staff", "ItStaff", "Teacher")),
):
    from app.services.ml_model import train_model
    return train_model(db)


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
