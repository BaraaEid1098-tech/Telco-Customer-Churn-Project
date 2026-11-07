namespace Churn.Application.Interfaces;

public interface IUnitOfWork
{
    ICustomerRepository Customers { get; }
    IPredictionRepository Predictions { get; }
    ICampaignRepository Campaigns { get; }
    ICampaignCustomerRepository CampaignCustomers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
