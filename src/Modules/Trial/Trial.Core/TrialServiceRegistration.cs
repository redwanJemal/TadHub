using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Trial.Contracts;
using Trial.Core.Services;

namespace Trial.Core;

public static class TrialServiceRegistration
{
    public static IServiceCollection AddTrialModule(this IServiceCollection services)
    {
        services.AddScoped<ITrialService, TrialService>();
        services.AddValidatorsFromAssembly(typeof(TrialServiceRegistration).Assembly);
        return services;
    }
}
