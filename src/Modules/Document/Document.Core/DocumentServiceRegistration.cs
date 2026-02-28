using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Document.Contracts;
using Document.Core.Services;

namespace Document.Core;

public static class DocumentServiceRegistration
{
    public static IServiceCollection AddDocumentModule(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddValidatorsFromAssembly(typeof(DocumentServiceRegistration).Assembly);
        return services;
    }
}
