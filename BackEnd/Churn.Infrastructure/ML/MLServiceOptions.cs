namespace Churn.Infrastructure.ML;

public sealed class MLServiceOptions
{
    public const string SectionName = "MLService";
    public string BaseUrl { get; set; } = "http://localhost:8000/";
    public int TimeoutSeconds { get; set; } = 30;
}
