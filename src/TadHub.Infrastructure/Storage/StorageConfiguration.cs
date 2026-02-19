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
            var client = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (!settings.UseHttps)
            {
                // MinIO defaults to HTTPS, explicitly use HTTP for local dev
            }

            return client.Build();
        });

        services.AddScoped<IFileStorageService, MinioFileStorageService>();

        return services;
    }
}
