"""
FastAPI Middleware - IP Adresini Çekme ve Request Logging
"""

from fastapi import Request
from typing import Callable
import logging

logger = logging.getLogger(__name__)


async def extract_client_ip(request: Request, call_next: Callable) -> str:
    """
    Client'ın gerçek IP adresini çek.
    
    Priorite:
    1. X-Forwarded-For (Load Balancer/Proxy)
    2. X-Real-IP (Nginx)
    3. CF-Connecting-IP (Cloudflare)
    4. Client.host (Direct)
    """
    x_forwarded_for = request.headers.get("X-Forwarded-For")
    if x_forwarded_for:
        ip = x_forwarded_for.split(",")[0].strip()
        logger.debug(f"IP from X-Forwarded-For: {ip}")
        request.state.client_ip = ip
        return ip
    
    x_real_ip = request.headers.get("X-Real-IP")
    if x_real_ip:
        logger.debug(f"IP from X-Real-IP: {x_real_ip}")
        request.state.client_ip = x_real_ip
        return x_real_ip
    
    cf_ip = request.headers.get("CF-Connecting-IP")
    if cf_ip:
        logger.debug(f"IP from CF-Connecting-IP: {cf_ip}")
        request.state.client_ip = cf_ip
        return cf_ip
    
    # Direct connection
    ip = request.client.host if request.client else "unknown"
    logger.debug(f"IP from client.host: {ip}")
    request.state.client_ip = ip
    return ip


async def ip_extraction_middleware(request: Request, call_next: Callable):
    """
    Her request'te client IP'sini çek ve request.state'e kaydet.
    """
    client_ip = await extract_client_ip(request, call_next)
    request.state.client_ip = client_ip
    
    response = await call_next(request)
    
    # Response header'ına IP'yi ekle (opsiyonel, debug için)
    response.headers["X-Client-IP"] = client_ip
    
    return response
