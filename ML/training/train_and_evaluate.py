from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Mapping

import joblib
import numpy as np
import pandas as pd
from sklearn.base import ClassifierMixin, clone
from sklearn.dummy import DummyClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import (
	accuracy_score,
	average_precision_score,
	confusion_matrix,
	f1_score,
	precision_score,
	recall_score,
	roc_auc_score,
)
from sklearn.model_selection import (
	GridSearchCV,
	StratifiedKFold,
	cross_val_predict,
	train_test_split,
)
from sklearn.naive_bayes import GaussianNB
from sklearn.neighbors import KNeighborsClassifier
from sklearn.preprocessing import StandardScaler
from sklearn.tree import DecisionTreeClassifier


ROOT = Path(__file__).resolve().parents[1]
DATA_PATH = ROOT / "data" / "WA_Fn-UseC_-Telco-Customer-Churn.csv"
ARTIFACTS_DIR = ROOT / "artifacts"
MODEL_VERSION = "1.0.0"
RANDOM_STATE = 42
TEST_SIZE = 0.2
FALSE_NEGATIVE_COST = 5
FALSE_POSITIVE_COST = 1

NUMERIC_COLS = ["tenure", "MonthlyCharges", "TotalCharges"]
RAW_FEATURE_COLS = [
	"gender",
	"SeniorCitizen",
	"Partner",
	"Dependents",
	"tenure",
	"PhoneService",
	"MultipleLines",
	"InternetService",
	"OnlineSecurity",
	"OnlineBackup",
	"DeviceProtection",
	"TechSupport",
	"StreamingTV",
	"StreamingMovies",
	"Contract",
	"PaperlessBilling",
	"PaymentMethod",
	"MonthlyCharges",
	"TotalCharges",
]
FEATURE_NAMES = [
	"SeniorCitizen",
	"tenure",
	"MonthlyCharges",
	"TotalCharges",
	"gender_Male",
	"Partner_Yes",
	"Dependents_Yes",
	"PhoneService_Yes",
	"MultipleLines_Yes",
	"InternetService_Fiber optic",
	"InternetService_No",
	"OnlineSecurity_Yes",
	"OnlineBackup_Yes",
	"DeviceProtection_Yes",
	"TechSupport_Yes",
	"StreamingTV_Yes",
	"StreamingMovies_Yes",
	"Contract_One year",
	"Contract_Two year",
	"PaperlessBilling_Yes",
	"PaymentMethod_Credit card (automatic)",
	"PaymentMethod_Electronic check",
	"PaymentMethod_Mailed check",
]
SERVICE_COLS = [
	"OnlineSecurity",
	"OnlineBackup",
	"DeviceProtection",
	"TechSupport",
	"StreamingTV",
	"StreamingMovies",
]
CATEGORY_VALUES = {
	"gender": ["Female", "Male"],
	"Partner": ["No", "Yes"],
	"Dependents": ["No", "Yes"],
	"PhoneService": ["No", "Yes"],
	"MultipleLines": ["No", "Yes"],
	"InternetService": ["DSL", "Fiber optic", "No"],
	"OnlineSecurity": ["No", "Yes"],
	"OnlineBackup": ["No", "Yes"],
	"DeviceProtection": ["No", "Yes"],
	"TechSupport": ["No", "Yes"],
	"StreamingTV": ["No", "Yes"],
	"StreamingMovies": ["No", "Yes"],
	"Contract": ["Month-to-month", "One year", "Two year"],
	"PaperlessBilling": ["No", "Yes"],
	"PaymentMethod": [
		"Bank transfer (automatic)",
		"Credit card (automatic)",
		"Electronic check",
		"Mailed check",
	],
}
METRIC_COLUMNS = [
	"model",
	"threshold_policy",
	"recall",
	"precision",
	"f1_score",
	"roc_auc",
	"pr_auc",
	"accuracy",
	"chosen_threshold",
	"tp",
	"fp",
	"tn",
	"fn",
]


