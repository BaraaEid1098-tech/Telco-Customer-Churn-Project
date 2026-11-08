# Telco Customer Churn Prediction

A GitHub-ready monorepo for customer churn data exploration, reproducible model training, HTTP inference, customer management, and retention campaigns.

## Architecture

```text
Flutter UI (UI placeholder)
          |
          v
ASP.NET Core 9 API (BackEnd/Churn.API)
   |          |             |
   v          v             v
SQL Server   Redis     FastAPI ML Service
                         |           |
                         v           v
                   Model artifacts  SHAP
```

- **Flutter UI:** `UI/` is reserved for the client; it currently contains only a placeholder.
- **ASP.NET Core API:** Clean Architecture projects under `BackEnd/`; owns customer, prediction, campaign, authentication, and dashboard endpoints.
- **FastAPI ML service:** `ML/app/`; loads the checked-in preprocessing contract and trained models and serves predictions and SHAP explanations.
- **SQL Server 2022:** stores customers, predictions, campaigns, and campaign outcomes. Compose restores the included backup on first startup.
- **Redis 7:** distributed cache used by the API.

## Repository layout

```text
BackEnd/
  Churn.API/                  ASP.NET Core controllers and startup
  Churn.Application/          use cases, DTOs, service contracts
  Churn.Domain/               entities and enums
  Churn.Infrastructure/       EF Core, SQL Server, Redis, ML HTTP client
  ChurnPredictionSystem.sln
  Dockerfile
DB/
  ChurnPredictionDB.bak
  Churn_Customers_100.csv
ML/
  app/                        FastAPI schemas, predictor, routes
  artifacts/                  tracked models, preprocessing, registry, reports
  data/                       Telco churn source CSV
  notebooks/                  EDA and logistic-regression reference notebooks
  training/                   reproducible training/evaluation script
  Dockerfile
  requirements.txt
scripts/
  restore-db.sh
docker-compose.yml
```

The model files in `ML/artifacts/` are intentionally tracked so a fresh clone can start inference without retraining. `ML/training/train_and_evaluate.py` can regenerate the model registry, reports, and split datasets.

## Model comparison

The following values are from the held-out test set (1,409 rows). Thresholds were selected using training-only out-of-fold predictions with business costs `FN=5`, `FP=1`; the test set was not used for threshold selection.

| Model | Default F1 | Default Recall | ROC-AUC | PR-AUC | Cost threshold | Tuned F1 | Tuned Recall | Tuned Precision |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| KNN | 0.5792 | 0.5668 | 0.8256 | 0.6045 | 0.1905 | 0.5774 | 0.9171 | 0.4214 |
| Gaussian Naive Bayes | 0.6102 | 0.7513 | 0.8235 | 0.5931 | 0.0027 | 0.5726 | 0.9171 | 0.4163 |
| Decision Tree | 0.6188 | 0.7941 | 0.8355 | 0.6357 | 0.3453 | 0.5877 | 0.8824 | 0.4406 |
| Random Forest | 0.6331 | 0.7727 | **0.8471** | **0.6570** | 0.3734 | **0.6135** | 0.8636 | **0.4757** |

Baselines at the default 0.50 threshold:

| Baseline | F1 | Recall | ROC-AUC | PR-AUC | Accuracy |
|---|---:|---:|---:|---:|---:|
| Dummy (most frequent) | 0.0000 | 0.0000 | 0.5000 | 0.2654 | 0.7346 |
| Logistic Regression (balanced) | 0.6164 | 0.7861 | 0.8419 | 0.6341 | 0.7402 |

The current registry selects Random Forest based on training OOF business cost. Full default/tuned confusion counts and all metrics are in `ML/artifacts/evaluation_comparison.csv`; hyperparameters and thresholds are in `ML/artifacts/model_registry.json`.

## Run with Docker Compose

Requirements: Docker Engine and Compose v2.

1. Configure a local SQL Server password:

   ```bash
   cp .env.example .env
   ```

   Set `MSSQL_SA_PASSWORD` in `.env` to a strong development password. Do not commit `.env`.

2. Build and start the full stack:

   ```bash
   docker compose up --build -d
   ```

   Compose starts SQL Server and Redis, restores `DB/ChurnPredictionDB.bak` through the one-shot `db-init` service, starts the ML service, then starts the API after its dependencies report healthy.

3. Check services and logs:

   ```bash
   docker compose ps
   docker compose logs -f ml-service api db-init
   ```

