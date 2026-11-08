# Telco Customer Churn Prediction System

A .NET Web API for managing Telco customer data, producing churn predictions through an external machine-learning service, and organizing customer-retention campaigns.

## Features

- Customer records and customer data management
- Bulk customer import from Telco churn CSV files
- Churn prediction through an external ML prediction service
- Retention campaigns and campaign membership/outcome tracking
- Dashboard summary analytics
- ASP.NET Core Identity registration, login, and authenticated API access

## Architecture and Tech Stack

The backend follows a Clean Architecture-style separation:

- **`Churn.Domain`** — business entities, enums, and shared domain types
- **`Churn.Application`** — application services, DTOs, validators, and repository/service interfaces
- **`Churn.Infrastructure`** — EF Core persistence, SQL Server repositories, Identity storage, Redis cache, and the HTTP ML-service client
- **`Churn.API`** — ASP.NET Core controllers, middleware, authentication setup, Swagger, and HTTP endpoints

**Technology:** .NET 9.0, ASP.NET Core Web API, Entity Framework Core 9 with SQL Server, SQL Server 2022, Redis 7, `HybridCache`, and an external ML Prediction Service.

The API container does not include the ML service. Predictions require that service to be running and reachable at the configured `MLService:BaseUrl`.

## Repository Structure

```text
.
├── BackEnd/
│   ├── Churn.API/                 # Web API, controllers, configuration
│   ├── Churn.Application/         # Use cases, DTOs, interfaces, validators
│   ├── Churn.Domain/              # Domain entities and enums
│   ├── Churn.Infrastructure/      # EF Core, SQL Server, Redis, ML client
│   ├── ChurnPredictionSystem.sln
│   ├── Directory.Build.props      # Shared .NET target: net9.0
│   └── Dockerfile
├── DB/
│   ├── ChurnPredictionDB.bak      # SQL Server backup restored by Compose
│   └── Churn_Customers_100.csv    # Sample customer data
├── scripts/
│   └── restore-db.sh              # Compose database initialization
├── docker-compose.yml             # API, SQL Server, Redis, db-init
├── .dockerignore
├── .env.example                   # Local Docker environment template
└── README.md
```

The local `.env` file is intentionally ignored by Git; create it from `.env.example`.

## Getting Started

### Option A: Docker Compose (Recommended)

This runs the API, SQL Server 2022, and Redis in containers. It works with Docker Desktop on Windows and Docker Engine on Linux. Fedora hosts using SELinux should keep the `:Z` bind-mount options in `docker-compose.yml`.

#### Prerequisites

- Docker Engine or Docker Desktop with Compose v2 (`docker compose`)
- A strong SQL Server `sa` password. SQL Server requires a password meeting its complexity policy.

#### 1. Configure the environment

Linux/macOS:

```bash
cp .env.example .env
```

Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and set `MSSQL_SA_PASSWORD` to a strong local development password:

```dotenv
MSSQL_SA_PASSWORD=Replace_With_A_Strong_Password
```

Do not commit `.env`; it may contain credentials.

#### 2. Build and start the services

```bash
docker compose up -d --build
```

Compose starts SQL Server and Redis, then runs the one-shot `db-init` service after SQL Server is healthy. On the first run, `db-init` reads `DB/ChurnPredictionDB.bak`, uses `RESTORE FILELISTONLY` to find the backup's logical data and log file names, and restores the database. If `ChurnPredictionDB` already exists in the persistent SQL volume, the restore step skips it.

Check service status and restore output:

```bash
docker compose ps --all
docker compose logs db-init
docker compose logs -f api
```

#### URLs and ports

| Service | Address |
|---|---|
| API | <http://localhost:8080> |
| Swagger UI (Development environment) | <http://localhost:8080/swagger> |
| Health check | <http://localhost:8080/health> |
| SQL Server from host tools | `127.0.0.1,1433` |
| Redis (Compose network only) | `redis:6379` |

