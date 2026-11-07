using Churn.Application.DTOs.Campaigns;
using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Domain.Enums;
using Microsoft.Extensions.Caching.Hybrid;

namespace Churn.Application.Services;

public sealed class CampaignService(
    IUnitOfWork unitOfWork,
    HybridCache cache)
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    private const string AllCampaignsKey = "campaigns:all";
    private const string DashboardKey = "dashboard:summary";

    private static string CampaignKey(int id) =>
        $"campaigns:{id}";

    private static string AudienceKey(int id) =>
        $"campaigns:{id}:audience";

    // Get All Campaigns
    public async Task<IReadOnlyList<CampaignResponse>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync(
            AllCampaignsKey,
            async token =>
            {
                var campaigns =
                    await unitOfWork.Campaigns.GetAllAsync(token);

                return campaigns.Select(Map).ToList();
            },
            options: CacheOptions,
            cancellationToken: ct);
    }

    public async Task<CampaignResponse?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync<CampaignResponse?>(
            CampaignKey(id),
            async token =>
            {
                var campaign =
                    await unitOfWork.Campaigns.GetByIdAsync(id, token);

                return campaign is null ? null : Map(campaign);
            },
            options: CacheOptions,
            cancellationToken: ct);
    }

    public async Task<CampaignResponse> CreateAsync(
        CreateCampaignRequest request,
        CancellationToken ct = default)
    {
        Validate(
            request.Name,
            request.CampaignType,
            request.StartDate,
            request.EndDate,
            request.Budget);

        var campaign = new Campaign
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CampaignType = request.CampaignType.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            Status = CampaignStatus.Draft
        };

        await unitOfWork.Campaigns.AddAsync(campaign, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(AllCampaignsKey, ct);
        await cache.RemoveAsync(DashboardKey, ct);

        return Map(campaign);
    }

    public async Task<CampaignResponse?> UpdateAsync(
        int id,
        UpdateCampaignRequest request,
        CancellationToken ct = default)
    {
        var campaign =
            await unitOfWork.Campaigns.GetByIdAsync(id, ct);

        if (campaign is null)
            return null;

        Validate(
            request.Name,
            request.CampaignType,
            request.StartDate,
            request.EndDate,
            request.Budget);

        if (!Enum.TryParse<CampaignStatus>(
                request.Status, true, out var status))
        {
            throw new ArgumentException(
                "Status must be Draft, Active, Completed, or Cancelled.");
        }

        campaign.Name = request.Name.Trim();
        campaign.Description = request.Description?.Trim();
        campaign.CampaignType = request.CampaignType.Trim();
        campaign.StartDate = request.StartDate;
        campaign.EndDate = request.EndDate;
        campaign.Budget = request.Budget;
        campaign.Status = status;
        campaign.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Campaigns.Update(campaign);

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(CampaignKey(id), ct);
        await cache.RemoveAsync(AllCampaignsKey, ct);
        await cache.RemoveAsync(AudienceKey(id), ct);
        await cache.RemoveAsync(DashboardKey, ct);

        return Map(campaign);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        var campaign =
            await unitOfWork.Campaigns.GetByIdAsync(id, ct);

        if (campaign is null)
            return false;

        unitOfWork.Campaigns.Delete(campaign);

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(CampaignKey(id), ct);
        await cache.RemoveAsync(AllCampaignsKey, ct);
        await cache.RemoveAsync(AudienceKey(id), ct);
        await cache.RemoveAsync(DashboardKey, ct);

        return true;
    }

    public async Task<IReadOnlyList<CampaignCustomerResponse>?> GetAudienceAsync(
        int campaignId,
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync<List<CampaignCustomerResponse>?>(
            AudienceKey(campaignId),
            async token =>
            {
                var campaign =
                    await unitOfWork.Campaigns.GetByIdAsync(
                        campaignId, token);

                if (campaign is null)
                    return null;

                var members =
                    await unitOfWork.CampaignCustomers.GetByCampaignIdAsync(
                        campaignId, token);

                return members.Select(MapMember).ToList();
            },
            options: CacheOptions,
            cancellationToken: ct);
    }

    public async Task<CampaignCustomerResponse?> AddCustomerAsync(
        int campaignId,
        AddCampaignCustomerRequest request,
        CancellationToken ct = default)
    {
        if (await unitOfWork.Campaigns.GetByIdAsync(campaignId, ct) is null)
            return null;

        if (await unitOfWork.Customers.GetByIdAsync(request.CustomerId, ct) is null)
            throw new ArgumentException("Customer not found.");

        if (await unitOfWork.CampaignCustomers
            .GetByCampaignAndCustomerAsync(
                campaignId, request.CustomerId, ct) is not null)
        {
            throw new InvalidOperationException(
                "Customer is already assigned to this campaign.");
        }

        if (!string.IsNullOrWhiteSpace(request.OfferDetails) &&
            request.OfferDetails.Length > 500)
        {
            throw new ArgumentException(
                "Offer details must be at most 500 characters.");
        }

        if (request.PredictionId.HasValue)
        {
            var prediction =
                await unitOfWork.Predictions.GetByIdAsync(
                    request.PredictionId.Value, ct);

            if (prediction is null ||
                prediction.CustomerId != request.CustomerId)
            {
                throw new ArgumentException(
                    "Prediction must belong to the selected customer.");
            }
        }

        var member = new CampaignCustomer
        {
            CampaignId = campaignId,
            CustomerId = request.CustomerId,
            PredictionId = request.PredictionId,
            OfferDetails = request.OfferDetails?.Trim()
        };

        await unitOfWork.CampaignCustomers.AddAsync(member, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(AudienceKey(campaignId), ct);

        return MapMember(member);
    }

    public async Task<CampaignCustomerResponse?> UpdateCustomerAsync(
        int campaignId,
        int memberId,
        UpdateCampaignCustomerRequest request,
        CancellationToken ct = default)
    {
        var member =
            await unitOfWork.CampaignCustomers.GetByIdAsync(
                memberId, ct);

        if (member is null || member.CampaignId != campaignId)
            return null;

        if (!Enum.TryParse<ContactStatus>(
                request.ContactStatus, true, out var contactStatus))
        {
            throw new ArgumentException(
                "Contact status must be Pending, Contacted, Responded, or Failed.");
        }

        if (!Enum.TryParse<CampaignOutcome>(
                request.CampaignOutcome, true, out var outcome))
        {
            throw new ArgumentException(
                "Campaign outcome must be Unknown, Retained, Churned, or NotInterested.");
        }

        if (request.Notes?.Length > 500)
            throw new ArgumentException(
                "Notes must be at most 500 characters.");

        member.ContactStatus = contactStatus;
        member.ContactDate = request.ContactDate;
        member.CampaignOutcome = outcome;
        member.Notes = request.Notes?.Trim();
        member.UpdatedAt = DateTime.UtcNow;

        unitOfWork.CampaignCustomers.Update(member);

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(AudienceKey(campaignId), ct);

        return MapMember(member);
    }

    private static void Validate(
        string name,
        string type,
        DateTime start,
        DateTime end,
        decimal? budget)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
            throw new ArgumentException(
                "Name is required and must be at most 150 characters.");

        if (string.IsNullOrWhiteSpace(type) || type.Length > 50)
            throw new ArgumentException(
                "Campaign type is required and must be at most 50 characters.");

        if (end < start)
            throw new ArgumentException(
                "End date must be on or after the start date.");

        if (budget < 0)
            throw new ArgumentException("Budget cannot be negative.");

        if (budget > 9999999999.99m)
            throw new ArgumentException(
                "Budget exceeds the supported amount.");
    }

    private static CampaignResponse Map(Campaign c) =>
        new(
            c.Id,
            c.Name,
            c.Description,
            c.CampaignType,
            c.StartDate,
            c.EndDate,
            c.Budget,
            c.Status.ToString(),
            c.CreatedAt);

    private static CampaignCustomerResponse MapMember(
        CampaignCustomer x) =>
        new(
            x.Id,
            x.CampaignId,
            x.CustomerId,
            x.PredictionId,
            x.ContactStatus.ToString(),
            x.ContactDate,
            x.OfferDetails,
            x.CampaignOutcome.ToString(),
            x.Notes);
}