# Howazit Survey Service

This project implements a survey response ingestion service built with ASP.NET Core 9 (compatible with .NET 6+) that meets the functional and advanced requirements outlined in the brief. The service validates and sanitizes survey responses, processes them asynchronously, persists data to simulated fast storage and SQLite, and exposes real-time metrics, health checks, and observability hooks.

## Features
- REST API (`POST /api/surveys/responses`) with payload validation, sanitization, and asynchronous queuing
- Background worker that persists responses to SQLite (via EF Core) and in-memory fast storage, with retry logic
- AES encryption of sensitive metadata before persistence
- In-memory metrics aggregation (`GET /api/metrics/nps`) and background insights job (`GET /api/metrics/insights`)
- Swagger UI, structured logging, OpenTelemetry metrics (console exporter), and health checks (`/health`)
- Unit tests for core processing components with Cobertura coverage report (`HowazitSurveyService.Tests/coverage/coverage.cobertura.xml`)
- Dockerfile and docker-compose.yml for containerized deployment

## Getting Started

### Prerequisites
- .NET SDK 9.0 (or adjust `TargetFramework` to a compatible version)
- PowerShell or a compatible shell

### Configuration
All configuration is handled through `appsettings.json` (override in environment-specific files or environment variables as needed):

```json
"ConnectionStrings": {
  "Sqlite": "Data Source=HowazitSurveyService.db"
},
"Encryption": {
  "Key": "<base64-encoded 32-byte key>",
  "Iv": "<base64-encoded 16-byte IV>"
},
"SurveyProcessing": {
  "MaxRetryAttempts": 5,
  "RetryDelayMilliseconds": 2000
},
"Insights": {
  "RefreshIntervalSeconds": 60
}
```

> **Note:** Replace the sample encryption key/IV in `appsettings.json` before production use. You can generate compatible secrets with `dotnet user-secrets set` or any AES key generator.

### Database
The service uses SQLite and automatically calls `EnsureCreated()` on startup to create the database file if it doesn't exist. The database file (`HowazitSurveyService.db`) is created in the application directory. For production scenarios, switch to migrations.

### Running Locally
```powershell
dotnet restore
dotnet build
dotnet run
```
The service listens on `http://localhost:5044` (HTTP) or `https://localhost:7290` (HTTPS). Swagger UI is available at `/swagger`.

### Docker
#### Using docker-compose (recommended)
```powershell
docker-compose up -d
```
The service will be available at `http://localhost:32770`. Health check endpoint: `http://localhost:32770/health`.

#### Using Docker directly
```powershell
docker build -t howazit-survey-service .
docker run -p 8080:8080 -e ConnectionStrings__Sqlite="Data Source=HowazitSurveyService.db" howazit-survey-service
```

## API Summary
- `POST /api/surveys/responses` – queues a survey response
- `GET /api/metrics/nps` – returns aggregated NPS per client
- `GET /api/metrics/insights` – returns periodic satisfaction insight snapshot
- `GET /health` – liveness health check
- `GET /swagger` – interactive API documentation

Sample ingestion payload:
```json
{
  "surveyId": "post-purchase",
  "clientId": "client-acme",
  "responseId": "resp-123",
  "responses": {
    "nps_score": 9,
    "satisfaction": "satisfied",
    "custom_fields": {
      "orderId": "ABC-123",
      "comments": "Great experience"
    }
  },
  "metadata": {
    "timestamp": "2024-01-01T10:00:00Z",
    "user_agent": "Mozilla/5.0",
    "ip_address": "203.0.113.10"
  }
}
```

## Testing & Coverage
```powershell
dotnet test
./.dotnet-tools/coverlet HowazitSurveyService.Tests/bin/Debug/net9.0/HowazitSurveyService.Tests.dll `
  --target "dotnet" `
  --targetargs "test --no-build --nologo" `
  --format cobertura `
  --output HowazitSurveyService.Tests/coverage/coverage
```

The Cobertura report is stored at `HowazitSurveyService.Tests/coverage/coverage.cobertura.xml`.

## Observability
- OpenTelemetry metrics with console exporter (extend to OTLP/Prometheus as desired)
- Comprehensive logging across controllers, processors, and background services
- Health checks for SQLite database connectivity (`/health`)

## Project Structure
- `Controllers/` – API endpoints
- `BackgroundServices/` – hosted queue processor and insights refresher
- `Model/` – EF Core DbContext, domain models, DTOs, and entity definitions
  - `Domain/` – domain models
  - `Dtos/` – data transfer objects
  - `Entities/` – database entities
- `Repositories/` – relational and fast-storage repositories
- `Services/` – domain services (messaging, metrics, processing, sanitization, security, submission, insights)
- `Options/` – configuration options
- `Validators/` – FluentValidation validators
- `HowazitSurveyService.Tests/` – xUnit test project
  - `coverage/` – generated coverage reports
- `docs/` – architecture notes and API descriptions

## Next Steps / Enhancements
- Integrate a real SQS/DynamoDB provider for queue and fast storage
- Publish OpenTelemetry traces/metrics to a centralized collector
- Harden retry policies with exponential backoff and dead-letter queues
- Broaden test coverage (integration tests, EF in-memory tests)


