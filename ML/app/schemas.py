from __future__ import annotations

from typing import Annotated

from pydantic import BaseModel, ConfigDict, Field


NonNegativeFloat = Annotated[float, Field(ge=0)]
NonNegativeInt = Annotated[int, Field(ge=0)]
YesNo = Annotated[str, Field(pattern="^(Yes|No)$")]


class CustomerFeatures(BaseModel):
	model_config = ConfigDict(extra="forbid")

	gender: Annotated[str, Field(pattern="^(Female|Male)$")]
	SeniorCitizen: Annotated[int, Field(ge=0, le=1)]
	Partner: YesNo
	Dependents: YesNo
	tenure: NonNegativeInt
	PhoneService: YesNo
	MultipleLines: Annotated[
		str,
		Field(pattern="^(Yes|No|No phone service)$"),
	]
	InternetService: Annotated[str, Field(pattern="^(DSL|Fiber optic|No)$")]
	OnlineSecurity: Annotated[str, Field(pattern="^(Yes|No|No internet service)$")]
	OnlineBackup: Annotated[str, Field(pattern="^(Yes|No|No internet service)$")]
	DeviceProtection: Annotated[
		str,
		Field(pattern="^(Yes|No|No internet service)$"),
	]
	TechSupport: Annotated[str, Field(pattern="^(Yes|No|No internet service)$")]
	StreamingTV: Annotated[str, Field(pattern="^(Yes|No|No internet service)$")]
	StreamingMovies: Annotated[
		str,
		Field(pattern="^(Yes|No|No internet service)$"),
	]
	Contract: Annotated[
		str,
		Field(pattern="^(Month-to-month|One year|Two year)$"),
	]
	PaperlessBilling: YesNo
	PaymentMethod: Annotated[
		str,
		Field(
			pattern=(
				"^(Bank transfer \\(automatic\\)|Credit card \\(automatic\\)|"
				"Electronic check|Mailed check)$"
			)
		),
	]
	MonthlyCharges: NonNegativeFloat
	TotalCharges: NonNegativeFloat


class PredictionFactor(BaseModel):
	feature: str
	impact: str


class PredictionResponse(BaseModel):
	churn_probability: float = Field(serialization_alias="churnProbability")
	risk_level: str = Field(serialization_alias="riskLevel")
	model_version: str = Field(serialization_alias="modelVersion")
	top_factors: list[PredictionFactor] = Field(serialization_alias="topFactors")


class BatchPredictionResponse(BaseModel):
	predictions: list[PredictionResponse]
