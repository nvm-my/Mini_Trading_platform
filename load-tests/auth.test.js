/**
 * Load test – authentication endpoints (Register + Login).
 * Ramps up to 20 concurrent virtual users over 1 minute.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5085';

// Custom metrics
const loginErrors = new Counter('login_errors');
const loginRate = new Rate('login_success_rate');
const loginDuration = new Trend('login_duration_ms');

export const options = {
  stages: [
    { duration: '15s', target: 5 },
    { duration: '30s', target: 20 },
    { duration: '15s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    login_success_rate: ['rate>0.99'],
  },
};

// Shared credentials created once by the setup function
export function setup() {
  const email = `loadtest_${Date.now()}@example.com`;
  const password = 'LoadTest123!';

  const res = http.post(
    `${BASE_URL}/api/auth/register`,
    JSON.stringify({ name: 'Load Test User', email, password, role: 'Client' }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  if (res.status !== 200) {
    throw new Error(`Setup registration failed: ${res.status} ${res.body}`);
  }

  return { email, password };
}

export default function ({ email, password }) {
  const start = Date.now();

  const res = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({ email, password }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  const success = check(res, {
    'login: status 200': (r) => r.status === 200,
    'login: token present': (r) => !!r.json('token'),
  });

  loginDuration.add(Date.now() - start);
  loginRate.add(success);
  if (!success) loginErrors.add(1);

  sleep(1);
}
