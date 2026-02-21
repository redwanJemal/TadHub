using Microsoft.Extensions.DependencyInjection;
using ReferenceData.Contracts;
using ReferenceData.Core.Seeds;
using ReferenceData.Core.Services;

namespace ReferenceData.Core;

/// <summary>
/// ReferenceData module service registration.
/// </summary>
public static class ReferenceDataServiceRegistration
{
    /// <summary>
    /// Adds ReferenceData module services to the service collection.
    /// </summary>
    public static IServiceCollection AddReferenceDataModule(this IServiceCollection services)
    {
        // Services
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IJobCategoryService, JobCategoryService>();

        // Seeders
        services.AddScoped<CountrySeeder>();
        services.AddScoped<JobCategorySeeder>();

        return services;
    }
}
