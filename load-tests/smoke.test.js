/**
 * Smoke test – runs a single virtual user through the happy path once.
 * Purpose: verify the API is reachable and core flows return 2xx.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5085';

export const options = {
  vus: 1,
  iterations: 1,
};

export default function () {
  // 1. Register a new user
  const registerPayload = JSON.stringify({
    name: `Smoke User ${Date.now()}`,
    email: `smoke_${Date.now()}@example.com`,
    password: 'SmokePass123!',
    role: 'Client',
  });

  const registerRes = http.post(`${BASE_URL}/api/auth/register`, registerPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(registerRes, { 'register: status 200': (r) => r.status === 200 });

  const token = registerRes.json('token');

  const authHeaders = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // 2. Get instruments
  const instrumentsRes = http.get(`${BASE_URL}/api/instruments`, authHeaders);
  check(instrumentsRes, { 'instruments: status 200': (r) => r.status === 200 });

  // 3. Get trades
  const tradesRes = http.get(`${BASE_URL}/api/trades/my`, authHeaders);
  check(tradesRes, { 'trades: status 200': (r) => r.status === 200 });

  // 4. Health check
  const healthRes = http.get(`${BASE_URL}/health`);
  check(healthRes, { 'health: status 200': (r) => r.status === 200 });

  sleep(1);
}
