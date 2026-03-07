# ── Build stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Mini/TradingPlatformAPI/TradingPlatformAPI.csproj Mini/TradingPlatformAPI/
RUN dotnet restore Mini/TradingPlatformAPI/TradingPlatformAPI.csproj

COPY Mini/TradingPlatformAPI/ Mini/TradingPlatformAPI/
WORKDIR /src/Mini/TradingPlatformAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TradingPlatformAPI.dll"]
