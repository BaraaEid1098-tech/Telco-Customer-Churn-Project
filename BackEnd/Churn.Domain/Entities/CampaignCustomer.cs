using Churn.Domain.Common;
using Churn.Domain.Enums;

namespace Churn.Domain.Entities;

public class CampaignCustomer : BaseEntity
{
    public int CampaignId { get; set; }
    public int CustomerId { get; set; }
    public int? PredictionId { get; set; }

    public ContactStatus ContactStatus { get; set; } = ContactStatus.Pending;
    public DateTime? ContactDate { get; set; }
    public string? OfferDetails { get; set; }
    public CampaignOutcome CampaignOutcome { get; set; } = CampaignOutcome.Unknown;
    public string? Notes { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Prediction? Prediction { get; set; }
}
