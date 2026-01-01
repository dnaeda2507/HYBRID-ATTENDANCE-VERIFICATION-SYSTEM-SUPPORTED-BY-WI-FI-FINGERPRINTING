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
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
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