Service URLs:

| Service | Host address |
|---|---|
| ASP.NET Core API | `http://localhost:8080` |
| API Swagger (Development) | `http://localhost:8080/swagger` |
| API health | `http://localhost:8080/health` |
| FastAPI ML service | `http://localhost:8000` |
| ML health | `http://localhost:8000/health` |
| ML model registry and evaluation | `http://localhost:8000/models` |
| SQL Server | `127.0.0.1:1433` |
| Redis | Compose network only, `redis:6379` |

The API connects to the ML service at `http://ml-service:8000/` over the Compose network. Set `ML_SERVICE_BASE_URL` only when intentionally overriding that address. The SQL port is bound to loopback; the ML and API ports are published on the host.

The SQL data volume persists across normal container replacement. Avoid `docker compose down -v` unless you intentionally want to delete that database volume. The API does not automatically apply EF Core migrations at startup; its database must match the checked-in schema/backup.

## Train models locally

Use Python with the versions in `ML/requirements.txt`:

```bash
python -m pip install -r ML/requirements.txt
python ML/training/train_and_evaluate.py
```

The script loads `ML/data/WA_Fn-UseC_-Telco-Customer-Churn.csv`, creates the stratified train/test split, fits the scaler only on training rows, runs 5-fold stratified model searches, tunes thresholds on training OOF probabilities, and writes model/split artifacts under `ML/artifacts/`. It also verifies SHAP explanations for the selected model, Decision Tree, and Random Forest.

To run the ML service outside Docker:

```bash
cd ML
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

## API endpoints

Most ASP.NET application endpoints require a bearer token. The Identity endpoints are mapped by ASP.NET Core Identity (`POST /register`, `POST /login`); Swagger supports bearer authentication.

| Service | Method and route | Description |
|---|---|---|
| API | `GET /health` | API health |
| API | `GET/POST /api/customers`, `GET/PUT/DELETE /api/customers/{id}` | Customer management |
| API | `POST /api/imports/customers/csv` | Import CSV using multipart field `file` |
| API | `GET /api/predictions`, `GET /api/predictions/{id}` | Read saved predictions |
| API | `POST /api/predictions/{customerId}` | Predict and persist one customer's result |
| API | `GET/POST /api/campaigns`, `GET/PUT/DELETE /api/campaigns/{id}` | Campaign management |
| API | `/api/campaigns/{id}/customers` | Campaign audience and outcomes |
| API | `GET /api/dashboard/summary` | Dashboard summary |
| ML | `GET /health` | Active model, version, selected threshold |
| ML | `GET /models` | Full registry and evaluation rows |
| ML | `POST /predict?model_name=Random%20Forest` | Predict one raw customer row |
| ML | `POST /predict/batch?model_name=Random%20Forest` | Score a JSON array of customer rows |

`model_name` is optional and defaults to the registry's best model. Supported names are the registry keys: `Dummy (most frequent)`, `Logistic Regression`, `KNN`, `Naive Bayes`, `Decision Tree`, and `Random Forest`.

The single-row ML request is a flat JSON object matching the 19 customer feature fields sent by `MLService.cs`. Example:

```json
{
  "gender": "Female",
  "SeniorCitizen": 0,
  "Partner": "Yes",
  "Dependents": "No",
  "tenure": 12,
  "PhoneService": "Yes",
  "MultipleLines": "No",
  "InternetService": "Fiber optic",
  "OnlineSecurity": "No",
  "OnlineBackup": "Yes",
  "DeviceProtection": "No",
  "TechSupport": "No",
  "StreamingTV": "Yes",
  "StreamingMovies": "No",
  "Contract": "Month-to-month",
  "PaperlessBilling": "Yes",
  "PaymentMethod": "Electronic check",
  "MonthlyCharges": 75.5,
  "TotalCharges": 906.0
}
```

Prediction response:

```json
{
  "churnProbability": 0.6821,
  "riskLevel": "Medium",
  "modelVersion": "1.0.0",
  "topFactors": [
    {
      "feature": "Contract_Two year",
      "impact": "negative (-0.159327)"
    }
  ]
}
```

`impact` is a string to match the ASP.NET `MLFactor.Impact` contract; it contains the SHAP direction and signed contribution. Probabilities are rounded to four decimal places. Risk bands match the API: High at `>=0.7`, Medium at `>=0.4`, otherwise Low. Batch requests accept a JSON array of the same flat customer objects and return `{"predictions": [...]}`.
