using MassTransit;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Messaging.Filters;

/// <summary>
/// MassTransit publish filter that adds tenant context to outgoing messages.
/// Ensures all published events carry the TenantId header for multi-tenancy.
/// </summary>
/// <typeparam name="T">The message type being published.</typeparam>
public sealed class TenantContextPublishFilter<T> : IFilter<PublishContext<T>> where T : class
{
    private readonly ITenantContext _tenantContext;

    public const string TenantIdHeader = "X-Tenant-Id";

    public TenantContextPublishFilter(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        // Add tenant ID header if tenant context is resolved
        if (_tenantContext.IsResolved)
        {
            context.Headers.Set(TenantIdHeader, _tenantContext.TenantId.ToString());
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("tenantContextPublish");
    }
}

/// <summary>
/// MassTransit send filter that adds tenant context to outgoing messages.
/// Ensures all sent commands carry the TenantId header for multi-tenancy.
/// </summary>
/// <typeparam name="T">The message type being sent.</typeparam>
public sealed class TenantContextSendFilter<T> : IFilter<SendContext<T>> where T : class
{
    private readonly ITenantContext _tenantContext;

    public const string TenantIdHeader = "X-Tenant-Id";

    public TenantContextSendFilter(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        // Add tenant ID header if tenant context is resolved
        if (_tenantContext.IsResolved)
        {
            context.Headers.Set(TenantIdHeader, _tenantContext.TenantId.ToString());
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("tenantContextSend");
    }
}
