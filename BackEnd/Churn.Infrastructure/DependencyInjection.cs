using Churn.Application.Interfaces;
using Churn.Infrastructure.ML;
using Churn.Infrastructure.Persistence;
using Churn.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
namespace Churn.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ChurnDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<MLServiceOptions>(
            configuration.GetSection(MLServiceOptions.SectionName));

        services.AddHttpClient<IMLService, MLService>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignCustomerRepository, CampaignCustomerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration["Redis:ConnectionString"];
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });
        return services;
    }
}
