# Mini Trading Platform – Mobile App

A React Native mobile application built with [Expo](https://expo.dev/).

## Features

- **Authentication** – Register / Login; JWT token stored in AsyncStorage.
- **Instruments** – Browse tradeable instruments and current prices.
- **Order Placement** – Place BUY/SELL, MARKET/LIMIT orders; animated feedback.
- **Trade History** – View all executed trades for the logged-in user.
- **Navigation** – Stack-based navigation; automatic redirect to Login when unauthenticated.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | React Native via Expo |
| Language | TypeScript |
| Navigation | React Navigation (Stack) |
| HTTP client | Axios |
| Storage | AsyncStorage |

## Getting Started

### Prerequisites

- Node.js 18+ and npm 9+
- Expo CLI (`npm install -g expo-cli`) or use `npx expo`
- The backend API accessible from the device/emulator

### Setup

```bash
cp .env.example .env          # edit EXPO_PUBLIC_API_URL if needed
npm install
npx expo start
```

Scan the QR code with [Expo Go](https://expo.dev/client) on your device, or press:
- `a` to open in Android emulator
- `i` to open in iOS simulator (macOS only)
- `w` to open in browser (web preview)

## Project Structure

```
mobile/
├── src/
│   ├── api/             # Axios API service modules
│   ├── context/
│   │   └── AuthContext.tsx
│   ├── navigation/
│   │   └── AppNavigator.tsx
│   └── screens/
│       ├── LoginScreen.tsx
│       ├── RegisterScreen.tsx
│       ├── InstrumentsScreen.tsx
│       ├── PlaceOrderScreen.tsx
│       └── TradesScreen.tsx
├── App.tsx              # Entry point
└── app.json             # Expo configuration
```
