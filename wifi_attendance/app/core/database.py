from pydantic_settings import BaseSettings
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker

class Settings(BaseSettings):
    DATABASE_URL: str = "mssql+pymssql://sa:YourPassword@localhost:1433/CSEProject"  # Dev fallback — prod .env override eder
    SECRET_KEY: str = "change-this-in-production"
    ALGORITHM: str = "HS256"
    JWT_ISSUER: str = "CoreIdentity"
    JWT_AUDIENCE: str = "CoreIdentityUser"
    INTERNAL_TOKEN: str = "wifi-ml-internal-secret-2024"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 3600
    CS_BACKEND_URL: str = "http://backend:80"
    ENVIRONMENT: str = "production"  # "development" = gevşetilmiş IP/BSSID kontrolleri

    class Config:
        env_file = ".env"

settings = Settings()
engine = create_engine(settings.DATABASE_URL, echo=False)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def ensure_schema_compat() -> None:
    """Eski tablolarda eksik kolonları ekle (create_all bunu yapmaz)."""
    from sqlalchemy import text

    migrations = [
        """
        IF NOT EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME='wifi_training_samples' AND COLUMN_NAME='created_at'
        )
        ALTER TABLE wifi_training_samples
        ADD created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
        """,
    ]
    with engine.begin() as conn:
        for sql in migrations:
            conn.execute(text(sql))