For DBeaver or another SQL client, connect to `127.0.0.1`, port `1433`, database `ChurnPredictionDB`, user `sa`, and the password from `.env`. Enable **Trust server certificate** (equivalent to `TrustServerCertificate=true`). The SQL port is bound to loopback and is not published on external interfaces.

The default ML-service URL in Compose is `http://host.docker.internal:8000/`. Run the ML service on the host at port 8000, or set `ML_SERVICE_BASE_URL` in `.env` to a URL reachable from the API container. The ML service is not started by this Compose file.

#### Useful Docker commands

```bash
# Follow all service logs
docker compose logs -f

# Stop and remove containers and network (keep the SQL volume)
docker compose down

# Stop containers and delete the SQL volume to restore the backup on next start
docker compose down -v

# Rebuild and start the API after source changes
docker compose up -d --build
```

`docker compose down -v` permanently deletes the Compose-managed SQL data volume. Use it only when you want a fresh database restored from the `.bak` file.

### Option B: Local Windows Visual Studio / .NET CLI

Run SQL Server and Redis locally or in separate containers. The API expects both dependencies: SQL Server for persistence and Redis for the distributed cache.

#### 1. Restore the database backup

Restore `DB/ChurnPredictionDB.bak` to a local SQL Server instance using SQL Server Management Studio's **Restore Database** workflow or `sqlcmd`. Ensure the restored database name is `ChurnPredictionDB`. The Compose restore script is intended for its SQL Server container and is not a general Windows restore script.

#### 2. Configure local settings

Set the connection string, Redis connection, and ML-service URL using User Secrets (recommended) or `BackEnd/Churn.API/appsettings.json`. For example:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ChurnPredictionDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project BackEnd/Churn.API
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379" --project BackEnd/Churn.API
dotnet user-secrets set "MLService:BaseUrl" "http://localhost:8000/" --project BackEnd/Churn.API
```

The checked-in `appsettings.json` uses `Server=.` and Windows trusted authentication by default. Change the connection string for your local SQL Server authentication mode and instance. If SQL Server is in a container, use its host-published address and SQL authentication instead. Start Redis locally or provide a reachable Redis endpoint. Start the ML service separately for prediction requests.

#### 3. Run the API

From the repository root:

```powershell
dotnet restore BackEnd/ChurnPredictionSystem.sln
dotnet run --project BackEnd/Churn.API
```

Set `ASPNETCORE_ENVIRONMENT=Development` when needed to enable Swagger. The provided launch profiles define local HTTP/HTTPS development URLs; HTTPS redirection is enabled by the API.

The project does not automatically apply EF Core migrations at startup. The database must already be restored or otherwise provisioned with the required schema.

## Main API Endpoints

Most application endpoints require authentication. The Identity API endpoints are mapped by ASP.NET Core Identity; register and log in at `/register` and `/login`. Swagger supports bearer-token authorization.

| Area | Routes | Description |
|---|---|---|
| Auth | `POST /register`, `POST /login`, `GET /api/auth/me` | Register, obtain an Identity token, and retrieve the authenticated user's identity |
| Customers | `GET/POST /api/customers`, `GET/PUT/DELETE /api/customers/{id}` | List, create, retrieve, update, and delete customers |
| Imports | `POST /api/imports/customers/csv` | Import customer rows as multipart form data using the `file` field; maximum request size is 50 MB |
| Predictions | `GET /api/predictions`, `GET /api/predictions/{id}`, `POST /api/predictions/{customerId}` | List or retrieve predictions, or request a prediction for a customer through the ML service |
| Campaigns | `GET/POST /api/campaigns`, `GET/PUT/DELETE /api/campaigns/{id}` | Manage retention campaigns |
| Campaign customers | `GET /api/campaigns/{id}/customers`, `POST /api/campaigns/{id}/customers`, `PUT /api/campaigns/{id}/customers/{memberId}` | View and manage campaign members and outcomes |
| Dashboard | `GET /api/dashboard/summary` | Retrieve dashboard summary analytics |
| Health | `GET /health` | Anonymous application health response |

Controller routes and request models are the source of truth; use Swagger for the full schemas and response details.
