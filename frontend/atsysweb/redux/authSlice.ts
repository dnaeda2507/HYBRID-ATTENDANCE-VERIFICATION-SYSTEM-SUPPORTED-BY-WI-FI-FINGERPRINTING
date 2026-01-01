import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  id?: string | null;
  roles: string[] | null;
  userName?: string | null;
  email?: string | null;
  isVerified?: boolean;
  jwToken?: string | null;
}

const initialState: AuthState = {
  id: null,
  roles: null,
  userName: null,
  email: null,
  isVerified: false,
  jwToken: null,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{
        roles: AuthState["roles"];
        id: AuthState["id"];
        userName: AuthState["userName"];
        email: AuthState["email"];
        isVerified: AuthState["isVerified"];
        jwToken: AuthState["jwToken"];
      }>
    ) => {
      const { roles, id, userName, email, isVerified, jwToken } =
        action.payload;
      state.roles = roles;
      state.id = id;
      state.userName = userName;
      state.email = email;
      state.isVerified = isVerified;
      state.jwToken = jwToken;
    },
    clearCredentials: (state) => {
      state.roles = null;
      state.id = null;
      state.userName = null;
      state.email = null;
      state.isVerified = false;
      state.jwToken = null;
    },
  },
});

export const { setCredentials, clearCredentials } = authSlice.actions;
export default authSlice.reducer;
