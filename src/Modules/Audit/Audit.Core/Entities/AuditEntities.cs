using SaasKit.SharedKernel.Entities;

namespace Audit.Core.Entities;

public class AuditEvent : TenantScopedEntity
{
    public string EventName { get; set; } = string.Empty;
    public string? Payload { get; set; } // JSONB
    public string? Metadata { get; set; } // JSONB
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
}

public class AuditLog : TenantScopedEntity
{
    public string Action { get; set; } = string.Empty; // create, update, delete
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; } // JSONB
    public string? NewValues { get; set; } // JSONB
    public Guid? UserId { get; set; }
}

public class Webhook : TenantScopedEntity
{
    public string Url { get; set; } = string.Empty;
    public string? Events { get; set; } // JSON array of event names
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastTriggeredAt { get; set; }
    public int FailureCount { get; set; } = 0;
    public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

public class WebhookDelivery : TenantScopedEntity
{
    public Guid WebhookId { get; set; }
    public Webhook Webhook { get; set; } = null!;
    public string EventName { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public int? StatusCode { get; set; }
    public int Attempts { get; set; } = 0;
    public string Status { get; set; } = "pending"; // pending, success, failed
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
}
