namespace Churn.Application.DTOs.Campaigns;

public sealed record AddCampaignCustomerRequest(int CustomerId, int? PredictionId, string? OfferDetails);
public sealed record UpdateCampaignCustomerRequest(string ContactStatus, DateTime? ContactDate,
    string CampaignOutcome, string? Notes);
public sealed record CampaignCustomerResponse(int Id, int CampaignId, int CustomerId, int? PredictionId,
    string ContactStatus, DateTime? ContactDate, string? OfferDetails, string CampaignOutcome, string? Notes);
