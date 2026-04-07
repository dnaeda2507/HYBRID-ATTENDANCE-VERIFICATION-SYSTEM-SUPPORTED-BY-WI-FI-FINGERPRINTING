from pydantic_settings import BaseSettings
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from app.models.models import Base


class Settings(BaseSettings):
    DATABASE_URL: str = "mssql+pymssql://sa:Test_Password@localhost:1433/CSEProjectTest"
    SECRET_KEY: str = "change-this-in-production"
    ALGORITHM: str = "HS256"
    INTERNAL_TOKEN: str = "wifi-ml-internal-secret-2024"
    INTERNAL_TOKEN: str = "wifi-ml-internal-secret-2024"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 3600

    class Config:
        env_file = ".env"


settings = Settings()

engine = create_engine(settings.DATABASE_URL, echo=False)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


def create_tables():
    Base.metadata.create_all(bind=engine)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
