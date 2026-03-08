# Mini Trading Platform

> **Disclaimer:** This is an educational/demo project. It is not financial advice and should not be used in any real-money trading context.

A full-stack trading platform demonstrating core software-engineering concepts: REST API, React web frontend, React Native mobile app, background processing, load testing, security testing, and containerised deployment.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database](#database)
- [Testing](#testing)
- [Load Testing](#load-testing)
- [Security Testing](#security-testing)
- [Deployment](#deployment)
- [Project Structure](#project-structure)
- [Contributing](#contributing)

---

## Project Overview

Mini Trading Platform simulates the essential mechanics of an electronic trading venue:

- Users register and log in using JWT-authenticated endpoints.
- Admins manage tradeable instruments (stocks, etc.) and their prices.
- Clients submit BUY or SELL orders (Market or Limit) against those instruments.
- An in-process **matching engine** pairs compatible orders using price-time priority.
- A **billing service** settles executed trades by adjusting wallet balances.
- A **background service** runs periodic reconciliation of open orders.
- A **React web app** provides a browser-based trading interface.
- A **React Native mobile app** provides an on-the-go trading interface.

All data is persisted in **MongoDB**.

---

## Architecture

```
┌──────────────────┐     ┌──────────────────┐
│  React Frontend  │     │  React Native    │
│  (Vite + TS)     │     │  Mobile (Expo)   │
└────────┬─────────┘     └────────┬─────────┘
         │                        │
         └──────────┬─────────────┘
                    │ HTTP / REST
         ┌──────────▼─────────────┐
         │  ASP.NET Core 10 API   │
         │  JWT Auth · Rate Limit │
         │  CORS · Health Checks  │
         ├──────────┬─────────────┤
         │ Matching │  Billing    │
         │ Engine   │  Service    │
         ├──────────┴─────────────┤
         │  Background Service    │
         │  (Reconciliation)      │
         └──────────┬─────────────┘
                    │
         ┌──────────▼─────────────┐
         │       MongoDB          │
         └────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend language | C# 13 |
| Backend framework | ASP.NET Core 10 (`net10.0`) |
| Database | MongoDB (via MongoDB.Driver 3.x) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Password hashing | BCrypt.Net-Next |
| API documentation | Swashbuckle / Swagger UI |
| Background jobs | `IHostedService` / `BackgroundService` |
| Rate limiting | `Microsoft.AspNetCore.RateLimiting` |
| Web frontend | React 18 + TypeScript + Vite 4 |
| Mobile app | React Native via Expo |
| Mobile navigation | React Navigation |
| HTTP client | Axios |
| Load testing | k6 |
| Security scanning | OWASP ZAP + Trivy |
| Containerisation | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## Getting Started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running [MongoDB](https://www.mongodb.com/try/download/community) instance (or use `docker compose up mongodb`)
- Node.js 20+ (for frontend and mobile)

### Quick Start (Docker Compose)

```bash
# Copy and edit environment variables
cp .env.example .env

# Start all services (API + MongoDB + Frontend)
docker compose up -d
```

| Service | URL |
|---|---|
| API | http://localhost:5085 |
| Swagger UI | http://localhost:5085/swagger |
| Health check | http://localhost:5085/health |
| Web frontend | http://localhost:3000 |

### Manual Setup

```bash
# Backend
cd Mini/TradingPlatformAPI
dotnet user-secrets set "MongoDbSettings:ConnectionString" "mongodb://localhost:27017"
dotnet user-secrets set "MongoDbSettings:DatabaseName" "TradingPlatformDb"
dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-key-min-32-chars"
dotnet run

# Frontend
cd frontend
cp .env.example .env
npm install && npm run dev      # http://localhost:3000

# Mobile
cd mobile
cp .env.example .env
npm install && npx expo start
```

---

## Configuration

### Required secrets

| Key | Description |
|---|---|
| `MongoDbSettings:ConnectionString` | MongoDB connection string |
| `MongoDbSettings:DatabaseName` | Target database name |
| `JwtSettings:SecretKey` | HMAC-SHA256 signing key (min 32 characters) |

### CORS

Add allowed origins in `appsettings.json` or via environment variables:

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://your-production-domain.com"]
  }
}
```

---

## Database

MongoDB collections:

| Collection | Model | Description |
|---|---|---|
| `Users` | `User` | Registered users and wallet balances |
| `Instruments` | `Instrument` | Tradeable assets with current prices |
| `Orders` | `Order` | All submitted orders |
| `Trades` | `Trade` | Executed trade records |
| `FixMessages` | `FixMessage` | FIX 4.2 ExecutionReport messages |

---

## Testing

### Unit tests (xUnit)

```bash
dotnet test Mini/TradingPlatformAPI.Tests/TradingPlatformAPI.Tests.csproj
```

31 unit tests covering:
- Authentication (JWT, BCrypt)
- Order placement and cancellation
- Matching engine (full-fill, partial-fill, price validation)
- Billing (wallet debit/credit)
- FIX 4.2 message formatting

---

## Load Testing

Uses [k6](https://k6.io/). See [`load-tests/README.md`](load-tests/README.md) for full instructions.

```bash
# Quick smoke test
k6 run load-tests/smoke.test.js

# Full order-placement load test (50 VUs)
k6 run load-tests/orders.test.js
```

---

## Security Testing

Uses [OWASP ZAP](https://www.zaproxy.org/) and [Trivy](https://trivy.dev/).  
See [`security-tests/README.md`](security-tests/README.md) for full instructions.

```bash
# Passive scan via Docker
docker run --rm \
  -v $(pwd)/security-tests/reports:/zap/wrk \
  ghcr.io/zaproxy/zaproxy:stable \
  zap-baseline.py -t http://host.docker.internal:5085/swagger/v1/swagger.json \
  -r zap-baseline-report.html

# Container image scan
trivy image mini-trading-platform:latest
```

---

## Deployment

### Docker Compose (local / staging)

```bash
docker compose up --build -d
```

### Container images (GitHub Actions CD)

On every push to `main` or a version tag (`v*.*.*`), GitHub Actions builds and pushes images to GitHub Container Registry:

- `ghcr.io/<owner>/mini-trading-api`
- `ghcr.io/<owner>/mini-trading-frontend`

---

## Project Structure

```
Mini_Trading_platform/
├── .github/
│   └── workflows/
│       ├── ci.yml              # Build, test, security scan
│       └── cd.yml              # Docker build & push
├── Mini/
│   ├── TradingPlatformAPI/     # ASP.NET Core Web API
│   │   ├── BackgroundServices/ # IHostedService implementations
│   │   ├── Config/             # Settings POCOs
│   │   ├── Controllers/        # HTTP endpoints
│   │   ├── DTOs/               # Request/response models (with validation)
│   │   ├── Middleware/         # Global exception handler
│   │   ├── Models/             # MongoDB document models
│   │   ├── Repositories/       # Data access layer
│   │   │   └── Interfaces/     # Repository abstractions (SOLID)
│   │   ├── Services/           # Business logic
│   │   └── Program.cs          # DI, middleware, CORS, rate limiting
│   └── TradingPlatformAPI.Tests/   # xUnit unit tests
├── frontend/                   # React + TypeScript web app
│   ├── src/
│   │   ├── api/                # Axios service modules
│   │   ├── components/         # Reusable components
│   │   ├── context/            # React context (AuthContext)
│   │   └── pages/              # Route-level components
│   └── Dockerfile
├── mobile/                     # React Native Expo app
│   ├── src/
│   │   ├── api/                # Axios service modules
│   │   ├── context/            # AuthContext with AsyncStorage
│   │   ├── navigation/         # React Navigation setup
│   │   └── screens/            # Screen components
│   └── App.tsx
├── load-tests/                 # k6 load testing scripts
├── security-tests/             # OWASP ZAP + Trivy configs
├── scripts/                    # Python data-scraping utilities
├── docker-compose.yml
├── .editorconfig
├── .gitignore
├── CONTRIBUTING.md
├── Directory.Build.props
├── global.json
└── README.md
```

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for full guidelines.

---

## License

No license specified. All rights reserved by the repository owner.

---

> **Financial Disclaimer:** This project is provided solely for educational and demonstration purposes. It does not constitute financial advice. Use at your own risk.
