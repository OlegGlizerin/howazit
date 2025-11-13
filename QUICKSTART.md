# Quick Start Guide

## Prerequisites
- Docker Desktop

## Run with Docker Compose

```powershell
cd HowazitSurveyService
docker-compose up -d
```

## Access the API

- **Swagger UI**: http://localhost:32770/swagger
- **Health Check**: http://localhost:32770/health
- **API**: http://localhost:32770/api

## View Logs

```powershell
docker-compose logs -f survey-api
```

## Stop the Service

```powershell
docker-compose down
```

