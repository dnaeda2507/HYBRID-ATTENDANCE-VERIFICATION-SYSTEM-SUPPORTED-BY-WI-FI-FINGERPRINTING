from pydantic_settings import BaseSettings
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from urllib.parse import quote_plus

class Settings(BaseSettings):
    DATABASE_URL: str = ""
    DB_SERVER: str = "localhost"
    DB_PORT: int = 1433
    DB_NAME: str = "CSEProject"
    DB_USER: str = "sa"
    DB_PASSWORD: str = "YourPassword"
    SECRET_KEY: str = "change-this-in-production"
    ALGORITHM: str = "HS256"
    INTERNAL_TOKEN: str = "wifi-ml-internal-secret-2024"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 3600
    CS_BACKEND_URL: str = "http://backend:80"

    class Config:
        env_file = ".env"

settings = Settings()

if settings.DATABASE_URL:
    _db_url = settings.DATABASE_URL
else:
    _db_url = (
        f"mssql+pymssql://{settings.DB_USER}:{quote_plus(settings.DB_PASSWORD)}"
        f"@{settings.DB_SERVER}:{settings.DB_PORT}/{settings.DB_NAME}"
    )

engine = create_engine(_db_url, echo=False)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
