# 🛰️ MissionLog

> Enterprise work-order and approval-workflow platform built on ASP.NET Core 8, Blazor WebAssembly, and SignalR — inspired by real aerospace MRO systems.

[![CI](https://github.com/TheAstrelo/MissionLog/actions/workflows/ci.yml/badge.svg)](https://github.com/TheAstrelo/MissionLog/actions)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    MissionLog System                         │
│                                                             │
│  ┌──────────────────┐         ┌──────────────────────────┐  │
│  │  Blazor WASM     │◄───────►│  ASP.NET Core 8 Web API  │  │
│  │  (Frontend)      │  HTTP   │  (Backend)               │  │
│  │                  │◄───────►│                          │  │
│  │  - Dashboard     │ SignalR │  - JWT Authentication    │  │
│  │  - Work Orders   │  WS     │  - Work Order CRUD       │  │
│  │  - Approvals     │         │  - Approval Workflow     │  │
│  │  - Live Updates  │         │  - SignalR Hub           │  │
│  └──────────────────┘         └──────────┬───────────────┘  │
│                                          │ EF Core           │
│                                          ▼                   │
│                               ┌──────────────────────┐      │
│                               │  SQL Server Express  │      │
│                               │  - Users             │      │
│                               │  - WorkOrders        │      │
│                               │  - ApprovalActions   │      │
│                               │  - Comments          │      │
│                               └──────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
MissionLog/
├── src/
│   ├── MissionLog.Core/              # Domain layer
│   │   ├── Entities/                 # WorkOrder, User, ApprovalAction
│   │   ├── Enums/                    # WorkOrderStatus, Priority
│   │   ├── Interfaces/               # IWorkOrderRepository, IUnitOfWork
│   │   └── DTOs/                     # Request/response contracts
│   ├── MissionLog.Infrastructure/    # Data layer
│   │   ├── Data/AppDbContext.cs      # EF Core context + seeding
│   │   ├── Repositories/             # WorkOrderRepository, UserRepository
│   │   └── Services/                 # UnitOfWork, TokenService (JWT)
│   ├── MissionLog.API/               # Web API
│   │   ├── Controllers/              # AuthController, WorkOrdersController
│   │   ├── Hubs/WorkOrderHub.cs      # SignalR real-time hub
│   │   └── Program.cs                # DI, middleware, startup
│   └── MissionLog.BlazorApp/         # Blazor WASM frontend
│       └── Services/                 # ApiService, AuthStateService
└── tests/
    └── MissionLog.Tests/             # xUnit workflow tests
```

## Features

- **JWT Authentication** — Role-based access (Technician, Engineer, Supervisor, Admin)
- **Work Order Lifecycle** — Draft → Submitted → Under Review → Approved → In Progress → Completed
- **Approval Workflow** — Role-gated approve/reject actions with audit trail
- **Real-Time Updates** — SignalR broadcasts status changes to all connected clients live
- **Clean Architecture** — Core / Infrastructure / API separation with Repository + Unit of Work patterns
- **Swagger UI** — Full API documentation at `/swagger`
- **Seeded Demo Data** — 4 demo users out of the box

## Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server Express (free): [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Run Locally

```bash
# Clone
git clone https://github.com/TheAstrelo/MissionLog.git
cd MissionLog

# Run API (auto-migrates DB on first run)
cd src/MissionLog.API
dotnet run

# In a new terminal, run Blazor
cd src/MissionLog.BlazorApp
dotnet run
```

API: `https://localhost:7100`  
Swagger: `https://localhost:7100/swagger`  
Blazor: `https://localhost:7200`

### Demo Accounts

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `supervisor` | `Super123!` | Supervisor |
| `engineer` | `Eng123!` | Engineer |
| `technician` | `Tech123!` | Technician |

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 8) |
| Backend | ASP.NET Core 8 Web API |
| Real-Time | SignalR |
| Auth | JWT (RS256) |
| ORM | Entity Framework Core 8 |
| Database | SQL Server Express |
| Tests | xUnit + Moq + FluentAssertions |
| CI/CD | GitHub Actions |

## Running Tests

```bash
dotnet test tests/MissionLog.Tests
```

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | ❌ | Login |
| POST | `/api/auth/register` | ❌ | Register |
| GET | `/api/workorders` | ✅ | List all work orders |
| GET | `/api/workorders/my` | ✅ | My work orders |
| GET | `/api/workorders/summary` | ✅ | Dashboard summary stats |
| POST | `/api/workorders` | ✅ | Create work order |
| PUT | `/api/workorders/{id}` | ✅ | Update work order |
| POST | `/api/workorders/{id}/submit` | ✅ | Submit for review |
| POST | `/api/workorders/{id}/approve` | 🔒 Supervisor | Approve |
| POST | `/api/workorders/{id}/reject` | 🔒 Supervisor | Reject |
| POST | `/api/workorders/{id}/complete` | ✅ | Mark complete |

## SignalR Events

| Event | Payload | Description |
|---|---|---|
| `WorkOrderCreated` | `WorkOrderDto` | New WO created |
| `WorkOrderUpdated` | `WorkOrderDto` | WO details changed |
| `WorkOrderStatusChanged` | `{ Id, NewStatus, Action, UpdatedBy }` | Status transition |

---

Built by [@TheAstrelo](https://github.com/TheAstrelo)
