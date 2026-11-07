namespace Churn.Application.Interfaces;

public interface IMLService
{
    Task<MLPredictionResponse> PredictAsync(
        MLPredictionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class MLPredictionRequest
{
    public Dictionary<string, object?> Features { get; set; } = new();
}

public sealed class MLPredictionResponse
{
    public decimal ChurnProbability { get; set; }
    public string RiskLevel { get; set; } = null!;
    public string ModelVersion { get; set; } = null!;
    public List<MLFactor> TopFactors { get; set; } = new();
}

public sealed class MLFactor
{
    public string Feature { get; set; } = null!;
    public string Impact { get; set; } = null!;
}
