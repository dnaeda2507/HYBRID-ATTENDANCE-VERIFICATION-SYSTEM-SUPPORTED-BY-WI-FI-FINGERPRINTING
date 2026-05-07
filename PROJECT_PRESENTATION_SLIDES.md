# Hybrid Attendance Verification System

### A Secure Attendance Tracking Platform Supported by Wi-Fi Fingerprinting

Akdeniz University

Prepared for academic presentation

---

# 1. Problem Statement

- Conventional attendance procedures are labor-intensive, interrupt lecture flow, and are vulnerable to proxy attendance.
- Password-only or QR-only approaches improve speed but do not reliably prove physical presence in the correct classroom.
- In large classes, the institution needs a method that is scalable, auditable, and resistant to casual misuse.
- Indoor verification is especially difficult because GNSS-based location methods are unreliable inside buildings.

---

# 2. Project Objective

- The project aims to build a secure and automated attendance system that verifies both identity and classroom presence.
- The central idea is a hybrid model: attendance is managed through session control in the core backend, while classroom presence is validated through Wi-Fi fingerprinting.
- The system also preserves operational flexibility through QR-code and password-based fallback methods.
- The target outcome is a deployable university-scale platform rather than a purely theoretical prototype.

---

# 3. Core Contribution

- The project combines administrative attendance management with indoor location verification in one integrated platform.
- It separates concerns through three major application layers: a web administration interface, a mobile student interface, and a machine-learning microservice.
- It extends Wi-Fi classification with practical security controls such as campus-network validation, BSSID whitelisting, and suspicious-event logging.
- It supports role-based governance for IT staff, instructors, academic staff, and students.

---

# 4. System Scope and User Roles

- Students join active attendance sessions and submit attendance from the mobile application.
- Teachers and academic staff manage courses, lectures, and attendance sessions from the web interface.
- IT staff manage user records and operational configuration.
- Administrators can review security-related logs and model-training processes.
- The platform is designed for classroom-centered attendance, not generic geolocation tracking.

---

# 5. Implemented Technology Stack

- Web frontend: Next.js 15, TypeScript, React 19, Redux Toolkit, RTK Query, Tailwind CSS.
- Mobile frontend: Flutter 3 and Dart, with Wi-Fi scanning, QR scanning, secure token storage, and provider-based state management.
- Core backend: ASP.NET Core 8 with Clean Architecture, MediatR-based CQRS, Entity Framework Core, JWT authentication, and Swagger.
- ML service: Python FastAPI with scikit-learn, SQLAlchemy, and joblib.
- Containerized deployment: Docker Compose orchestration with SQL Server, backend API, ML service, and frontend service.

---

# 6. Why This Stack Was Appropriate

- ASP.NET Core and Clean Architecture provide strong separation between domain logic, application services, and infrastructure concerns.
- Next.js offers a maintainable administrative dashboard and efficient integration with OpenAPI-driven client generation.
- Flutter enables a single mobile codebase for student-side attendance operations.
- FastAPI is well suited for an isolated machine-learning service that can evolve independently of the transactional backend.
- Docker-based deployment makes the full platform reproducible for development, testing, and institutional demonstration.

---

# 7. High-Level System Architecture

```text
Flutter Mobile App
   -> Wi-Fi scan / QR scan / password submission
   -> ASP.NET Core Backend
   -> FastAPI ML Service
   -> SQL Server

Next.js Web Dashboard
   -> ASP.NET Core Backend
   -> SQL Server

FastAPI ML Service
   -> predicts classroom match from Wi-Fi fingerprints
   -> returns confidence and suspicion metadata
```

- The backend remains the authoritative source for sessions, users, courses, and attendance records.
- The ML service is delegated the specialized task of classroom inference from radio-signal patterns.

---

# 8. Clean Architecture in the Backend

- The backend solution follows a layered Clean Architecture structure: Application, Infrastructure, and Web API.
- Application contains use cases, DTOs, mappings, validation behaviors, and CQRS handlers.
- Infrastructure handles persistence, repositories, external service integration, and database migrations.
- Web API exposes controllers, authentication middleware, health checks, and API versioning.
- This structure improves maintainability, testability, and controlled growth of domain logic.

---

# 9. Attendance Session Lifecycle

- An instructor selects a course and creates a timed attendance session from the web dashboard.
- The backend generates a session token and a QR code representation of that token.
- The system keeps the session open only within the configured attendance window.
- After the lecture period, the instructor can terminate the session and retrieve a session report.
- The report identifies attended users and flags suspicious cases for later review.

---

# 10. Hybrid Verification Modes

- Mode 1: QR code attendance using a generated session token.
- Mode 2: Password attendance using the same session-bound validation principle.
- Mode 3: Wi-Fi attendance using access-point fingerprints collected from the student device.
- The hybrid design increases operational robustness because attendance can continue even if one channel is temporarily degraded.
- Wi-Fi mode is the distinctive research-oriented component because it attempts to verify true classroom presence.

---

# 11. Wi-Fi Fingerprinting Concept

