# CivicAlert: Standard C# ASP.NET Core 10 Web API Reference Guide

This document explains the standard, idiomatic architecture implemented for the **CivicAlert** Web API, built with **C# ASP.NET Core 10** and **PostgreSQL (Npgsql)**.

---

## 1. Project Directory Layout (The Canonical Standard)

```
CivicAlert/
├── CivicAlert.csproj         # Project configuration & NuGet dependencies
├── Program.cs                # Entry point & composition root (DI & middleware pipeline)
├── appsettings.json          # PostgreSQL ConnectionString, JWT settings, and uploads path
│
├── Controllers/              # Presentation layer (HTTP endpoints, routing, status codes)
│   ├── AuthController.cs     # /api/auth (register, login)
│   ├── ReportController.cs   # /api/report (create, get, list, filter, delete)
│   └── DemoController.cs     # /api/demo (testing pipeline & role security)
│
├── Services/                 # Business logic interfaces & implementations
│   ├── IAuthService.cs & AuthService.cs
│   ├── IReportService.cs & ReportService.cs
│   └── IJwtService.cs & JwtService.cs
│
├── Entities/                 # Core domain models (Database tables)
│   ├── AppUser.cs            # Citizen & Administrator account entity
│   ├── ReportModel.cs        # Municipal issue report entity
│   ├── DemoModel.cs          # Pipeline test entity
│   ├── Role.cs               # Role enum (CLIENT, ADMIN)
│   └── ReportStatus.cs       # ReportStatus enum (OPEN, IN_PROGRESS, SOLVED)
│
├── DTOs/                     # Data Transfer Objects (Contracts for requests & responses)
│   ├── Auth/
│   │   ├── RegisterDto.cs
│   │   ├── RegisterResponseDto.cs
│   │   ├── LoginDto.cs
│   │   └── LoginResponseDto.cs
│   └── Report/
│       ├── CreateReportRequest.cs
│       └── ReportDto.cs
│
├── Data/                     # Data access layer
│   └── CivicAlertDbContext.cs # Entity Framework Core DbContext (Npgsql PostgreSQL)
│
├── Middleware/               # Cross-cutting HTTP middleware
│   └── ExceptionMiddlewareExtensions.cs # Centralized error handling & status code mapping
│
├── Exceptions/               # Domain-specific business exceptions
│   ├── UserAlreadyExistsException.cs
│   ├── InvalidRegisterCredentialsException.cs
│   ├── InvalidLoginCredentialsException.cs
│   └── ReportNotFoundException.cs
│
└── uploads/                  # Physical disk storage for uploaded hazard photos
```

---

## 2. API Endpoints Reference

All endpoints, payload shapes, and status codes maintain 100% parity with the original Spring Boot API:

| HTTP Method & Route | Controller Action | Description | Status Code | Authorization |
| :--- | :--- | :--- | :--- | :--- |
| `POST /api/auth/register` | `AuthController.Register` | Registers a citizen user | `201 Created` | Public |
| `POST /api/auth/login` | `AuthController.Login` | Validates credentials, issues JWT | `200 OK` | Public |
| `POST /api/report` | `ReportController.CreateReport` | Submits civic report with photos | `201 Created` | `CLIENT` role |
| `GET /api/report/{id}` | `ReportController.GetReport` | Retrieves report by numeric ID | `200 OK` | Public |
| `GET /api/report/all` | `ReportController.GetAllReports` | Retrieves all reports | `200 OK` | Public |
| `GET /api/report/all-by-username/{username}` | `ReportController.GetAllReportsByUsername` | Filters reports by user email | `200 OK` | Public |
| `GET /api/report/all-by-category/{category}` | `ReportController.GetAllReportsByCategory` | Filters reports by category | `200 OK` | Public |
| `DELETE /api/report/{id}` | `ReportController.DeleteReport` | Removes a report by ID | `200 OK` | Public |
| `POST /api/demo` | `DemoController.TestDemo` | Creates demo item | `201 Created` | Public |
| `GET /api/demo` | `DemoController.GetDemos` | Retrieves demo items | `200 OK` | `CLIENT` role |
| `GET /uploads/{fileName}` | Static Files Middleware | Serves uploaded images | `200 OK` | Public |

---

## 3. Testing via Swagger UI

Open `http://localhost:3000/` in your browser:
1. Call **`POST /api/auth/login`** using pre-seeded test accounts:
   - **Citizen**: `citizen@civicalert.gov` / `Password123!` (Role: `CLIENT`)
   - **Admin**: `admin@civicalert.gov` / `Admin123!` (Role: `ADMIN`, `CLIENT`)
2. Copy the `accessToken`.
3. Click the green **Authorize** button at top-right, enter `Bearer <your_token>`, and click **Authorize**.
4. Test report creation with photos, filtering by category, or deleting reports directly from your browser.
