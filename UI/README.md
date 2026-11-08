# RetenScope Web UI

The UI is a dependency-free static HTML/CSS/JavaScript client for the ASP.NET Core API and its FastAPI-backed model endpoints.

## Run locally

```bash
python -m http.server 5500
```

Open `http://localhost:5500`. The default API address is `http://localhost:8080`; it can be changed on the sign-in screen or under Model & settings. Sign in with an analyst account or select demo mode to use clearly labeled illustrative data.

The browser stores the API address in local storage and keeps the bearer token in session storage. Use HTTPS for deployed API endpoints and do not use shared browser sessions for analyst accounts.

## Run with Docker Compose

From the monorepo root:

```bash
docker compose up --build -d ui api
```

The static site is served at `http://localhost:8081`. The Compose API is at `http://localhost:8080`; CORS allows the local UI origins and `https://miqnaas.github.io`.

The browser talks to the ASP.NET API. Authenticated API proxy endpoints `GET /api/ml/models` and public health endpoint `GET /api/ml/health` query FastAPI over the internal Compose network, so the ML service does not need browser CORS access.

## GitHub Pages

The original Pages workflow is retained under `.github/workflows/` for the standalone frontend repository. For deployment from this monorepo, a GitHub Actions workflow must be placed at the repository root `.github/workflows/` and upload `UI/` as the Pages artifact. Configure the API address in the UI after deploying; GitHub Pages cannot host the API, ML service, or database.

The configured backend CORS allowlist includes the Pages origin `https://miqnaas.github.io`. The API itself must be hosted on a publicly reachable HTTPS endpoint for the hosted UI to use live data.

## Integration behavior

- Authentication uses ASP.NET Identity `POST /login?useCookies=false` and `POST /register`; the bearer token is attached to protected `/api/*` requests.
- Customer, prediction, dashboard, CSV import, campaign, audience, and campaign outcome views use the corresponding ASP.NET API endpoints.
- Saved prediction `topFactors` are rendered as signed SHAP contribution bars. Demo mode is clearly labeled and never reports mock values as live predictions.
- Service health is queried through `/health` and `/api/ml/health`; the model registry and evaluation table are loaded from `/api/ml/models`.
