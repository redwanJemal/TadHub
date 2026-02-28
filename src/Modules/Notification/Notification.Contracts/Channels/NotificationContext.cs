namespace Notification.Contracts.Channels;

/// <summary>
/// Context for dispatching a notification across channels.
/// </summary>
public sealed record NotificationContext
{
    public Guid TenantId { get; init; }
    public Guid RecipientUserId { get; init; }
    public string? RecipientEmail { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public string? Link { get; init; }
    public string EventType { get; init; } = string.Empty;
    public Dictionary<string, string> TemplateData { get; init; } = new();
}
