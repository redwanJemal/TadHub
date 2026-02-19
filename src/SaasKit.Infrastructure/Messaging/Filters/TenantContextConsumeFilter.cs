using MassTransit;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Messaging.Filters;

/// <summary>
/// MassTransit consume filter that extracts tenant context from incoming messages.
/// Sets up the tenant context for the consumer from the X-Tenant-Id header.
/// </summary>
/// <typeparam name="T">The message type being consumed.</typeparam>
public sealed class TenantContextConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ITenantContextSetter _tenantContextSetter;

    public const string TenantIdHeader = "X-Tenant-Id";

    public TenantContextConsumeFilter(ITenantContextSetter tenantContextSetter)
    {
        _tenantContextSetter = tenantContextSetter;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Extract tenant ID from header
        var tenantIdHeader = context.Headers.Get<string>(TenantIdHeader);
        
        if (!string.IsNullOrEmpty(tenantIdHeader) && Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            _tenantContextSetter.SetTenant(tenantId);
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("tenantContextConsume");
    }
}

/// <summary>
/// Interface for setting tenant context (used by consume filter).
/// Implement this on your TenantContext class.
/// </summary>
public interface ITenantContextSetter
{
    void SetTenant(Guid tenantId, string? slug = null);
}
