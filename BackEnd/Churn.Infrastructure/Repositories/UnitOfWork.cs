using Churn.Application.Interfaces;
using Churn.Infrastructure.Persistence;

namespace Churn.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ChurnDbContext _context;

    public ICustomerRepository Customers { get; }
    public IPredictionRepository Predictions { get; }
    public ICampaignRepository Campaigns { get; }
    public ICampaignCustomerRepository CampaignCustomers { get; }

    public UnitOfWork(
        ChurnDbContext context,
        ICustomerRepository customers,
        IPredictionRepository predictions,
        ICampaignRepository campaigns,
        ICampaignCustomerRepository campaignCustomers)
    {
        _context = context;
        Customers = customers;
        Predictions = predictions;
        Campaigns = campaigns;
        CampaignCustomers = campaignCustomers;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
