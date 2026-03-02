# Mini Trading Platform

> **Disclaimer:** This is an educational/demo project. It is not financial advice and should not be used in any real-money trading context.

A lightweight REST API built with **ASP.NET Core (.NET 10)** that demonstrates the core concepts of a trading platform: user authentication, instrument management, order placement with a basic matching engine, and wallet/billing settlement.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database](#database)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

---

## Project Overview

Mini Trading Platform is a back-end API that simulates the essential mechanics of an electronic trading venue:

- Users register and log in using JWT-authenticated endpoints.
- Admins manage tradeable instruments (stocks, etc.) and their prices.
- Clients submit BUY or SELL orders (Market or Limit) against those instruments.
- An in-process **matching engine** pairs compatible orders using price-time priority.
- A **billing service** settles executed trades by adjusting wallet balances.

All data is persisted in **MongoDB**.

---

## Key Features

| Feature | Details |
|---|---|
| **Authentication** | Register / Login with BCrypt password hashing and JWT tokens |
| **Role-Based Access** | `Admin` and `Client` roles enforced via `[Authorize(Roles = "...")]` |
| **Instrument Management** | Admins create instruments and update current prices |
| **Order Management** | Place BUY/SELL, Market/Limit orders; cancel open orders |
| **Matching Engine** | Price-time priority matching; statuses: `OPEN`, `PARTIAL`, `FILLED`, `CANCELLED` |
| **Trade History** | Retrieve all trades executed for the logged-in user |
| **Wallet / Billing** | Automatic balance debit (buyer) and credit (seller) on trade settlement |
| **Swagger UI** | Interactive API docs available in Development mode at `/swagger` |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 13 |
| Framework | ASP.NET Core 10 (`net10.0`) |
| Database | MongoDB (via MongoDB.Driver 3.x) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Password Hashing | BCrypt.Net-Next |
| API Documentation | Swashbuckle / Swagger UI |
| SDK Pin | .NET SDK 10.0.x (see `global.json`) |

---

## Getting Started

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A running [MongoDB](https://www.mongodb.com/try/download/community) instance (local or cloud, e.g. Atlas)

### Clone the repository

```bash
git clone https://github.com/nvm-my/Mini_Trading_platform.git
cd Mini_Trading_platform
```

### Restore dependencies

```bash
dotnet restore Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
```

### Build

```bash
dotnet build Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
```

### Run

```bash
dotnet run --project Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
```

The API will start on `http://localhost:5085` (HTTP) or `https://localhost:7237` (HTTPS).

Open `http://localhost:5085/swagger` in your browser to explore the API interactively (Development mode only).

---

## Configuration

Configuration is handled through the standard ASP.NET Core `appsettings.json` / environment-variable layering.

### `appsettings.json` (checked-in defaults)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Required secrets (do NOT commit to source control)

Create a `appsettings.Development.json` (already git-ignored for non-default environments) or use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local development:

```bash
cd Mini/TradingPlatformAPI
dotnet user-secrets set "MongoDbSettings:ConnectionString" "mongodb://localhost:27017"
dotnet user-secrets set "MongoDbSettings:DatabaseName"    "TradingPlatformDb"
dotnet user-secrets set "JwtSettings:SecretKey"           "your-super-secret-key-min-32-chars"
```

| Key | Description |
|---|---|
| `MongoDbSettings:ConnectionString` | MongoDB connection string |
| `MongoDbSettings:DatabaseName` | Target database name |
| `JwtSettings:SecretKey` | HMAC-SHA256 signing key (min 32 characters) |

For production deployments, supply these as environment variables or through a secrets manager (Azure Key Vault, AWS Secrets Manager, etc.).

---

## Database

The platform uses **MongoDB** — a schema-less document database. No migration scripts are needed; collections are created automatically when the application first writes a document.

### Collections

| Collection | Model | Description |
|---|---|---|
| `users` | `User` | Registered users and their wallet balances |
| `instruments` | `Instrument` | Tradeable assets with current prices |
| `orders` | `Order` | All submitted orders |
| `trades` | `Trade` | Executed trade records |

### Seed data

There is no automated seed script. Use the Swagger UI or a tool such as [MongoDB Compass](https://www.mongodb.com/products/compass) or `mongosh` to insert initial instruments:

```js
db.instruments.insertOne({
  name: "ACME Corp",
  symbol: "ACME",
  currentPrice: 100.00,
  isActive: true
})
```

---

## Testing

There are no automated tests in the repository at this time.  
To add tests, create an xUnit (or NUnit) project under `Mini/TradingPlatformAPI.Tests/` and reference the main project.

```bash
dotnet new xunit -n TradingPlatformAPI.Tests -o Mini/TradingPlatformAPI.Tests
dotnet add Mini/TradingPlatformAPI.Tests/TradingPlatformAPI.Tests.csproj reference \
    Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
dotnet test Mini/TradingPlatformAPI.Tests/TradingPlatformAPI.Tests.csproj
```

---

## Project Structure

```
Mini_Trading_platform/
├── .editorconfig                  # Code style / formatting rules
├── .gitignore
├── CONTRIBUTING.md
├── Directory.Build.props          # Shared MSBuild properties & analyzers
├── global.json                    # Pinned .NET SDK version
├── README.md
└── Mini/
    └── TradingPlatformAPI/        # ASP.NET Core Web API project
        ├── Config/
        │   ├── JwtSettings.cs     # JWT configuration POCO
        │   └── MongoDbSettings.cs # MongoDB configuration POCO
        ├── Controllers/
        │   ├── AuthController.cs        # POST /api/auth/register, /login
        │   ├── InstrumentController.cs  # GET/POST /api/instruments
        │   ├── OrderController.cs       # POST/DELETE /api/orders
        │   ├── TradeController.cs       # GET /api/trades/my
        │   └── UserController.cs        # User profile endpoints
        ├── DTOs/
        │   ├── LoginDTO.cs
        │   ├── OrderDTO.cs
        │   └── RegisterDTO.cs
        ├── Models/
        │   ├── Instrument.cs
        │   ├── Order.cs
        │   ├── Trade.cs
        │   ├── User.cs
        │   └── Wallet.cs
        ├── Repositories/
        │   ├── InstrumentRepository.cs
        │   ├── OrderRepository.cs
        │   ├── TradeRepository.cs
        │   └── UserRepository.cs
        ├── Services/
        │   ├── AuthService.cs           # Registration & login with JWT generation
        │   ├── BillingService.cs        # Wallet debit/credit on trade settlement
        │   ├── MatchingEngineService.cs # Price-time priority order matching
        │   ├── OrderService.cs          # Place & cancel orders
        │   └── WalletService.cs        # (reserved for future wallet operations)
        ├── Program.cs
        ├── appsettings.json
        └── TradingPlatformAPI.csproj
```

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for full guidelines.

Short summary:
1. Fork the repository and create a feature branch from `main`.
2. Follow the code style enforced by `.editorconfig` and run `dotnet format` before submitting.
3. Keep pull requests focused — one concern per PR.
4. Add or update tests when changing business logic.
5. Open a pull request and fill in the PR template checklist.

---

## License

No license specified. All rights reserved by the repository owner.

---

> **Financial Disclaimer:** This project is provided solely for educational and demonstration purposes. It does not constitute financial advice, investment advice, or a recommendation to buy or sell any financial instrument. Use at your own risk.