def normalize_frame(
	data: Mapping[str, object] | pd.DataFrame,
	category_values: Mapping[str, list[str]] = CATEGORY_VALUES,
) -> pd.DataFrame:
	"""Encode raw customer rows into the fixed, ordered 23-feature contract."""
	frame = data.copy() if isinstance(data, pd.DataFrame) else pd.DataFrame([dict(data)])
	frame = frame.drop(columns=["customerID", "Churn"], errors="ignore")

	missing_columns = sorted(set(RAW_FEATURE_COLS) - set(frame.columns))
	if missing_columns:
		raise ValueError(f"Missing required customer columns: {missing_columns}")

	frame = frame.loc[:, RAW_FEATURE_COLS].copy()
	frame["TotalCharges"] = pd.to_numeric(
		frame["TotalCharges"], errors="coerce"
	).fillna(0)
	for column in ("SeniorCitizen", *NUMERIC_COLS):
		frame[column] = pd.to_numeric(frame[column], errors="raise")

	for column in SERVICE_COLS:
		frame[column] = frame[column].replace("No internet service", "No")
	frame["MultipleLines"] = frame["MultipleLines"].replace(
		"No phone service", "No"
	)
	for column, allowed_values in category_values.items():
		unsupported_values = set(frame[column].dropna()) - set(allowed_values)
		if frame[column].isna().any() or unsupported_values:
			raise ValueError(
				f"Unsupported or missing values in {column}: "
				f"{sorted(map(str, unsupported_values))}"
			)
		frame[column] = pd.Categorical(frame[column], categories=allowed_values)

	encoded = pd.get_dummies(frame, drop_first=True, dtype=np.int8)
	encoded = encoded.reindex(columns=FEATURE_NAMES, fill_value=0)
	for column in ("SeniorCitizen", *NUMERIC_COLS):
		encoded[column] = encoded[column].astype(np.float64)
	return encoded.loc[:, FEATURE_NAMES]


def prepare_model_features(
	data: Mapping[str, object] | pd.DataFrame,
	preprocessing_bundle: Mapping[str, object],
) -> pd.DataFrame:
	"""Apply the saved feature schema and training-fitted scaler to raw rows."""
	feature_names = preprocessing_bundle["feature_names"]
	numeric_cols = preprocessing_bundle["numeric_cols"]
	scaler = preprocessing_bundle["scaler"]
	category_values = preprocessing_bundle["category_values"]
	if not isinstance(feature_names, list) or not isinstance(numeric_cols, list):
		raise TypeError("The preprocessing bundle contains invalid column metadata.")
	if not isinstance(category_values, dict):
		raise TypeError("The preprocessing bundle contains invalid category metadata.")
	if not isinstance(scaler, StandardScaler):
		raise TypeError("The preprocessing bundle does not contain a StandardScaler.")

	features = normalize_frame(
		data,
		category_values=category_values,
	).reindex(columns=feature_names, fill_value=0)
	features.loc[:, numeric_cols] = scaler.transform(
		features[numeric_cols].astype(np.float64)
	)
	return features


def load_preprocessing_bundle(
	path: Path | str = ARTIFACTS_DIR / "preprocessing.joblib",
) -> dict[str, object]:
	"""Load the fitted preprocessing metadata and scaler for inference."""
	bundle = joblib.load(path)
	if not isinstance(bundle, dict):
		raise TypeError("The preprocessing artifact must contain a dictionary.")
	return bundle


def encode_target(target: pd.Series) -> pd.Series:
	"""Map the source dataset's churn labels to integer class labels."""
	if target.dtype == "object" or isinstance(target.dtype, pd.StringDtype):
		encoded = target.map({"No": 0, "Yes": 1})
	else:
		encoded = pd.to_numeric(target, errors="raise")
	if encoded.isna().any():
		raise ValueError("Churn contains missing or unsupported labels.")
	return encoded.astype(np.int8)


def classification_metrics(
	y_true: pd.Series,
	probabilities: np.ndarray,
	threshold: float,
) -> dict[str, float | int]:
	predictions = (probabilities >= threshold).astype(np.int8)
	tn, fp, fn, tp = confusion_matrix(y_true, predictions, labels=[0, 1]).ravel()
	return {
		"recall": float(recall_score(y_true, predictions, zero_division=0)),
		"precision": float(precision_score(y_true, predictions, zero_division=0)),
		"f1_score": float(f1_score(y_true, predictions, zero_division=0)),
		"roc_auc": float(roc_auc_score(y_true, probabilities)),
		"pr_auc": float(average_precision_score(y_true, probabilities)),
		"accuracy": float(accuracy_score(y_true, predictions)),
		"chosen_threshold": float(threshold),
		"tp": int(tp),
		"fp": int(fp),
		"tn": int(tn),
		"fn": int(fn),
	}


