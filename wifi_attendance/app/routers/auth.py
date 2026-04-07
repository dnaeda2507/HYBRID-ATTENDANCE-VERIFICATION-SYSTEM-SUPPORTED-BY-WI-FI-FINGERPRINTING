from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.orm import Session
from sqlalchemy import text
import httpx

from app.core.database import get_db
from app.core.security import get_current_user
from app.schemas.schemas import Token, UserOut

router = APIRouter(prefix="/auth", tags=["Auth"])

CS_BACKEND_URL = "https://localhost:9001"


@router.post("/login", response_model=Token)
def login(form: OAuth2PasswordRequestForm = Depends(), db: Session = Depends(get_db)):
    # C# backend'e proxy yap
    try:
        response = httpx.post(
            f"{CS_BACKEND_URL}/api/Account/authenticate/mobile",
            json={"email": form.username, "password": form.password},
            verify=False,
            timeout=5.0,
        )
    except httpx.ConnectError:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="C# backend'e ulaşılamıyor.",
        )

    data = response.json()

    if not data.get("success"):
        errors = data.get("errors", [])
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=errors[0] if errors else "Giriş başarısız",
        )

    # C#'ın token'ını direkt döndür — aynı secret key kullandığımız için geçerli
    return {
        "access_token": data["data"]["jwToken"],
        "token_type": "bearer",
    }


@router.get("/me", response_model=UserOut)
def me(current_user: dict = Depends(get_current_user)):
    return current_user
