import { getToken, clearToken } from "./auth";

export async function fetchWithAuth(
  input: RequestInfo,
  init: RequestInit = {}
) {
  const token = getToken();
  const headers = new Headers(init.headers);
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(input, { ...init, headers });
  if (res.status === 401) {
    clearToken();
    window.location.href = "/login";
  }
  return res;
}