def select_cost_threshold(
	y_true: pd.Series,
	probabilities: np.ndarray,
) -> tuple[float, float]:
	"""Minimize the declared FN/FP business cost using training OOF scores only."""
	candidates = np.unique(np.concatenate(([0.0, 0.5, 1.0], probabilities)))
	best_threshold = 0.5
	best_cost = float("inf")
	best_f1 = -1.0
	for threshold in candidates:
		predictions = (probabilities >= threshold).astype(np.int8)
		tn, fp, fn, tp = confusion_matrix(y_true, predictions, labels=[0, 1]).ravel()
		cost = float(FALSE_NEGATIVE_COST * fn + FALSE_POSITIVE_COST * fp)
		f1 = float(f1_score(y_true, predictions, zero_division=0))
		if cost < best_cost or (cost == best_cost and f1 > best_f1):
			best_threshold = float(threshold)
			best_cost = cost
			best_f1 = f1
	return best_threshold, best_cost


def build_searches(cv: StratifiedKFold) -> dict[str, GridSearchCV]:
	return {
		"KNN": GridSearchCV(
			KNeighborsClassifier(),
			{
				"n_neighbors": [5, 11, 15, 21],
				"weights": ["uniform", "distance"],
			},
			scoring="f1",
			cv=cv,
			n_jobs=-1,
			pre_dispatch="2*n_jobs",
			refit=True,
		),
		"Naive Bayes": GridSearchCV(
			GaussianNB(),
			{"var_smoothing": np.logspace(-12, -6, 7)},
			scoring="f1",
			cv=cv,
			n_jobs=-1,
			pre_dispatch="2*n_jobs",
			refit=True,
		),
		"Decision Tree": GridSearchCV(
			DecisionTreeClassifier(random_state=RANDOM_STATE),
			{
				"max_depth": [4, 6, 8, 10],
				"min_samples_leaf": [10, 20, 50],
				"class_weight": [None, "balanced"],
			},
			scoring="f1",
			cv=cv,
			n_jobs=-1,
			pre_dispatch="2*n_jobs",
			refit=True,
		),
		"Random Forest": GridSearchCV(
			RandomForestClassifier(random_state=RANDOM_STATE, n_jobs=1),
			{
				"n_estimators": [100, 200],
				"max_depth": [6, 8, 10, None],
				"min_samples_leaf": [5, 10, 20],
				"class_weight": ["balanced", "balanced_subsample"],
			},
			scoring="f1",
			cv=cv,
			n_jobs=-1,
			pre_dispatch="2*n_jobs",
			refit=True,
		),
	}


def evaluate_shap(
	models: Mapping[str, ClassifierMixin],
	X_train: pd.DataFrame,
	customer: pd.DataFrame,
	best_model_name: str,
) -> dict[str, list[dict[str, str | float]]]:
	"""Explain one customer for the best model and verify both tree models."""
	import shap

	names_to_explain = list(dict.fromkeys([best_model_name, "Random Forest", "Decision Tree"]))
	background = shap.sample(X_train, min(50, len(X_train)), random_state=RANDOM_STATE)
	results: dict[str, list[dict[str, str | float]]] = {}

	for name in names_to_explain:
		estimator = models[name]
		if isinstance(estimator, (RandomForestClassifier, DecisionTreeClassifier)):
			explainer = shap.TreeExplainer(estimator)
			explanation = explainer(customer)
		else:
			explainer = shap.Explainer(
				estimator.predict_proba,
				background,
				algorithm="permutation",
			)
			explanation = explainer(
				customer,
				max_evals=2 * len(FEATURE_NAMES) + 1,
			)

		values = np.asarray(explanation.values)
		if values.ndim == 3:
			class_index = list(estimator.classes_).index(1)
			if values.shape[0] == 1 and values.shape[-1] > class_index:
				values = values[0, :, class_index]
			elif values.shape[1] == 1 and values.shape[0] > class_index:
				values = values[class_index, 0, :]
			else:
				raise ValueError(
					f"Unexpected SHAP output shape for {name}: {values.shape}"
				)
		elif values.ndim == 2:
			values = values[0]
		else:
			raise ValueError(f"Unexpected SHAP output shape for {name}: {values.shape}")

		ranked = sorted(
			zip(FEATURE_NAMES, values, strict=True),
			key=lambda item: abs(float(item[1])),
			reverse=True,
		)
		results[name] = [
			{
				"feature": feature,
				"shap_value": float(value),
				"direction": "positive" if value > 0 else "negative",
			}
			for feature, value in ranked[:10]
		]

	return results


