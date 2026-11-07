using Churn.Domain.Entities;

namespace Churn.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer entity, CancellationToken cancellationToken = default);
    void Update(Customer entity);
    void Delete(Customer entity);
}
