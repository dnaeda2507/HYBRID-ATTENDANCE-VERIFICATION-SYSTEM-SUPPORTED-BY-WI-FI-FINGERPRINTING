import csv
import io
import re
from collections import defaultdict
from datetime import datetime
from typing import Dict, List
from sqlalchemy.orm import Session

from app.models.wifi_models import Classroom, WifiTrainingSample, WifiTrainingAccessPoint

MAC_PATTERN = re.compile(r'^([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}$')


def parse_rssi(value: str):
    if not value or value.strip() == '':
        return None
    cleaned = re.sub(r'[^0-9\-]', '', value.strip())
    try:
        rssi = int(cleaned)
        if -110 <= rssi <= 0:
            return rssi
    except ValueError:
        pass
    return None


def detect_bssid_columns(fieldnames: List[str]) -> List[str]:
    return [col for col in fieldnames if MAC_PATTERN.match(col.strip())]


def import_from_custom_csv(content: str, db: Session, scanned_by: str = None) -> Dict:
    reader = csv.DictReader(io.StringIO(content))

    if not reader.fieldnames:
        return {"success": False, "error": "CSV boş veya başlık satırı yok"}

    bssid_cols = detect_bssid_columns(reader.fieldnames)

    if not bssid_cols:
        return {"success": False, "error": "BSSID kolonu bulunamadı"}

    has_class = 'class' in reader.fieldnames
    has_blook = 'blook' in reader.fieldnames

    created_classrooms = 0
    created_samples = 0
    skipped_rows = 0

    for row in reader:
        if has_blook and has_class:
            classroom_name = f"{row.get('blook', '').strip()}{row.get('class', '').strip()}"
        elif has_class:
            classroom_name = row.get('class', '').strip()
        else:
            classroom_name = "Unknown"

        if not classroom_name or classroom_name == "Unknown":
            skipped_rows += 1
            continue

        aps = []
        for bssid in bssid_cols:
            rssi = parse_rssi(row.get(bssid, ''))
            if rssi is not None:
                aps.append({
                    "bssid": bssid.strip(),
                    "ssid": None,
                    "rssi": rssi,
                    "frequency": None,
                    "channel": None,
                })

        if not aps:
            skipped_rows += 1
            continue

        classroom = db.query(Classroom).filter(Classroom.name == classroom_name).first()

        if not classroom:
            floor = None
            if row.get('floor', '').strip().isdigit():
                floor = int(row.get('floor', '').strip())
            classroom = Classroom(
                name=classroom_name,
                building=row.get('blook', '').strip() or None,
                floor=floor,
            )
            db.add(classroom)
            db.flush()
            created_classrooms += 1

        rp_id = row.get('RP ID', '').strip()
        notes = row.get('açıklama', '').strip()
        sample = WifiTrainingSample(
            classroom_id=classroom.id,
            notes=f"{notes} | {rp_id}" if rp_id else notes or "CSV import",
        )
        db.add(sample)
        db.flush()

        for ap in aps:
            db.add(WifiTrainingAccessPoint(training_sample_id=sample.id, **ap))

        created_samples += 1

    db.commit()

    return {
        "success": True,
        "message": "CSV başarıyla yüklendi ✓",
        "created_classrooms": created_classrooms,
        "created_samples": created_samples,
        "skipped_rows": skipped_rows,
        "detected_bssids": bssid_cols,
    }


def import_from_simple_csv(content: str, db: Session, scanned_by: str = None) -> Dict:
    reader = csv.DictReader(io.StringIO(content))

    if not reader.fieldnames:
        return {"success": False, "error": "CSV boş"}

    required = {"classroom_name", "bssid", "rssi"}
    missing = required - set(reader.fieldnames)
    if missing:
        return {"success": False, "error": f"Eksik kolonlar: {missing}"}

    classroom_aps: Dict[str, List[dict]] = defaultdict(list)

    for row in reader:
        rssi = parse_rssi(row.get("rssi", ""))
        if rssi is None:
            continue
        classroom_aps[row["classroom_name"].strip()].append({
            "bssid": row["bssid"].strip(),
            "ssid": row.get("ssid", "").strip() or None,
            "rssi": rssi,
            "frequency": int(row["frequency"]) if row.get("frequency") else None,
            "channel": int(row["channel"]) if row.get("channel") else None,
        })

    created_classrooms = 0
    created_samples = 0

    for classroom_name, aps in classroom_aps.items():
        classroom = db.query(Classroom).filter(Classroom.name == classroom_name).first()

        if not classroom:
            classroom = Classroom(name=classroom_name)
            db.add(classroom)
            db.flush()
            created_classrooms += 1

        sample = WifiTrainingSample(
            classroom_id=classroom.id,
            notes=f"CSV import - {datetime.utcnow().strftime('%Y-%m-%d %H:%M')}",
        )
        db.add(sample)
        db.flush()

        for ap in aps:
            db.add(WifiTrainingAccessPoint(training_sample_id=sample.id, **ap))

        created_samples += 1

    db.commit()

    return {
        "success": True,
        "message": "CSV başarıyla yüklendi ✓",
        "created_classrooms": created_classrooms,
        "created_samples": created_samples,
    }
