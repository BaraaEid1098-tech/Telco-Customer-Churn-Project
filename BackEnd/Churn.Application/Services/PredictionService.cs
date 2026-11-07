using System.Text.Json;
using Churn.Application.DTOs.Predictions;
using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Domain.Enums;

namespace Churn.Application.Services;

public sealed class PredictionService(IUnitOfWork unitOfWork, IMLService mlService)
{
    public async Task<PredictionResponse?> PredictAsync(int customerId, CancellationToken ct = default)
    {
        var customer = await unitOfWork.Customers.GetByIdAsync(customerId, ct);
        if (customer is null) return null;
        // Only model features are sent. CustomerCode and ActualChurn are intentionally excluded.
        var features = new Dictionary<string, object?>
        {
            ["gender"] = customer.Gender, ["SeniorCitizen"] = customer.SeniorCitizen ? 1 : 0,
            ["Partner"] = YesNo(customer.Partner), ["Dependents"] = YesNo(customer.Dependents),
            ["tenure"] = customer.Tenure, ["PhoneService"] = YesNo(customer.PhoneService),
            ["MultipleLines"] = customer.MultipleLines, ["InternetService"] = customer.InternetService,
            ["OnlineSecurity"] = customer.OnlineSecurity, ["OnlineBackup"] = customer.OnlineBackup,
            ["DeviceProtection"] = customer.DeviceProtection, ["TechSupport"] = customer.TechSupport,
            ["StreamingTV"] = customer.StreamingTV, ["StreamingMovies"] = customer.StreamingMovies,
            ["Contract"] = customer.Contract, ["PaperlessBilling"] = YesNo(customer.PaperlessBilling),
            ["PaymentMethod"] = customer.PaymentMethod, ["MonthlyCharges"] = customer.MonthlyCharges,
            ["TotalCharges"] = customer.TotalCharges ?? 0m
        };
        var result = await mlService.PredictAsync(new MLPredictionRequest { Features = features }, ct);
        if (result.ChurnProbability is < 0 or > 1) throw new InvalidOperationException("ML service returned a probability outside 0..1.");
        var risk = result.ChurnProbability >= .7m ? RiskLevel.High : result.ChurnProbability >= .4m ? RiskLevel.Medium : RiskLevel.Low;
        var prediction = new Prediction { CustomerId = customerId, ChurnProbability = result.ChurnProbability,
            RiskLevel = risk, ModelVersion = string.IsNullOrWhiteSpace(result.ModelVersion) ? "unknown" : result.ModelVersion,
            TopFactors = JsonSerializer.Serialize(result.TopFactors ?? new()) };
        await unitOfWork.Predictions.AddAsync(prediction, ct); await unitOfWork.SaveChangesAsync(ct);
        return Map(prediction);
    }
    public async Task<IReadOnlyList<PredictionResponse>> GetAllAsync(CancellationToken ct = default) =>
        (await unitOfWork.Predictions.GetAllAsync(ct)).Select(Map).ToList();
    public async Task<PredictionResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prediction = await unitOfWork.Predictions.GetByIdAsync(id, ct);
        return prediction is null ? null : Map(prediction);
    }
    private static string YesNo(bool value) => value ? "Yes" : "No";
    private static PredictionResponse Map(Prediction p)
    {
        var factors = string.IsNullOrWhiteSpace(p.TopFactors) ? new List<PredictionFactorResponse>() :
            (JsonSerializer.Deserialize<List<MLFactor>>(p.TopFactors) ?? new()).Select(x => new PredictionFactorResponse(x.Feature, x.Impact)).ToList();
        return new(p.Id, p.CustomerId, p.ChurnProbability, p.RiskLevel.ToString(), p.ModelVersion, factors, p.CreatedAt);
    }
}
