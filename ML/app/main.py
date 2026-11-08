from __future__ import annotations

from contextlib import asynccontextmanager
from pathlib import Path
from typing import Annotated

from fastapi import FastAPI, HTTPException, Query, Request
from starlette.concurrency import run_in_threadpool

from app.predictor import ChurnPredictor
from app.schemas import (
	BatchPredictionResponse,
	CustomerFeatures,
	PredictionResponse,
)


ARTIFACT_DIR = Path(__file__).resolve().parents[1] / "artifacts"


@asynccontextmanager
async def lifespan(application: FastAPI):
	application.state.predictor = ChurnPredictor(ARTIFACT_DIR)
	yield


app = FastAPI(
	title="Telco Churn ML Service",
	version="1.0.0",
	description="Churn prediction and SHAP explanations for the Telco customer API.",
	lifespan=lifespan,
)


def get_predictor(request: Request) -> ChurnPredictor:
	return request.app.state.predictor


@app.get("/health")
async def health(request: Request) -> dict[str, object]:
	return get_predictor(request).health()


@app.get("/models")
async def models(request: Request) -> dict[str, object]:
	return get_predictor(request).models_summary()


@app.post("/predict", response_model=PredictionResponse)
async def predict(
	customer: CustomerFeatures,
	request: Request,
	model_name: Annotated[str | None, Query()] = None,
) -> dict[str, object]:
	predictor = get_predictor(request)
	try:
		return await run_in_threadpool(
			predictor.predict,
			customer.model_dump(),
			model_name,
		)
	except ValueError as error:
		raise HTTPException(status_code=422, detail=str(error)) from error


@app.post("/predict/batch", response_model=BatchPredictionResponse)
async def predict_batch(
	customers: list[CustomerFeatures],
	request: Request,
	model_name: Annotated[str | None, Query()] = None,
) -> dict[str, list[dict[str, object]]]:
	predictor = get_predictor(request)
	results: list[dict[str, object]] = []
	for customer in customers:
		try:
			result = await run_in_threadpool(
				predictor.predict,
				customer.model_dump(),
				model_name,
			)
		except ValueError as error:
			raise HTTPException(status_code=422, detail=str(error)) from error
		results.append(result)
	return {"predictions": results}
