using Audit.Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Messaging.Observers;

/// <summary>
/// MassTransit publish observer that records every published IDomainEvent
/// as an AuditEvent for the activity log.
/// </summary>
public sealed class AuditPublishObserver : IPublishObserver
{
    private readonly IServiceProvider _serviceProvider;

    public AuditPublishObserver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task PrePublish<T>(PublishContext<T> context) where T : class
        => Task.CompletedTask;

    public async Task PostPublish<T>(PublishContext<T> context) where T : class
    {
        if (context.Message is not IDomainEvent)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

            // Extract TenantId from the event itself (most reliable in background context)
            var tenantId = Guid.Empty;
            var tenantIdProp = context.Message.GetType().GetProperty("TenantId");
            if (tenantIdProp?.GetValue(context.Message) is Guid eventTenantId && eventTenantId != Guid.Empty)
                tenantId = eventTenantId;

            // Fallback to tenant context if available
            if (tenantId == Guid.Empty && tenantContext.IsResolved)
                tenantId = tenantContext.TenantId;

            if (tenantId == Guid.Empty)
                return;

            // Set tenant context so interceptors work correctly
            if (tenantContext is TadHub.Infrastructure.Auth.TenantContext mutableContext)
                mutableContext.SetTenant(tenantId);

            Guid? userId = null;
            var userIdProp = context.Message.GetType().GetProperty("ChangedByUserId");
            if (userIdProp?.GetValue(context.Message) is string userIdStr && Guid.TryParse(userIdStr, out var eventUserId))
                userId = eventUserId;

            await auditService.RecordEventAsync(
                tenantId,
                typeof(T).Name,
                context.Message,
                userId,
                ipAddress: null,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<AuditPublishObserver>();
            logger?.LogError(ex, "Failed to record audit event for {MessageType}", typeof(T).Name);
        }
    }

    public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        => Task.CompletedTask;
}
