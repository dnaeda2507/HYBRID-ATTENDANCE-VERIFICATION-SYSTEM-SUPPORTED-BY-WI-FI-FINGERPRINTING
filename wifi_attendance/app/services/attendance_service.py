from sqlalchemy.orm import Session as DBSession
from datetime import datetime
from fastapi import HTTPException
import httpx

from app.models.wifi_models import (
    WifiSession,
    StudentWifiScan,
    StudentWifiAccessPoint,
)
from app.schemas.schemas import StudentScanCreate, AttendanceCheckInResponse

TIMESTAMP_TOLERANCE_SECONDS = 300
CS_BACKEND_URL = "https://localhost:9001"


def save_scan_to_cs(
    student_id: str,
    session_id: str,
    scanned_at: datetime,
    access_points: list,
    student_token: str,
) -> bool:
    """WiFi taramasını C# backend'e kaydet"""
    try:
        response = httpx.post(
            f"{CS_BACKEND_URL}/api/Wifi/scans",
            json={
                "studentId": student_id,
                "sessionId": int(session_id) if session_id.isdigit() else 0,
                "scannedAtUtc": scanned_at.isoformat(),
                "accessPoints": [
                    {"bssid": ap.bssid, "rssi": ap.rssi}
                    for ap in access_points
                ],
            },
            headers={"Authorization": f"Bearer {student_token}"},
            verify=False,
            timeout=5.0,
        )
        return response.status_code == 200
    except Exception as e:
        print(f"C# WiFi scan kaydı başarısız: {e}")
        return False


def mark_attendance_in_cs(cs_session_id: str, student_token: str) -> bool:
    """C# backend'e attendance kaydı yaptır"""
    try:
        response = httpx.post(
            f"{CS_BACKEND_URL}/api/sessions/attend",
            json={
                "sessionId": int(cs_session_id) if cs_session_id.isdigit() else 0,
                "token": student_token,
            },
            verify=False,
            timeout=5.0,
        )
        data = response.json()
        return response.status_code == 200 and data.get("succeeded", False)
    except Exception as e:
        print(f"C# attendance kaydı başarısız: {e}")
        return False


def check_in_with_wifi(
    db: DBSession,
    student_id: str,
    student_token: str,
    payload: StudentScanCreate,
) -> AttendanceCheckInResponse:

    # 1. WifiSession var mı?
    wifi_session = db.query(WifiSession).filter(
        WifiSession.cs_session_id == payload.session_id
    ).first()
    if not wifi_session:
        raise HTTPException(
            status_code=404,
            detail="Oturum bulunamadı veya WiFi yoklaması başlatılmamış"
        )

    # 2. Session aktif mi?
    if not wifi_session.is_active:
        raise HTTPException(status_code=400, detail="Bu oturum için yoklama artık kapalı")

    # 3. Replay attack kontrolü
    now = datetime.utcnow()
    diff = abs((now - payload.client_timestamp).total_seconds())
    if diff > TIMESTAMP_TOLERANCE_SECONDS:
        raise HTTPException(
            status_code=400,
            detail=f"Geçersiz zaman damgası. Fark: {int(diff)} saniye"
        )

    # 4. Daha önce yoklama aldı mı?
    existing = (
        db.query(StudentWifiScan)
        .filter(
            StudentWifiScan.wifi_session_id == wifi_session.id,
            StudentWifiScan.student_id == student_id,
        )
        .first()
    )
    if existing:
        raise HTTPException(status_code=409, detail="Bu oturum için zaten yoklama alındı")

    # 5. WiFi taramasını FastAPI DB'ye kaydet
    scan = StudentWifiScan(
        student_id=student_id,
        wifi_session_id=wifi_session.id,
        device_info=payload.device_info,
        scanned_at=now,
    )
    db.add(scan)
    db.flush()

    ap_objects = []
    for ap in payload.access_points:
        ap_obj = StudentWifiAccessPoint(
            scan_id=scan.id,
            bssid=ap.bssid,
            ssid=ap.ssid,
            rssi=ap.rssi,
            frequency=ap.frequency,
            channel=ap.channel,
        )
        db.add(ap_obj)
        ap_objects.append(ap_obj)

    db.flush()

    # 6. ML modeli ile konum tahmin et
    from app.services.ml_model import predict_classroom
    matched_classroom_id, confidence = predict_classroom(ap_objects, db)

    scan.matched_classroom_id = matched_classroom_id
    scan.confidence = confidence
    db.commit()

    if matched_classroom_id:
        # 7. C# backend'e WiFi scan kaydet
        save_scan_to_cs(
            student_id=student_id,
            session_id=payload.session_id,
            scanned_at=now,
            access_points=ap_objects,
            student_token=student_token,
        )

        # 8. C# backend'e attendance işaretle
        cs_success = mark_attendance_in_cs(payload.session_id, student_token)

        return AttendanceCheckInResponse(
            success=True,
            message="Konum doğrulandı ve yoklama kaydedildi ✓" if cs_success else "Konum doğrulandı ✓ (C# kaydı beklemede)",
            attendance_id=scan.id,
            matched_classroom=matched_classroom_id,
            confidence=round(confidence, 3),
        )
    else:
        # Konum doğrulanamadı — C#'a scan'i yine de gönder (log için)
        save_scan_to_cs(
            student_id=student_id,
            session_id=payload.session_id,
            scanned_at=now,
            access_points=ap_objects,
            student_token=student_token,
        )
        return AttendanceCheckInResponse(
            success=False,
            message=f"Konum doğrulanamadı. Benzerlik skoru: {round(confidence, 3)}, gereken: 0.60",
            confidence=round(confidence, 3),
        )


