import { configureStore } from "@reduxjs/toolkit";
import { atsysApi } from "./generatedTypes";
import authReducer from "./authSlice";

const store = configureStore({
  reducer: {
    emptyReducer: () => ({}),
    [atsysApi.reducerPath]: atsysApi.reducer,
    auth: authReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(atsysApi.middleware),
});

store.subscribe(() => {
  const { auth } = store.getState();
  if (auth.roles) {
    localStorage.setItem("auth", JSON.stringify(auth));
  } else {
    localStorage.removeItem("auth");
  }
});

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
export default store;
