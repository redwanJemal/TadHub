using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Candidate.Contracts;
using Candidate.Core.Services;

namespace Candidate.Core;

/// <summary>
/// Candidate module service registration.
/// </summary>
public static class CandidateServiceRegistration
{
    /// <summary>
    /// Adds Candidate module services to the service collection.
    /// </summary>
    public static IServiceCollection AddCandidateModule(this IServiceCollection services)
    {
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddValidatorsFromAssembly(typeof(CandidateServiceRegistration).Assembly);
        return services;
    }
}
