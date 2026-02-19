using TadHub.SharedKernel.Entities;

namespace Notification.Core.Entities;

/// <summary>
/// Represents a notification for a user within a tenant.
/// </summary>
public class Notification : TenantScopedEntity
{
    /// <summary>
    /// The user this notification is for.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Notification title/headline.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification body/message.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Notification type: info, warning, success, error.
    /// </summary>
    public string Type { get; set; } = "info";

    /// <summary>
    /// Optional link/action URL.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// When the notification was read.
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }
}
