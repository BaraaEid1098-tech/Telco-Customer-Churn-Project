using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Churn.Infrastructure.Repositories;

public sealed class CustomerRepository(ChurnDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await context.Customers.FirstOrDefaultAsync(x => x.CustomerCode == code, cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Customers.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
        => await context.Customers.AddAsync(entity, cancellationToken);

    public void Update(Customer entity) => context.Customers.Update(entity);

    public void Delete(Customer entity) => context.Customers.Remove(entity);
}
