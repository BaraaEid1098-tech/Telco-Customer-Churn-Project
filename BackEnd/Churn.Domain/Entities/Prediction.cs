using Churn.Domain.Common;
using Churn.Domain.Enums;

namespace Churn.Domain.Entities;

public class Prediction : BaseEntity
{
    public int CustomerId { get; set; }
    public decimal ChurnProbability { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string ModelVersion { get; set; } = null!;
    public string? TopFactors { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<CampaignCustomer> CampaignCustomers { get; set; } = new List<CampaignCustomer>();
}
