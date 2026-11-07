using Churn.Domain.Entities;

namespace Churn.Application.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Campaign>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Campaign entity, CancellationToken cancellationToken = default);
    void Update(Campaign entity);
    void Delete(Campaign entity);
}
