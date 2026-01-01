const TOKEN_KEY = "token";

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}
export function setToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token);
}
export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function parseJwt(token: string): { exp: number; [k: string]: unknown } {
  const [, payload] = token.split(".");
  const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
  return JSON.parse(
    decodeURIComponent(
      json
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join("")
    )
  );
}

let expiryTimer: number | null = null;
export function scheduleLogout(onExpired: () => void) {
  const token = getToken();
  if (!token) return;
  const { exp } = parseJwt(token);
  const ms = exp * 1000 - Date.now();
  if (ms <= 0) {
    onExpired();
  } else {
    if (expiryTimer != null) clearTimeout(expiryTimer);
    expiryTimer = window.setTimeout(onExpired, ms);
  }
}

export function logout() {
  clearToken();
  if (expiryTimer != null) clearTimeout(expiryTimer);
  window.location.href = "/login";
}
