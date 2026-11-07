using Microsoft.Extensions.DependencyInjection;
using Churn.Application.Services;

namespace Churn.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<CampaignService>();
        services.AddScoped<PredictionService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<ImportService>();

        return services;
    }
}
