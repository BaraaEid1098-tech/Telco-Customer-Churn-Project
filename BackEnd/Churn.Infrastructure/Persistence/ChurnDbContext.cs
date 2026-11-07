using Churn.Domain.Entities;
using Churn.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Churn.Infrastructure.Persistence;

public sealed class ChurnDbContext : IdentityDbContext<ApplicationUser>
{
    public ChurnDbContext(DbContextOptions<ChurnDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignCustomer> CampaignCustomers => Set<CampaignCustomer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ChurnDbContext).Assembly);
    }
}
