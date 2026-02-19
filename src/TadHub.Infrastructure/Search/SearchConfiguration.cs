using Meilisearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Search;

/// <summary>
/// Search service registration.
/// </summary>
public static class SearchConfiguration
{
    /// <summary>
    /// Adds Meilisearch services.
    /// </summary>
    public static IServiceCollection AddSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(MeilisearchSettings.SectionName).Get<MeilisearchSettings>()
            ?? new MeilisearchSettings();

        services.Configure<MeilisearchSettings>(configuration.GetSection(MeilisearchSettings.SectionName));

        // Register Meilisearch client
        services.AddSingleton(_ => new MeilisearchClient(settings.Url, settings.ApiKey));

        services.AddScoped<ISearchService, MeilisearchService>();

        return services;
    }
}
