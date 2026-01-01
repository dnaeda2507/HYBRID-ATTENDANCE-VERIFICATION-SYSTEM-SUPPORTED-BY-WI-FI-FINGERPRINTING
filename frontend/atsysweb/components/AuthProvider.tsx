"use client";

import { useEffect, useState, ReactNode } from "react";
import { usePathname, useRouter } from "next/navigation";
import LoginPage from "@/app/login/page";
import {
  clearToken,
  getToken,
  parseJwt,
  scheduleLogout,
} from "@/app/utils/auth";

export default function AuthProvider({ children }: { children: ReactNode }) {
  const [checked, setChecked] = useState(false);
  const [authed, setAuthed] = useState(false);
  const router = useRouter();
  const path = usePathname();

  useEffect(() => {
    const token = getToken();
    if (!token) {
      setAuthed(false);
      if (path !== "/login") router.replace("/login");
    } else {
      const { exp } = parseJwt(token);
      if (exp * 1000 <= Date.now()) {
        clearToken();
        setAuthed(false);
        if (path !== "/login") router.replace("/login");
      } else {
        scheduleLogout(() => {
          clearToken();
          router.replace("/login");
        });
        setAuthed(true);
        if (path === "/login") router.replace("/");
      }
    }
    setChecked(true);
  }, [path, router]);

  if (!checked) return null;
  if (!authed) return <LoginPage />;
  return <>{children}</>;
}
