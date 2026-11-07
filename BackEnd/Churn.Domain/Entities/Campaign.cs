using Churn.Domain.Common;
using Churn.Domain.Enums;

namespace Churn.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string CampaignType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Budget { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    public ICollection<CampaignCustomer> CampaignCustomers { get; set; } = new List<CampaignCustomer>();
}
