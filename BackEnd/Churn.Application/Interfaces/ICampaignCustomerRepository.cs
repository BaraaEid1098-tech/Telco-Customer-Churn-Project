using Churn.Domain.Entities;

namespace Churn.Application.Interfaces;

public interface ICampaignCustomerRepository
{
    Task<CampaignCustomer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CampaignCustomer?> GetByCampaignAndCustomerAsync(int campaignId, int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampaignCustomer>> GetByCampaignIdAsync(int campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(CampaignCustomer entity, CancellationToken cancellationToken = default);
    void Update(CampaignCustomer entity);
}