- A Wi-Fi fingerprint is a vector of surrounding access points and their RSSI values at a specific location.
- Different classrooms exhibit distinguishable signal signatures because of access-point topology, walls, distance, and interference conditions.
- The system collects classroom-specific training samples and uses them to learn a mapping from observed signal patterns to classroom identities.
- During attendance, the student device scans nearby Wi-Fi networks and sends the strongest observations to the backend workflow.

---

# 12. Machine Learning Pipeline

- Training data is collected per classroom as BSSID and RSSI observations.
- Feature engineering constructs vectors over the observed BSSID space and applies RSSI normalization.
- Recent samples are weighted more heavily than older ones to reduce drift from environmental changes.
- The implementation uses a KNN and Random Forest ensemble with cross-validation and optional hyperparameter tuning.
- Model artifacts and training history are persisted for later inspection and retraining.

---

# 13. Prediction Workflow

- The mobile application captures surrounding access points and sends a scan payload to the backend.
- The ASP.NET Core backend stores the scan and forwards the relevant Wi-Fi features to the FastAPI service.
- The ML service predicts the most likely classroom and returns a confidence score together with suspicion metadata.
- The backend compares the predicted classroom with the expected course location.
- Attendance is marked only when the prediction is sufficiently consistent with the session context.

---

# 14. Security Model

- Authentication is based on JWT bearer tokens rather than an external identity provider.
- Authorization is role-based and differentiates between IT staff, teachers, academic staff, and students.
- Session-based attendance codes prevent arbitrary attendance marking outside an active lecture context.
- Wi-Fi verification is strengthened with campus-network checks, BSSID whitelist logic, and suspicious-behavior scoring.
- Every service exposes a health endpoint to support operational monitoring.

---

# 15. Security Scoring and Auditability

- The implementation describes a weighted security score derived from IP validation, BSSID validation, ML confidence, and behavioral patterns.
- Predictions can be classified as trusted, verified, suspicious, or blocked according to score thresholds.
- Suspicious cases are preserved rather than silently discarded, which supports institutional audit and later investigation.
- Logging includes network-level and prediction-level metadata, improving traceability for disputed attendance cases.
- This is important in academic settings where transparency is as important as automation.

---

# 16. User Experience: Web Interface

- The web application provides structured modules for users, lectures, courses, attendance, past sessions, and settings.
- Teachers can create sessions, display QR codes, observe the countdown window, end sessions, and inspect attendance reports.
- IT staff can manage users and role assignments from the dashboard.
- The interface is responsive, with separate sidebar and mobile-header navigation patterns.
- The current visual language is functional and institutional rather than experimental, which fits administrative use.

---

# 17. User Experience: Mobile Interface

- The Flutter application is the student-facing attendance client.
- It supports three attendance actions directly from the lecture screen: password entry, QR scanning, and Wi-Fi submission.
- The mobile client includes secure token storage, QR decoding, permission handling, and Wi-Fi scanning support.
- This design reduces friction for students while preserving the security benefits of session-bound attendance.
- The mobile-first path is essential because Wi-Fi sensing must occur on the student device.

---

# 18. Containerized Deployment Model

- The repository includes Docker Compose definitions for four main services: SQL Server, ASP.NET Core backend, FastAPI ML service, and Next.js frontend.
- Environment variables configure JWT settings, mail settings, internal service tokens, and inter-service base URLs.
- The backend depends on the database, while the ML service and frontend depend on backend availability.
- This deployment style enables consistent multi-service startup for demonstrations and controlled test environments.

---

# 19. Engineering Challenges and Solutions

- Challenge: Verifying physical presence indoors where GPS is weak.
- Solution: Wi-Fi fingerprinting with supervised learning and classroom-specific training data.
- Challenge: Balancing security with classroom usability.
- Solution: Hybrid attendance modes so instruction can continue when one verification channel is inconvenient.
- Challenge: Coordinating business logic across transactional and ML services.
- Solution: A backend-centric orchestration model in which the core API remains the source of truth and the ML service acts as a specialized predictor.

---

# 20. Current Limitations

- Wi-Fi fingerprints are sensitive to environmental drift, infrastructure changes, and device heterogeneity.
- The model quality depends on the coverage and freshness of the classroom training dataset.
- Some security descriptions in project documentation are more ambitious than what has been fully evaluated experimentally.
- Institutional rollout would require broader validation across more buildings, time periods, and student devices.
- Indoor verification should therefore be presented as a probabilistic decision-support mechanism, not an infallible proof source.

---

# 21. Future Work and Closing Remarks

- Extend data collection across additional faculties and lecture halls at Akdeniz University.
- Conduct formal accuracy, false-positive, and false-negative evaluations under varying occupancy and network conditions.
- Enrich the analytics layer with stronger dashboards for attendance trends and security events.
- Investigate multimodal verification by combining Wi-Fi with BLE, device attestation, or timetable-aware anomaly detection.
- The project demonstrates that a hybrid architecture can move attendance management from manual confirmation toward context-aware and security-conscious verification.