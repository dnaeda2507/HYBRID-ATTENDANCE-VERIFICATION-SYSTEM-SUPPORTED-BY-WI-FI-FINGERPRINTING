"use client";

import { Provider } from "react-redux";
import store from "./store";
import { useEffect } from "react";
import { setCredentials } from "./authSlice";

function Providers({ children }: { children: React.ReactNode }) {
  useEffect(() => {
    const saved = localStorage.getItem("auth");
    if (saved) {
      try {
        const auth = JSON.parse(saved);
        store.dispatch(setCredentials(auth));
      } catch {
        console.warn("Failed to parse saved auth");
      }
    }
  }, []);

  useEffect(() => {
    const unsubscribe = store.subscribe(() => {
      const state = store.getState();
      localStorage.setItem("auth", JSON.stringify(state.auth));
    });
    return () => unsubscribe();
  }, []);

  return <Provider store={store}>{children}</Provider>;
}

export default Providers;