def get_session_attendance(db: DBSession, cs_session_id: str):
    wifi_session = db.query(WifiSession).filter(
        WifiSession.cs_session_id == cs_session_id
    ).first()
    if not wifi_session:
        return []
    return db.query(StudentWifiScan).filter(
        StudentWifiScan.wifi_session_id == wifi_session.id
    ).all()


def auto_check_in_with_wifi(
    db: DBSession,
    student_id: str,
    student_token: str,
    payload,
) -> AttendanceCheckInResponse:
    """
    Session ID olmadan otomatik yoklama.
    Tüm aktif WiFi session'larına bakar, WiFi eşleşiyorsa yoklama alır.
    """
    # Tüm aktif session'ları al
    active_sessions = db.query(WifiSession).filter(
        WifiSession.is_active == True
    ).all()

    if not active_sessions:
        raise HTTPException(status_code=404, detail="Şu an aktif WiFi yoklaması bulunmuyor")

    # Replay attack kontrolü
    now = datetime.utcnow()
    diff = abs((now - payload.client_timestamp).total_seconds())
    if diff > TIMESTAMP_TOLERANCE_SECONDS:
        raise HTTPException(
            status_code=400,
            detail=f"Geçersiz zaman damgası. Fark: {int(diff)} saniye"
        )

    # Daha önce bu session'lardan birine yoklama aldı mı?
    for session in active_sessions:
        existing = db.query(StudentWifiScan).filter(
            StudentWifiScan.wifi_session_id == session.id,
            StudentWifiScan.student_id == student_id,
        ).first()
        if existing:
            raise HTTPException(status_code=409, detail="Bu oturum için zaten yoklama alındı")

    # WiFi taramasını geçici olarak oluştur (DB'ye kaydetmeden)
    from app.services.ml_model import predict_classroom
    from app.models.wifi_models import WifiTrainingAccessPoint

    # AP objelerini oluştur
    ap_objects = []
    for ap in payload.access_points:
        ap_obj = type('AP', (), {
            'bssid': ap.bssid,
            'ssid': ap.ssid,
            'rssi': ap.rssi,
            'frequency': ap.frequency,
            'channel': ap.channel,
        })()
        ap_objects.append(ap_obj)

    # ML modeli ile konum tahmin et
    matched_classroom_id, confidence = predict_classroom(ap_objects, db)

    if not matched_classroom_id:
        return AttendanceCheckInResponse(
            success=False,
            message=f"Konum doğrulanamadı. Benzerlik skoru: {round(confidence, 3)}, gereken: 0.60",
            confidence=round(confidence, 3),
        )

    # Eşleşen classroom'a ait aktif session'ı bul
    matched_session = None
    for session in active_sessions:
        if session.classroom_id == matched_classroom_id:
            matched_session = session
            break

    if not matched_session:
        return AttendanceCheckInResponse(
            success=False,
            message=f"Bulunduğunuz derslik ({matched_classroom_id}) için aktif yoklama bulunamadı",
            confidence=round(confidence, 3),
        )

    # Scan'i DB'ye kaydet
    scan = StudentWifiScan(
        student_id=student_id,
        wifi_session_id=matched_session.id,
        device_info=payload.device_info,
        scanned_at=now,
        matched_classroom_id=matched_classroom_id,
        confidence=confidence,
    )
    db.add(scan)
    db.flush()

    # AP'leri kaydet
    for ap in payload.access_points:
        db.add(StudentWifiAccessPoint(
            scan_id=scan.id,
            bssid=ap.bssid,
            ssid=ap.ssid,
            rssi=ap.rssi,
            frequency=ap.frequency,
            channel=ap.channel,
        ))

    db.commit()

    # C# backend'e kaydet
    save_scan_to_cs(
        student_id=student_id,
        session_id=matched_session.cs_session_id,
        scanned_at=now,
        access_points=scan,
        student_token=student_token,
    )
    cs_success = mark_attendance_in_cs(matched_session.cs_session_id, student_token)

    from app.models.wifi_models import Classroom
    classroom = db.query(Classroom).filter(Classroom.id == matched_classroom_id).first()

    return AttendanceCheckInResponse(
        success=True,
        message="Konum doğrulandı ve yoklama kaydedildi ✓" if cs_success else "Konum doğrulandı ✓",
        attendance_id=scan.id,
        matched_classroom=classroom.name if classroom else matched_classroom_id,
        confidence=round(confidence, 3),
    )
