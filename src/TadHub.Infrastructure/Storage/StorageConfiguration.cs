using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Storage;

/// <summary>
/// Storage service registration.
/// </summary>
public static class StorageConfiguration
{
    /// <summary>
    /// Adds MinIO file storage services.
    /// </summary>
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(MinioSettings.SectionName).Get<MinioSettings>()
            ?? new MinioSettings();

        services.Configure<MinioSettings>(configuration.GetSection(MinioSettings.SectionName));

        // Register MinIO client
        services.AddSingleton<IMinioClient>(_ =>
        {
            var endpoint = settings.Endpoint;
            var useSsl = settings.UseHttps;

            // Handle full URL format (e.g. "https://storage.example.com")
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
                && (uri.Scheme == "https" || uri.Scheme == "http"))
            {
                endpoint = uri.Host + (uri.IsDefaultPort ? "" : $":{uri.Port}");
                useSsl = uri.Scheme == "https";
            }

            var client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (useSsl)
                client.WithSSL();

            return client.Build();
        });

        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<ITenantFileService, TenantFileService>();

        return services;
    }
}
