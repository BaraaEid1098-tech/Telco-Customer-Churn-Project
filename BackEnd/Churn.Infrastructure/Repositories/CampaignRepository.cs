using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Churn.Infrastructure.Repositories;

public sealed class CampaignRepository(ChurnDbContext context) : ICampaignRepository
{
    public async Task<Campaign?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Campaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Campaign>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Campaigns.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Campaign entity, CancellationToken cancellationToken = default)
        => await context.Campaigns.AddAsync(entity, cancellationToken);

    public void Update(Campaign entity) => context.Campaigns.Update(entity);

    public void Delete(Campaign entity) => context.Campaigns.Remove(entity);
}
