using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Keycloak;

/// <summary>
/// Keycloak Admin API client configuration and service registration.
/// </summary>
public static class KeycloakConfiguration
{
    /// <summary>
    /// Adds Keycloak Admin API client services to the service collection.
    /// </summary>
    public static IServiceCollection AddKeycloakAdmin(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<KeycloakSettings>(
            configuration.GetSection(KeycloakSettings.SectionName));

        // Configure HttpClient for Keycloak Admin API
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>((sp, client) =>
        {
            var settings = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                ?? throw new InvalidOperationException("Keycloak settings not configured");

            // Parse realm name from authority URL
            // Authority format: http://localhost:8080/realms/saas-platform
            var authorityUri = new Uri(settings.Authority);
            var pathSegments = authorityUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2 || pathSegments[0] != "realms")
                throw new InvalidOperationException(
                    $"Invalid Keycloak Authority URL format: {settings.Authority}. " +
                    "Expected format: http://host:port/realms/realm-name");

            var realmName = pathSegments[1];
            var baseUrl = $"{authorityUri.Scheme}://{authorityUri.Host}:{authorityUri.Port}";

            // Admin API base URL: /admin/realms/{realm}/
            client.BaseAddress = new Uri($"{baseUrl}/admin/realms/{realmName}/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // For development environments that might use self-signed certs
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        return services;
    }
}
