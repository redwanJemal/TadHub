namespace Notification.Contracts.Channels;

/// <summary>
/// Dispatches notifications to all enabled channels for a tenant.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches a notification to a single recipient.
    /// </summary>
    Task DispatchAsync(NotificationContext context, CancellationToken ct = default);

    /// <summary>
    /// Dispatches a notification to multiple recipients.
    /// </summary>
    Task DispatchToManyAsync(
        Guid tenantId,
        IEnumerable<RecipientInfo> recipients,
        string title,
        string body,
        string type,
        string? link,
        string eventType,
        Dictionary<string, string>? templateData = null,
        CancellationToken ct = default);
}

/// <summary>
/// Recipient information for multi-dispatch.
/// </summary>
public sealed record RecipientInfo
{
    public Guid UserId { get; init; }
    public string? Email { get; init; }
}
