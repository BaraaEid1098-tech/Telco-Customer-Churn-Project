namespace Churn.Application.DTOs.Dashboard;

public sealed record DashboardSummaryResponse(int TotalCustomers, int KnownChurnedCustomers,
    decimal ChurnRatePercent, int HighRiskCustomers, decimal AverageMonthlyCharges,
    int ActiveCampaigns, int TotalPredictions);
