namespace Churn.Application.DTOs.Campaigns;

public sealed record CreateCampaignRequest(string Name, string? Description, string CampaignType,
    DateTime StartDate, DateTime EndDate, decimal? Budget);
public sealed record UpdateCampaignRequest(string Name, string? Description, string CampaignType,
    DateTime StartDate, DateTime EndDate, decimal? Budget, string Status);
public sealed record CampaignResponse(int Id, string Name, string? Description, string CampaignType,
    DateTime StartDate, DateTime EndDate, decimal? Budget, string Status, DateTime CreatedAt);
