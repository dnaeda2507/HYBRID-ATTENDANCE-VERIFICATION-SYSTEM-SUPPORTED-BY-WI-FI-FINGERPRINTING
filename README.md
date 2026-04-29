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
| **KNN + Random Forest Ensemble** | Hybrid ML model with time-weighted training samples + hyperparameter tuning |
| **🔒 IP Address Validation** | **NEW:** Verifies student is on campus network (CIDR-based geo-fencing) |
| **🔒 MAC Address Whitelist** | **NEW:** Only recognizes known legitimate AP's (BSSID validation) |
| **🔒 Security Scoring** | **NEW:** Weighted risk assessment (40% IP + 30% BSSID + 20% ML + 10% behavior) |
| **🔒 Audit Trails** | **NEW:** Complete logging of every prediction with security metadata |
| **🔒 VPN/Proxy Detection** | **NEW:** Flags suspicious network patterns (public IPs, port analysis) |
| **Session Management** | Teachers create attendance sessions; students join within a time window |
| **Role-Based Access Control** | SuperAdmin / Admin / Teacher / Student / ItStaff roles |
| **Web Dashboard** | Next.js admin panel — courses, lectures, sessions, attendance, security audit |
| **Flutter Mobile App** | Students scan Wi-Fi and submit attendance from their phone |
| **QR / Token Fallback** | Secondary verification channel when Wi-Fi signal is ambiguous |
| **Real-time Health Check** | `/health` endpoint on every service for uptime monitoring |
| **Model Auto-Retraining** | **NEW:** Automatic model updates based on data volume or accuracy threshold |
| **Adaptive RSSI Normalization** | **NEW:** Dynamic signal strength preprocessing per environment |

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

#### Authentication & Training
| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/auth/login` | All | Get JWT token |
| GET | `/training/classrooms` | Authenticated | List classrooms |
| POST | `/training/classrooms` | Staff/Admin | Add classroom |
| POST | `/training/samples` | Staff/Admin | Add Wi-Fi fingerprint sample |
| POST | `/training/train` | Staff/Admin | Train ML model (with optional hyperparameter tuning) |
| POST | `/training/upload-csv` | Staff/Admin | Bulk import training data (auto-retrain option) |
| GET | `/training/train/info` | Authenticated | Get current model metadata |
| GET | `/training/train/history` | Staff/Admin | View model training history |

#### Prediction with Security
| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/predict` | Student | **Predict classroom + IP/BSSID validation + security score** |

#### Security Management & Audit
| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/training/security/networks` | Admin/ItStaff | Add campus network CIDR range |
| GET | `/training/security/networks` | Staff/Admin/Teacher | List network configurations |
| POST | `/training/security/bssid-whitelist` | Admin/ItStaff | Add known AP to whitelist |
| GET | `/training/security/bssid-whitelist` | Staff/Admin/Teacher | View whitelisted APs |
| GET | `/training/security/events` | Admin/ItStaff | View security events & suspicious activity |
| GET | `/training/security/audit-logs` | Admin/ItStaff/Teacher | View prediction audit logs with security metadata |
| GET | `/training/security/statistics` | Admin/ItStaff | Get security statistics (success rate, event counts, etc.) |

#### Auto-Retraining
| Method | Endpoint | Role | Description |
|---|---|---|---|
| GET | `/training/retrain/status` | Staff/Admin | Check auto-retrain status |
| POST | `/training/retrain/trigger` | Staff/Admin | Manually trigger model retraining |
| GET | `/training/retrain/history` | Staff/Admin | View retraining history |

#### System Health
| Method | Endpoint | Role | Description |
|---|---|---|---|
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

## Security Architecture

### 2-Layer Security Model

```
Layer 1: Network-Level (IP Validation)
  ├─ CIDR Range Check (campus network geofencing)
  ├─ VPN/Proxy Detection (public IP flagging)
  └─ IP Whitelist/Blacklist

Layer 2: Application-Level (BSSID Validation)
  ├─ BSSID Whitelist (known legitimate APs)
  ├─ Suspicious BSSID Detection
  └─ RSSI Pattern Analysis

Combined: Security Score (0.0-1.0)
  ├─ 40% IP Validation Result
  ├─ 30% BSSID Whitelist Coverage
  ├─ 20% ML Model Confidence
  └─ 10% Time/Pattern Analysis

Status Levels:
  ├─ TRUSTED (≥0.85)    → Accept
  ├─ VERIFIED (≥0.70)   → Accept
  ├─ SUSPICIOUS (≥0.50) → Manual Review
  └─ BLOCKED (<0.50)    → Reject
```

### Audit & Logging

Every prediction request creates complete audit trail:
- **IP Validation Log**: IP address, CIDR match, VPN suspicion
- **Security Event** (if suspicious): Event type, severity, description
- **Prediction Audit**: IP validity, BSSID count/validity, security score, risk factors

---

## Machine Learning Pipeline

1. **Data Collection** — IT staff walks through each classroom and records Wi-Fi scans (BSSID + RSSI) via the admin panel.
2. **Feature Engineering** — RSSI values normalized with adaptive calibration; feature vector includes all unique BSSIDs. Missing BSSIDs → 0.
3. **Time Weighting** — Recent samples weighted higher (last 7 days: 1.0, last 30 days: 0.7, older: 0.4).
4. **Class Balancing** — SMOTE or RandomUnderSampler to handle imbalanced classrooms.
5. **Hyperparameter Tuning** — Optuna-based optimization for KNN k-value and Random Forest depth.
6. **Model Training** — KNN (k=3-15, optimized) and Random Forest (50-150 estimators) combined in a `VotingClassifier` (soft voting). Cross-validated with 5 folds.
7. **Prediction** — Student submits a Wi-Fi scan → feature vector → **IP validation** → **BSSID whitelist check** → ensemble prediction → security scoring → response.
8. **Auto-Retraining** — Automatic model updates when new training data exceeds threshold (e.g., 50+ new samples) or accuracy drops below 75%.
9. **Persistence** — Model artifacts saved with metadata (timestamp, accuracy, feature count, training parameters).

---

## Documentation

- **[Quick Start Guide](./SECURITY_QUICK_START.md)** — IP/MAC validation setup & usage examples
- **[Setup Guide](./SETUP_GUIDE.md)** — Full system deployment & configuration
- **[Optimization Guide](./OPTIMIZATION_GUIDE.md)** — ML model optimization & hyperparameter tuning

---

## Demo Credentials

> Replace with your seeded values before the demo.

| Role | Username | Password |
|---|---|---|
| Admin | `admin@gmail.com` | `1357Abc.` |

| Student | `student@gmail.com` | `1357Abc.` |

---
