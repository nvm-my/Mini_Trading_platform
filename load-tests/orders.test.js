/**
 * Load test – order placement endpoint.
 * Tests the complete order flow: register → get instruments → place orders.
 * Ramps up to 50 concurrent virtual users over 2 minutes.
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5085';

const orderErrors = new Counter('order_errors');
const orderSuccessRate = new Rate('order_success_rate');

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '60s', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000', 'p(99)<2000'],
    order_success_rate: ['rate>0.95'],
    http_req_failed: ['rate<0.05'],
  },
};

export function setup() {
  const timestamp = Date.now();
  const email = `order_test_${timestamp}@example.com`;
  const password = 'OrderTest123!';

  const regRes = http.post(
    `${BASE_URL}/api/auth/register`,
    JSON.stringify({ name: 'Order Test User', email, password, role: 'Client' }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  if (regRes.status !== 200) {
    throw new Error(`Setup failed: ${regRes.body}`);
  }

  const token = regRes.json('token');

  // Fetch instrument IDs to use in orders
  const instrRes = http.get(`${BASE_URL}/api/instruments`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  const instruments = instrRes.json() || [];
  const instrumentIds = instruments.map((i) => i.id).filter(Boolean);

  return { email, password, token, instrumentIds };
}

export default function (data) {
  const { instrumentIds } = data;

  // Re-login each iteration to test session management
  const loginRes = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({ email: data.email, password: data.password }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  if (loginRes.status !== 200) {
    orderErrors.add(1);
    return;
  }

  const token = loginRes.json('token');
  const authHeaders = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // Place a LIMIT BUY order if instruments are available
  if (instrumentIds.length > 0) {
    const instrumentId = instrumentIds[Math.floor(Math.random() * instrumentIds.length)];

    const orderRes = http.post(
      `${BASE_URL}/api/orders`,
      JSON.stringify({
        instrumentId,
        side: Math.random() > 0.5 ? 'BUY' : 'SELL',
        orderType: 'LIMIT',
        price: 100 + Math.random() * 50,
        quantity: Math.ceil(Math.random() * 10),
      }),
      authHeaders,
    );

    const success = check(orderRes, {
      'order: status 200': (r) => r.status === 200,
    });

    orderSuccessRate.add(success);
    if (!success) orderErrors.add(1);
  }

  sleep(1);
}
