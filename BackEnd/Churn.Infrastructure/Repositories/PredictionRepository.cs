using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Churn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Churn.Infrastructure.Repositories;

public sealed class PredictionRepository(ChurnDbContext context) : IPredictionRepository
{
    public async Task<Prediction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Predictions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Prediction>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Predictions.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Prediction entity, CancellationToken cancellationToken = default)
        => await context.Predictions.AddAsync(entity, cancellationToken);

    public void Update(Prediction entity) => context.Predictions.Update(entity);

    public void Delete(Prediction entity) => context.Predictions.Remove(entity);
}
