using Churn.Application.DTOs.Dashboard;
using Churn.Application.Interfaces;
using Churn.Domain.Enums;
using Microsoft.Extensions.Caching.Hybrid;

namespace Churn.Application.Services;

public sealed class DashboardService(
    IUnitOfWork unitOfWork,
    HybridCache cache)
{
    private const string DashboardCacheKey = "dashboard:summary";

    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync(
            DashboardCacheKey,
            async token =>
            {
                var customers =
                    await unitOfWork.Customers.GetAllAsync(token);

                var predictions =
                    await unitOfWork.Predictions.GetAllAsync(token);

                var campaigns =
                    await unitOfWork.Campaigns.GetAllAsync(token);

                var labeled = customers
                    .Where(x => x.ActualChurn.HasValue)
                    .ToList();

                var churned = labeled.Count(
                    x => x.ActualChurn == true);

                return new DashboardSummaryResponse(
                    customers.Count,
                    churned,
                    labeled.Count == 0
                        ? 0
                        : Math.Round(
                            (decimal)churned * 100 / labeled.Count, 2),
                    predictions.Count(
                        x => x.RiskLevel == RiskLevel.High),
                    customers.Count == 0
                        ? 0
                        : Math.Round(
                            customers.Average(x => x.MonthlyCharges), 2),
                    campaigns.Count(
                        x => x.Status == CampaignStatus.Active),
                    predictions.Count);
            },
            options: CacheOptions,
            cancellationToken: ct);
    }
}