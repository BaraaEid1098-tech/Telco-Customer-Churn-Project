using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Churn.Infrastructure.Repositories;

public sealed class CampaignCustomerRepository(ChurnDbContext context) : ICampaignCustomerRepository
{
    public Task<CampaignCustomer?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.CampaignCustomers.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<CampaignCustomer?> GetByCampaignAndCustomerAsync(int campaignId, int customerId, CancellationToken ct = default) =>
        context.CampaignCustomers.FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.CustomerId == customerId, ct);
    public async Task<IReadOnlyList<CampaignCustomer>> GetByCampaignIdAsync(int campaignId, CancellationToken ct = default) =>
        await context.CampaignCustomers.AsNoTracking().Where(x => x.CampaignId == campaignId).OrderBy(x => x.Id).ToListAsync(ct);
    public async Task AddAsync(CampaignCustomer entity, CancellationToken ct = default) => await context.CampaignCustomers.AddAsync(entity, ct);
    public void Update(CampaignCustomer entity) => context.CampaignCustomers.Update(entity);
}
