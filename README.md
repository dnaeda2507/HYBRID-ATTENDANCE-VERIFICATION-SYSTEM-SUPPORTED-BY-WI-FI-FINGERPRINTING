# Hybrid Attendance Verification System Supported by Wi-Fi Fingerprinting
A **hybrid attendance tracking system** that combines **Wi-Fi fingerprinting** (location-based verification) with a traditional **QR-code / session-token** approach. Students mark attendance via a Flutter mobile app that scans surrounding Wi-Fi access points; a machine-learning service (KNN + Random Forest ensemble) determines whether the student is physically present in the correct classroom.

---

## Table of Contents

1. [Features](#features)
2. [Architecture](#architecture)
3. [Tech Stack](#tech-stack)
4. [Repository Structure](#repository-structure)
5. [Getting Started](#getting-started)
   - [Prerequisites](#prerequisites)
   - [1 – Wi-Fi ML Service (Python / FastAPI)](#1--wi-fi-ml-service-python--fastapi)
   - [2 – Core Backend (C# / ASP.NET Core)](#2--core-backend-c--aspnet-core)
   - [3 – Web Frontend (Next.js)](#3--web-frontend-nextjs)
   - [4 – Mobile App (Flutter)](#4--mobile-app-flutter)
6. [API Overview](#api-overview)
7. [Machine Learning Pipeline](#machine-learning-pipeline)
8. [Demo Credentials](#demo-credentials)


---

## Features

| Feature | Description |
|---|---|
| **Wi-Fi Fingerprinting** | Classroom location verified via RSSI signals from surrounding APs |
| **KNN + Random Forest Ensemble** | Hybrid ML model with time-weighted training samples |
| **Session Management** | Teachers create attendance sessions; students join within a time window |
| **Role-Based Access Control** | SuperAdmin / Admin / Teacher / Student / ItStaff roles |
| **Web Dashboard** | Next.js admin panel — courses, lectures, sessions, attendance reports |
| **Flutter Mobile App** | Students scan Wi-Fi and submit attendance from their phone |
| **QR / Token Fallback** | Secondary verification channel when Wi-Fi signal is ambiguous |
| **Real-time Health Check** | `/health` endpoint on every service for uptime monitoring |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Flutter Mobile App                        │
│   (Dart · wifi_scan · permission_handler · Dio · Provider)      │
└────────────────────────┬────────────────────────────────────────┘
                         │  1. Scan Wi-Fi APs (BSSID / RSSI)
                         │  2. POST /predict  →  classroom_id
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│             Wi-Fi ML Microservice  (FastAPI · Python)            │
│   /auth  /training  /predict  /sessions  /wifi  /health         │
│   KNN + Random Forest · joblib · SQLAlchemy · SQLite/Postgres   │
└────────────────────────┬────────────────────────────────────────┘
                         │  3. Verified classroom_id + session token
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│           Core Backend  (ASP.NET Core 8 · Clean Architecture)    │
│   SessionController · CourseController · UserController          │
│   CQRS (MediatR) · Entity Framework Core · JWT Auth             │
└────────────────────────┬────────────────────────────────────────┘
                         │  4. REST API
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│              Web Frontend  (Next.js 15 · TypeScript)             │
│   /attendance  /courses  /lectures  /users  /settings            │
│   Redux Toolkit · RTK Query · Tailwind CSS                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Mobile | Flutter 3, Dart, `wifi_scan`, `permission_handler`, `mobile_scanner` |
| ML Service | Python 3.11, FastAPI, scikit-learn (KNN + Random Forest), joblib, SQLAlchemy |
| Core Backend | ASP.NET Core 8, Clean Architecture, CQRS + MediatR, Entity Framework Core |
| Web Frontend | Next.js 15, TypeScript, Redux Toolkit, RTK Query, Tailwind CSS |
| Auth | JWT Bearer tokens (shared across all services) |
| Database | PostgreSQL (production) / SQLite (development) |

---

## Repository Structure

```
├── backend/                        # C# ASP.NET Core solution
│   └── CleanArchitecture/
│       ├── CleanArchitecture.Application/   # CQRS commands, queries, DTOs
│       ├── CleanArchitecture.Infrastructure/ # EF Core, migrations, repositories
│       └── CleanArchitecture.WebApi/        # Controllers, middleware, Program.cs
│
├── frontend/
│   ├── atsysweb/                   # Next.js 15 web dashboard
│   │   ├── app/                    # App router pages
│   │   ├── components/             # Reusable UI components
│   │   └── redux/                  # Store, RTK Query generated types
│   └── mobile/
│       └── application/            # Flutter mobile app
│           └── lib/
│               ├── screens/        # Login, Home, Lecture, Attendance, Settings
│               ├── services/       # API, Wi-Fi, Auth services
│               └── models/         # Data models
│
├── wifi_attendance/                # Python FastAPI ML microservice
│   ├── main.py                     # App entry point
│   ├── requirements.txt
│   └── app/
│       ├── routers/                # auth, predict, training, sessions, wifi
│       ├── services/               # ml_model.py, wifi_service.py, attendance_service.py
│       ├── schemas/                # Pydantic request/response models
│       └── core/                   # database.py, security.py
│
└── README.md
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) + [pnpm](https://pnpm.io/)
- [Python 3.11+](https://www.python.org/)
- [Flutter 3.x](https://flutter.dev/docs/get-started/install)
- PostgreSQL (or SQLite for local dev)

---

### 1 – Wi-Fi ML Service (Python / FastAPI)

```bash
cd wifi_attendance
pip install -r requirements.txt
uvicorn main:app --reload --port 8000
```

API docs: http://localhost:8000/docs  
Health: http://localhost:8000/health

---

### 2 – Core Backend (C# / ASP.NET Core)

```bash
cd backend/CleanArchitecture/CleanArchitecture.WebApi
dotnet restore
dotnet run
```

API runs on: https://localhost:9001  
Swagger UI: https://localhost:9001/swagger

> Set your connection string in `appsettings.json` → `ConnectionStrings:DefaultConnection`.

---

### 3 – Web Frontend (Next.js)

```bash
cd frontend/atsysweb
pnpm install
pnpm dev
```

Web app: http://localhost:3000

> Set `NEXT_PUBLIC_API_URL` in `.env.local` to point to the C# backend.

---

### 4 – Mobile App (Flutter)

```bash
cd frontend/mobile/application
flutter pub get
flutter run
```

> Update `API_BASE_URL` in `lib/services/` to match your backend address before running on a physical device.

Build APK for distribution:

```bash
flutter build apk --release
# Output: build/app/outputs/flutter-apk/app-release.apk
```

---

## API Overview

### Wi-Fi ML Service (FastAPI · port 8000)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/auth/login` | All | Get JWT token |
| GET | `/training/classrooms` | Authenticated | List classrooms |
| POST | `/training/classrooms` | Staff/Admin | Add classroom |
| POST | `/training/samples` | Staff/Admin | Add Wi-Fi fingerprint sample |
| POST | `/training/train` | Staff/Admin | Train ML model |
| POST | `/predict` | Student | Predict classroom from Wi-Fi scan |
| GET | `/health` | Public | Service health check |

### Core Backend (ASP.NET Core · port 9001)

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/api/sessions/create` | Teacher/Staff | Create attendance session |
| POST | `/api/sessions/end` | Teacher/Staff | End active session |
| POST | `/api/sessions/attend` | Student | Mark attendance |
| GET | `/api/sessions/get-currentuser-attendances` | Student | View own attendance |
| GET | `/api/courses` | All | List courses |
| GET | `/api/lectures` | All | List lectures |

---

## Machine Learning Pipeline

1. **Data Collection** — IT staff walks through each classroom and records Wi-Fi scans (BSSID + RSSI) via the admin panel.
2. **Feature Engineering** — RSSI values normalised to [0, 1]; feature vector length = total unique BSSIDs seen across all classrooms. Missing BSSIDs → 0.
3. **Time Weighting** — Recent samples weighted higher (last 7 days: 1.0, last 30 days: 0.7, older: 0.4).
4. **Model Training** — KNN (k=5) and Random Forest (100 estimators) combined in a `VotingClassifier` (soft voting). Cross-validated with 5 folds.
5. **Prediction** — Student submits a Wi-Fi scan → feature vector → ensemble prediction → confidence score. If confidence < 0.60, attendance is flagged for manual review.
6. **Persistence** — Model artifacts saved with `joblib` (`wifi_model.joblib`, `bssid_index.joblib`, `label_encoder.joblib`).

---

## Demo Credentials

> Replace with your seeded values before the demo.

| Role | Username | Password |
|---|---|---|
| Admin | `admin@gmail.com` | `1357Abc.` |

| Student | `student@gmail.com` | `1357Abc.` |

---
