using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Contract.Contracts;
using Contract.Core.Services;

namespace Contract.Core;

public static class ContractServiceRegistration
{
    public static IServiceCollection AddContractModule(this IServiceCollection services)
    {
        services.AddScoped<IContractService, ContractService>();
        services.AddValidatorsFromAssembly(typeof(ContractServiceRegistration).Assembly);
        return services;
    }
}
