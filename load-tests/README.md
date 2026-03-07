# Load Testing – Mini Trading Platform

Load tests are implemented with [k6](https://k6.io/), an open-source load testing tool.

## Prerequisites

- [k6](https://k6.io/docs/getting-started/installation/) installed locally  
  or run via Docker: `docker run --rm -i grafana/k6 run - < <script>.js`

## Scripts

| Script | Description |
|---|---|
| `auth.test.js` | Login / Register endpoint throughput |
| `instruments.test.js` | Instrument listing under load |
| `orders.test.js` | Order placement with realistic ramp-up |
| `smoke.test.js` | Quick 1-VU sanity check across all endpoints |

## Running

```bash
# Smoke test (quick health check)
k6 run load-tests/smoke.test.js

# Full load test
k6 run load-tests/orders.test.js

# With environment override
k6 run --env BASE_URL=http://api.example.com load-tests/orders.test.js
```

## Configuration

Set the `BASE_URL` environment variable to target a non-default deployment.
Default: `http://localhost:5085`
