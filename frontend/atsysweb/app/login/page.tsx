"use client";
import React, { useState } from "react";
import "./login.css";
import Image from "next/image";
import { scheduleLogout, setToken } from "../utils/auth";
import { useRouter } from "next/navigation";
import {
  AuthenticationRequest,
  usePostApiAccountAuthenticateWebMutation,
} from "@/redux/generatedTypes";
import { toast, Toaster } from "react-hot-toast";
import { useDispatch } from "react-redux";
import { setCredentials } from "@/redux/authSlice";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [authenticateAsync] = usePostApiAccountAuthenticateWebMutation();
  const router = useRouter();
  const dispatch = useDispatch();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const credentials: AuthenticationRequest = {
      email,
      password,
    };
    const promise = authenticateAsync({
      authenticationRequest: credentials,
    }).unwrap();

    toast.promise(
      async () => {
        const res = await promise;
        return res;
      },
      {
        loading: "Logging in...",
        success: (res) => {
          if (res.success && res.data && res.data.jwToken) {
            setToken(res.data.jwToken);
            scheduleLogout(() => {
              window.location.href = "/login";
            });
            dispatch(
              setCredentials({
                roles: res.data.roles ?? null,
                id: res.data.id,
                userName: res.data.userName,
                email: res.data.email,
                isVerified: res.data.isVerified,
                jwToken: res.data.jwToken,
              })
            );
            router.replace("/");
            return res.message ?? "Login successful";
          } else {
            throw new Error(
              `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
            );
          }
        },
        error: (error) => {
          return `Login failed: ${error.message}`;
        },
      }
    );
  };

  return (
    <div className="login-container">
      <Image
        src="/image/akdu_logo.png"
        alt="Logo"
        width={100}
        height={100}
        className="logo"
      />
      <h2>Welcome</h2>
      <p className="subtitle">Login to your account</p>
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <form onSubmit={handleSubmit}>
        <div className="input-group">
          <span className="icon">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={1.5}
              stroke="currentColor"
              className="size-5 text-blue-500"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75"
              />
            </svg>
          </span>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="input-group">
          <span className="icon">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={1.5}
              stroke="currentColor"
              className="size-5 text-blue-500"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M16.5 10.5V6.75a4.5 4.5 0 1 0-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 0 0 2.25-2.25v-6.75a2.25 2.25 0 0 0-2.25-2.25H6.75a2.25 2.25 0 0 0-2.25 2.25v6.75a2.25 2.25 0 0 0 2.25 2.25Z"
              />
            </svg>
          </span>
          <input
            type={showPassword ? "text" : "password"}
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <button
            type="button"
            className="password-toggle"
            onClick={() => setShowPassword((current) => !current)}
            aria-label={showPassword ? "Hide password" : "Show password"}
            aria-pressed={showPassword}
          >
            {showPassword ? (
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                className="size-5 text-blue-500"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M3.98 8.223A10.477 10.477 0 0 0 1.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0 1 12 4.5c4.756 0 8.773 3.162 10.065 7.5a10.523 10.523 0 0 1-4.293 5.774M6.228 6.228 3 3m3.228 3.228 3.65 3.65m0 0a3 3 0 1 0 4.243 4.243m-4.243-4.243 4.243 4.243m0 0L21 21"
                />
              </svg>
            ) : (
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                className="size-5 text-blue-500"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z"
                />
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z"
                />
              </svg>
            )}
          </button>
        </div>

        <div className="options">
          <label>
            <input type="checkbox" /> Remember me
          </label>
          {/* <a href="#">Forgot Password?</a> */}
        </div>

        <button type="submit" className="login-btn">
          Login
        </button>
      </form>
    </div>
  );
}