def main() -> None:
	if not DATA_PATH.is_file():
		raise FileNotFoundError(f"Required dataset not found: {DATA_PATH}")
	ARTIFACTS_DIR.mkdir(parents=True, exist_ok=True)

	raw = pd.read_csv(DATA_PATH)
	if "Churn" not in raw:
		raise ValueError("The dataset must contain a Churn target column.")
	y = encode_target(raw["Churn"])
	X = normalize_frame(raw)
	if X.columns.tolist() != FEATURE_NAMES:
		raise RuntimeError("Preprocessing did not produce the contracted feature order.")

	X_train, X_test, y_train, y_test = train_test_split(
		X,
		y,
		test_size=TEST_SIZE,
		random_state=RANDOM_STATE,
		stratify=y,
	)
	scaler = StandardScaler()
	X_train = X_train.copy()
	X_test = X_test.copy()
	X_train.loc[:, NUMERIC_COLS] = scaler.fit_transform(
		X_train[NUMERIC_COLS].astype(np.float64)
	)
	X_test.loc[:, NUMERIC_COLS] = scaler.transform(
		X_test[NUMERIC_COLS].astype(np.float64)
	)

	preprocessing_bundle: dict[str, object] = {
		"scaler": scaler,
		"feature_names": FEATURE_NAMES.copy(),
		"numeric_cols": NUMERIC_COLS.copy(),
		"category_values": CATEGORY_VALUES,
		"model_version": MODEL_VERSION,
	}
	joblib.dump(preprocessing_bundle, ARTIFACTS_DIR / "preprocessing.joblib")
	for name, value in (
		("X_train", X_train),
		("X_test", X_test),
		("y_train", y_train),
		("y_test", y_test),
	):
		joblib.dump(value, ARTIFACTS_DIR / f"{name}.joblib")

	cv = StratifiedKFold(n_splits=5, shuffle=True, random_state=RANDOM_STATE)
	models: dict[str, ClassifierMixin] = {
		"Dummy (most frequent)": DummyClassifier(strategy="most_frequent"),
		"Logistic Regression": LogisticRegression(
			class_weight="balanced",
			random_state=RANDOM_STATE,
			max_iter=1000,
		),
	}
	best_params: dict[str, dict[str, object]] = {
		"Dummy (most frequent)": {"strategy": "most_frequent"},
		"Logistic Regression": {
			"class_weight": "balanced",
			"random_state": RANDOM_STATE,
			"max_iter": 1000,
		},
	}
	searches = build_searches(cv)
	for name, search in searches.items():
		print(f"Cross-validating {name}...")
		search.fit(X_train, y_train)
		models[name] = search.best_estimator_
		best_params[name] = dict(search.best_params_)
		print(f"  Best parameters: {search.best_params_}")

	for name in ("Dummy (most frequent)", "Logistic Regression"):
		models[name].fit(X_train, y_train)

	oof_probabilities: dict[str, np.ndarray] = {}
	thresholds: dict[str, float] = {}
	validation_costs: dict[str, float] = {}
	for name, estimator in models.items():
		print(f"Generating training-only OOF probabilities for {name}...")
		probabilities = cross_val_predict(
			clone(estimator),
			X_train,
			y_train,
			cv=cv,
			method="predict_proba",
			n_jobs=-1,
		)[:, 1]
		oof_probabilities[name] = probabilities
		threshold, cost = select_cost_threshold(y_train, probabilities)
		thresholds[name] = threshold
		validation_costs[name] = cost

	validation_f1 = {
		name: float(
			f1_score(
				y_train,
				(probabilities >= thresholds[name]).astype(np.int8),
				zero_division=0,
			)
		)
		for name, probabilities in oof_probabilities.items()
	}
	best_model_name = min(
		models,
		key=lambda name: (validation_costs[name], -validation_f1[name]),
	)

	rows: list[dict[str, str | float | int]] = []
	registry_models: dict[str, dict[str, object]] = {}
	for name, estimator in models.items():
		test_probabilities = estimator.predict_proba(X_test)[:, 1]
		default_metrics = classification_metrics(y_test, test_probabilities, 0.5)
		tuned_metrics = classification_metrics(
			y_test,
			test_probabilities,
			thresholds[name],
		)
		rows.extend(
			[
				{"model": name, "threshold_policy": "default_0.50", **default_metrics},
				{"model": name, "threshold_policy": "cost_tuned", **tuned_metrics},
			]
		)
		model_filename = f"{name.lower().replace(' ', '_').replace('(', '').replace(')', '')}.joblib"
		joblib.dump(
			{
				"model": estimator,
				"threshold": thresholds[name],
				"feature_names": FEATURE_NAMES.copy(),
				"numeric_cols": NUMERIC_COLS.copy(),
				"model_version": MODEL_VERSION,
			},
			ARTIFACTS_DIR / model_filename,
		)
		registry_models[name] = {
			"model_file": model_filename,
			"hyperparameters": best_params[name],
			"chosen_threshold": thresholds[name],
			"training_oof_cost": validation_costs[name],
			"training_oof_f1_at_chosen_threshold": validation_f1[name],
			"test_metrics": {
				"default_0.50": default_metrics,
				"cost_tuned": tuned_metrics,
			},
		}

	comparison = pd.DataFrame(rows, columns=METRIC_COLUMNS)
	comparison.to_csv(ARTIFACTS_DIR / "evaluation_comparison.csv", index=False)
	print("\nTest-set comparison (threshold selection used training OOF only):")
	print(comparison.to_string(index=False, float_format=lambda value: f"{value:.4f}"))

	explanation = evaluate_shap(
		models,
		X_train,
		X_test.iloc[[0]],
		best_model_name,
	)
	(ARTIFACTS_DIR / "shap_explanations.json").write_text(
		json.dumps(
			{
				"customer_index": str(X_test.index[0]),
				"explained_class": 1,
				"best_model": best_model_name,
				"top_features_by_absolute_contribution": explanation,
			},
			indent=2,
		),
		encoding="utf-8",
	)
	print(f"\nBest model by training OOF business cost: {best_model_name}")
	print("Single-customer SHAP contributions for the best model:")
	for contribution in explanation[best_model_name][:10]:
		print(
			f"  {contribution['direction']:>8} "
			f"{contribution['feature']}: {contribution['shap_value']:.6f}"
		)
	print("SHAP verification completed for the best model, Random Forest, and Decision Tree.")

	registry = {
		"model_version": MODEL_VERSION,
		"training_date_utc": datetime.now(timezone.utc).isoformat(),
		"dataset": DATA_PATH.name,
		"split": {
			"test_size": TEST_SIZE,
			"random_state": RANDOM_STATE,
			"stratified": True,
			"train_rows": len(X_train),
			"test_rows": len(X_test),
		},
		"feature_names": FEATURE_NAMES,
		"numeric_cols": NUMERIC_COLS,
		"threshold_selection": {
			"method": "minimum out-of-fold training cost; ties broken by F1",
			"cross_validation": "5-fold StratifiedKFold, shuffle=True, random_state=42",
			"false_negative_cost": FALSE_NEGATIVE_COST,
			"false_positive_cost": FALSE_POSITIVE_COST,
			"best_model_by_training_oof_cost": best_model_name,
		},
		"models": registry_models,
	}
	(ARTIFACTS_DIR / "model_registry.json").write_text(
		json.dumps(registry, indent=2, default=str),
		encoding="utf-8",
	)
	print(f"\nArtifacts saved to: {ARTIFACTS_DIR}")


if __name__ == "__main__":
	main()
