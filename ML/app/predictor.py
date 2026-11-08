from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import joblib
import numpy as np
import pandas as pd
import shap
from sklearn.ensemble import RandomForestClassifier
from sklearn.tree import DecisionTreeClassifier

from training.train_and_evaluate import prepare_model_features


class ChurnPredictor:
	def __init__(self, artifact_dir: Path) -> None:
		self.artifact_dir = artifact_dir
		self.preprocessing = joblib.load(artifact_dir / "preprocessing.joblib")
		self.registry = json.loads(
			(artifact_dir / "model_registry.json").read_text(encoding="utf-8")
		)
		self.comparison = pd.read_csv(artifact_dir / "evaluation_comparison.csv")
		train_features = joblib.load(artifact_dir / "X_train.joblib")
		self.background = shap.sample(
			train_features,
			min(50, len(train_features)),
			random_state=42,
		)

		model_registry = self.registry.get("models")
		if not isinstance(model_registry, dict) or not model_registry:
			raise ValueError("The model registry does not contain any models.")

		self.models: dict[str, dict[str, Any]] = {}
		for name, metadata in model_registry.items():
			model_path = artifact_dir / metadata["model_file"]
			bundle = joblib.load(model_path)
			model = bundle["model"]
			if bundle.get("feature_names") != self.preprocessing["feature_names"]:
				raise ValueError(f"Feature schema mismatch in model artifact: {name}")
			self.models[name] = {
				"model": model,
				"threshold": float(metadata["chosen_threshold"]),
			}

		self.active_model_name = self.registry["threshold_selection"][
			"best_model_by_training_oof_cost"
		]
		if self.active_model_name not in self.models:
			raise ValueError("The registry's selected model is not present.")
		self.model_version = str(self.registry["model_version"])

	def available_models(self) -> list[str]:
		return list(self.models)

	def predict(self, features: dict[str, object], model_name: str | None = None) -> dict[str, object]:
		selected_name = model_name or self.active_model_name
		if selected_name not in self.models:
			available = ", ".join(self.available_models())
			raise ValueError(
				f"Unknown model '{selected_name}'. Available models: {available}"
			)

		model_bundle = self.models[selected_name]
		estimator = model_bundle["model"]
		model_features = prepare_model_features(features, self.preprocessing)
		probability = float(estimator.predict_proba(model_features)[0, 1])
		probability = round(probability, 4)
		risk_level = (
			"High"
			if probability >= 0.7
			else "Medium"
			if probability >= 0.4
			else "Low"
		)
		return {
			"churn_probability": probability,
			"risk_level": risk_level,
			"model_version": self.model_version,
			"top_factors": self._explain(estimator, model_features),
		}

	def models_summary(self) -> dict[str, object]:
		return {
			"registry": self.registry,
			"evaluation_comparison": self.comparison.to_dict(orient="records"),
		}

	def health(self) -> dict[str, object]:
		active = self.models[self.active_model_name]
		return {
			"status": "healthy",
			"activeModel": self.active_model_name,
			"modelVersion": self.model_version,
			"threshold": active["threshold"],
		}

	def _explain(
		self,
		estimator: Any,
		model_features: pd.DataFrame,
	) -> list[dict[str, str]]:
		if isinstance(estimator, (RandomForestClassifier, DecisionTreeClassifier)):
			explanation = shap.TreeExplainer(estimator)(model_features)
		else:
			explainer = shap.Explainer(
				estimator.predict_proba,
				self.background,
				algorithm="permutation",
			)
			explanation = explainer(
				model_features,
				max_evals=2 * len(self.preprocessing["feature_names"]) + 1,
			)

		values = np.asarray(explanation.values)
		class_index = list(estimator.classes_).index(1)
		if values.ndim == 3:
			if values.shape[0] == 1 and values.shape[-1] > class_index:
				values = values[0, :, class_index]
			elif values.shape[1] == 1 and values.shape[0] > class_index:
				values = values[class_index, 0, :]
			else:
				raise ValueError(f"Unexpected SHAP output shape: {values.shape}")
		elif values.ndim == 2:
			values = values[0]
		else:
			raise ValueError(f"Unexpected SHAP output shape: {values.shape}")

		feature_names = self.preprocessing["feature_names"]
		ranked = sorted(
			zip(feature_names, values, strict=True),
			key=lambda item: abs(float(item[1])),
			reverse=True,
		)
		return [
			{
				"feature": feature,
				"impact": (
					f"{'positive' if value >= 0 else 'negative'} "
					f"({float(value):+.6f})"
				),
			}
			for feature, value in ranked[:5]
		]
