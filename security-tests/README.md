# Security Testing – Mini Trading Platform

This directory contains security testing configurations and scripts.

## Tools Used

| Tool | Purpose |
|---|---|
| [OWASP ZAP](https://www.zaproxy.org/) | Dynamic application security testing (DAST) |
| [Trivy](https://trivy.dev/) | Container image and dependency vulnerability scanning |
| [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) | Code style enforcement |

## Running OWASP ZAP Baseline Scan

### Prerequisites

- Docker installed
- API running on `http://localhost:5085`

```bash
# Pull ZAP image
docker pull ghcr.io/zaproxy/zaproxy:stable

# Run baseline scan (passive scan only – safe for CI)
docker run --rm \
  -v $(pwd)/security-tests/reports:/zap/wrk \
  ghcr.io/zaproxy/zaproxy:stable \
  zap-baseline.py \
    -t http://host.docker.internal:5085/swagger/v1/swagger.json \
    -r zap-baseline-report.html \
    -J zap-baseline-report.json

# Run full active scan (use only against non-production)
docker run --rm \
  -v $(pwd)/security-tests/reports:/zap/wrk \
  ghcr.io/zaproxy/zaproxy:stable \
  zap-full-scan.py \
    -t http://host.docker.internal:5085 \
    -r zap-full-report.html
```

## Running Trivy Container Scan

```bash
# Scan the Docker image built from the project
trivy image mini-trading-platform:latest

# Scan for known vulnerabilities in NuGet packages
trivy fs --scanners vuln Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
```

## OWASP ZAP Automation Framework Config

The file `zap-automation.yaml` defines a reusable ZAP automation plan for CI/CD integration.

```bash
docker run --rm \
  -v $(pwd)/security-tests:/zap/wrk \
  ghcr.io/zaproxy/zaproxy:stable \
  zap.sh -cmd -autorun /zap/wrk/zap-automation.yaml
```

## Security Controls Implemented in the API

| Control | Implementation |
|---|---|
| Authentication | JWT Bearer tokens (HMAC-SHA256) |
| Password storage | BCrypt with adaptive cost factor |
| Authorization | Role-based (`Admin` / `Client`) |
| Input validation | `[Required]`, `[Range]`, `[RegularExpression]` on all DTOs |
| Error messages | Global exception handler – no stack traces exposed |
| Rate limiting | Fixed-window (100 req/min) via `AspNetCore.RateLimiting` |
| CORS | Allowlist-based policy (`FrontendPolicy`) |
| Transport security | HTTPS redirection enforced |
