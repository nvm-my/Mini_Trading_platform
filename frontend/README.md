# Mini Trading Platform – Web Frontend

A React + TypeScript single-page application built with [Vite](https://vitejs.dev/).

## Features

- **Authentication** – Register and login; JWT token stored in `localStorage`.
- **Instruments** – Browse all tradeable instruments and their current prices.
- **Order Placement** – Place BUY/SELL, MARKET/LIMIT orders against any instrument.
- **Trade History** – View all executed trades for the logged-in user.
- **Protected Routes** – Unauthenticated users are redirected to the login page.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | React 18 |
| Language | TypeScript |
| Bundler | Vite 4 |
| Routing | React Router DOM v6 |
| HTTP client | Axios |

## Getting Started

### Prerequisites

- Node.js 18+ and npm 9+
- The backend API running on `http://localhost:5085` (or set `VITE_API_URL`)

### Setup

```bash
cp .env.example .env          # edit VITE_API_URL if needed
npm install
npm run dev                   # starts at http://localhost:3000
```

### Build for production

```bash
npm run build
npm run preview               # previews the production build locally
```

## Project Structure

```
frontend/
├── src/
│   ├── api/              # Axios-based API service modules
│   │   ├── client.ts     # Shared Axios instance + JWT interceptor
│   │   ├── auth.ts
│   │   ├── instruments.ts
│   │   ├── orders.ts
│   │   └── trades.ts
│   ├── components/
│   │   └── ProtectedRoute.tsx
│   ├── context/
│   │   └── AuthContext.tsx   # JWT auth state + hooks
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── InstrumentsPage.tsx
│   │   ├── PlaceOrderPage.tsx
│   │   └── TradesPage.tsx
│   ├── App.tsx           # Route configuration
│   └── main.tsx          # Entry point
├── .env.example
└── vite.config.ts
```
