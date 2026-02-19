using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace Audit.Contracts;

public record AuditEventDto(Guid Id, string EventName, string? Payload, Guid? UserId, DateTimeOffset CreatedAt);
public record AuditLogDto(Guid Id, string Action, string EntityType, Guid EntityId, string? OldValues, string? NewValues, Guid? UserId, DateTimeOffset CreatedAt);
public record WebhookDto(Guid Id, string Url, List<string>? Events, bool IsActive, DateTimeOffset? LastTriggeredAt, int FailureCount, DateTimeOffset CreatedAt);
public record CreateWebhookRequest(string Url, List<string>? Events);

public interface IAuditService
{
    Task<PagedList<AuditEventDto>> GetEventsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<AuditLogDto>> GetLogsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task RecordEventAsync(Guid tenantId, string eventName, object? payload, Guid? userId, string? ipAddress, CancellationToken ct = default);
    Task RecordLogAsync(Guid tenantId, string action, string entityType, Guid entityId, object? oldValues, object? newValues, Guid? userId, CancellationToken ct = default);
}

public interface IWebhookService
{
    Task<PagedList<WebhookDto>> GetWebhooksAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<WebhookDto>> CreateWebhookAsync(Guid tenantId, CreateWebhookRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteWebhookAsync(Guid tenantId, Guid webhookId, CancellationToken ct = default);
    Task TriggerWebhooksAsync(Guid tenantId, string eventName, object payload, CancellationToken ct = default);
}
