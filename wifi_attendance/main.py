from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.core.database import engine
from app.models.wifi_models import WifiBase
from app.routers import predict, training, auth

app = FastAPI(
    title="WiFi Fingerprinting ML Service",
    description="Sadece konum tahmini yapar. Tüm iş mantığı C# backend'inde.",
    version="2.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.on_event("startup")
def startup():
    WifiBase.metadata.create_all(bind=engine)

app.include_router(auth.router)
app.include_router(predict.router)
app.include_router(training.router)

@app.get("/health")
def health():
    return {"status": "ok", "service": "WiFi ML Service"}
