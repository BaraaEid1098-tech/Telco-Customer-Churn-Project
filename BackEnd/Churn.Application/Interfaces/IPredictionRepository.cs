using Churn.Domain.Entities;

namespace Churn.Application.Interfaces;

public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prediction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Prediction entity, CancellationToken cancellationToken = default);
    void Update(Prediction entity);
    void Delete(Prediction entity);
}
