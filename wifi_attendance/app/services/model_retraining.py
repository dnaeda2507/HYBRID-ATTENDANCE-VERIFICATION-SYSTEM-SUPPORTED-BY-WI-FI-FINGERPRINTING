"""
Model Otomatik Retraining Mekanizması

Stratejiler:
1. Veri tabanlı: Yeni veriler eklendiğinde
2. Zaman tabanlı: Belirli aralıklarla
3. Performans tabanlı: Doğruluk düştüğünde
"""

import os
import json
from datetime import datetime, timedelta
from typing import Optional, Dict
from sqlalchemy.orm import Session

from app.models.wifi_models import WifiTrainingSample


MODEL_DIR = "models"
MODEL_METADATA_PATH = f"{MODEL_DIR}/model_metadata.json"
RETRAIN_HISTORY_PATH = f"{MODEL_DIR}/retrain_history.json"

# ─── Retraining Konfigürasyonu ─────────────────────────────────────────────

# Yeni eğitim verisi eklendiğinde retrain et (kaç örnek sonra?)
RETRAIN_ON_NEW_SAMPLES = 10

# Zaman tabanlı retrain (kaç saat sonra?)
RETRAIN_INTERVAL_HOURS = 24

# Minimum veri sayısı (retrain yapılabilmesi için)
MIN_SAMPLES_FOR_RETRAIN = 2


def load_model_metadata() -> Optional[Dict]:
    """Son eğitilmiş model'in metadata'sını yükle"""
    if not os.path.exists(MODEL_METADATA_PATH):
        return None
    
    try:
        with open(MODEL_METADATA_PATH, 'r') as f:
            return json.load(f)
    except:
        return None


def get_last_retrain_time() -> Optional[datetime]:
    """Son retraining'in zamanını al"""
    metadata = load_model_metadata()
    if metadata and "timestamp" in metadata:
        try:
            return datetime.fromisoformat(metadata["timestamp"])
        except:
            pass
    return None


def get_original_sample_count() -> int:
    """Son eğitim sırasındaki örnek sayısını al"""
    metadata = load_model_metadata()
    if metadata and "original_samples" in metadata:
        return metadata["original_samples"]
    return 0


def should_retrain_on_new_samples(db: Session) -> bool:
    """
    Yeni veriler eklenmiş mi ve retrain trigger'ı geçildi mi?
    
    Kontrol: 
    - Son eğitimden sonra RETRAIN_ON_NEW_SAMPLES kadar yeni veri geldi mi?
    """
    current_sample_count = db.query(WifiTrainingSample).count()
    original_count = get_original_sample_count()
    
    new_samples = current_sample_count - original_count
    
    return new_samples >= RETRAIN_ON_NEW_SAMPLES and current_sample_count >= MIN_SAMPLES_FOR_RETRAIN


def should_retrain_on_schedule() -> bool:
    """
    Belirli zaman aralığı geçti mi?
    
    Kontrol:
    - Son eğitimden RETRAIN_INTERVAL_HOURS saat geçti mi?
    """
    last_retrain = get_last_retrain_time()
    
    if last_retrain is None:
        return False
    
    time_elapsed = datetime.utcnow() - last_retrain
    interval = timedelta(hours=RETRAIN_INTERVAL_HOURS)
    
    return time_elapsed > interval


def should_retrain(db: Session) -> bool:
    """
    Retrain yapılmalı mı?
    
    Koşullar:
    1. Yeni veri trigger'ı
    2. Zaman tabanlı trigger
    """
    return (
        should_retrain_on_new_samples(db) or
        should_retrain_on_schedule()
    )


def log_retrain_event(
    reason: str,
    success: bool,
    accuracy: Optional[float] = None,
    sample_count: int = 0
) -> None:
    """Retrain etkinliğini kaydet"""
    history = []
    
    if os.path.exists(RETRAIN_HISTORY_PATH):
        try:
            with open(RETRAIN_HISTORY_PATH, 'r') as f:
                history = json.load(f)
        except:
            pass
    
    entry = {
        "timestamp": datetime.utcnow().isoformat(),
        "reason": reason,
        "success": success,
        "accuracy": accuracy,
        "sample_count": sample_count,
    }
    
    history.append(entry)
    
    # Son 100 etkinliği tut
    if len(history) > 100:
        history = history[-100:]
    
    try:
        with open(RETRAIN_HISTORY_PATH, 'w') as f:
            json.dump(history, f, indent=2, default=str)
    except:
        pass


def auto_retrain_if_needed(db: Session) -> Dict:
    """
    Koşullar sağlanırsa modeli otomatik olarak retrain et.
    
    Döner: {should_retrain, reason, result}
    """
    if not should_retrain(db):
        return {
            "should_retrain": False,
            "reason": "Retrain koşulları sağlanmadı",
            "result": None,
        }
    
    # Retrain nedeni belirle
    reason = "Unknown"
    if should_retrain_on_new_samples(db):
        reason = "Yeni eğitim verileri eklendi"
    elif should_retrain_on_schedule():
        reason = "Zaman tabanlı yeniden eğitim"
    
    try:
        from app.services.ml_model import train_model
        
        result = train_model(db, enable_tuning=True)
        
        log_retrain_event(
            reason=reason,
            success=result.get("success", False),
            accuracy=result.get("accuracy"),
            sample_count=result.get("total_samples", 0),
        )
        
        return {
            "should_retrain": True,
            "reason": reason,
            "result": result,
        }
    
    except Exception as e:
        log_retrain_event(
            reason=reason,
            success=False,
            accuracy=None,
        )
        return {
            "should_retrain": True,
            "reason": reason,
            "result": {"error": str(e)},
        }


def get_retrain_history() -> list:
    """Retrain geçmişini al"""
    if not os.path.exists(RETRAIN_HISTORY_PATH):
        return []
    
    try:
        with open(RETRAIN_HISTORY_PATH, 'r') as f:
            history = json.load(f)
        return history[-50:]  # Son 50
    except:
        return []


def get_retrain_status(db: Session) -> Dict:
    """Retrain durumunu döndür"""
    return {
        "should_retrain_now": should_retrain(db),
        "reason_new_samples": should_retrain_on_new_samples(db),
        "reason_schedule": should_retrain_on_schedule(),
        "last_retrain": get_last_retrain_time().isoformat() if get_last_retrain_time() else None,
        "current_samples": db.query(WifiTrainingSample).count(),
        "original_samples_at_last_train": get_original_sample_count(),
        "next_scheduled_retrain": (
            (get_last_retrain_time() + timedelta(hours=RETRAIN_INTERVAL_HOURS)).isoformat()
            if get_last_retrain_time() else None
        ),
    }
