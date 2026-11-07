namespace Churn.Application.DTOs.Predictions;

public sealed record CreatePredictionRequest(int CustomerId);
public sealed record PredictionResponse(int Id, int CustomerId, decimal ChurnProbability, string RiskLevel, string ModelVersion, IReadOnlyList<PredictionFactorResponse> TopFactors, DateTime CreatedAt);
public sealed record PredictionFactorResponse(string Feature, string Impact);
