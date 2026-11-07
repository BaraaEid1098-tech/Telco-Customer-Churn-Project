using System.Net.Http.Json;
using Churn.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Churn.Infrastructure.ML;

public sealed class MLService : IMLService
{
    private readonly HttpClient _httpClient;

    public MLService(HttpClient httpClient, IOptions<MLServiceOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
    }

    public async Task<MLPredictionResponse> PredictAsync(
        MLPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "predict", request.Features, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MLPredictionResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("ML service returned an empty response.");
    }
}
