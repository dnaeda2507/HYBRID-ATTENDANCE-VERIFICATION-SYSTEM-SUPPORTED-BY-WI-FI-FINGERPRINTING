from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.core.database import engine
from app.models.wifi_models import WifiBase
from app.routers import predict, training, auth
from app.core.middleware import ip_extraction_middleware

app = FastAPI(
    title="WiFi Fingerprinting ML Service",
    description="Sadece konum tahmini yapar. Tüm iş mantığı C# backend'inde.",
    version="2.0.0-secure",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# IP Extraction Middleware (function-based → @app.middleware decorator)
@app.middleware("http")
async def _ip_middleware(request, call_next):
    return await ip_extraction_middleware(request, call_next)

@app.on_event("startup")
def startup():
    WifiBase.metadata.create_all(bind=engine)
    
    # Security tabloları ve varsayılan konfigürasyonları initialize et
    from app.core.database import get_db
    from app.services.security_validation import initialize_default_networks
    
    try:
        db = next(get_db())
        initialize_default_networks(db)
        db.close()
    except Exception as e:
        print(f"Startup initialization hatası: {e}")

app.include_router(auth.router)
app.include_router(predict.router)
app.include_router(training.router)

@app.get("/health")
def health():
    return {"status": "ok", "service": "WiFi ML Service", "version": "2.0.0-secure"}
