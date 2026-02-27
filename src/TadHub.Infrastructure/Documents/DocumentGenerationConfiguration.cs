using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace TadHub.Infrastructure.Documents;

public static class DocumentGenerationConfiguration
{
    public static IServiceCollection AddDocumentGeneration(this IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return services;
    }
}
