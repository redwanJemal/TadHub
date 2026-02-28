using Financial.Contracts;
using Financial.Core.Gateways;
using Financial.Core.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Financial.Core;

public static class FinancialServiceRegistration
{
    public static IServiceCollection AddFinancialModule(this IServiceCollection services)
    {
        services.AddScoped<IDiscountProgramService, DiscountProgramService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();
        services.AddScoped<IPaymentGateway, ManualPaymentGateway>();
        services.AddValidatorsFromAssembly(typeof(FinancialServiceRegistration).Assembly);
        return services;
    }
}
