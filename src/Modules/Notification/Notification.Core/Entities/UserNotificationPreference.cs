using TadHub.SharedKernel.Entities;

namespace Notification.Core.Entities;

/// <summary>
/// Per-user notification preference for a specific event type.
/// </summary>
public class UserNotificationPreference : SoftDeletableEntity
{
    /// <summary>
    /// The user this preference belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The event type (e.g., "candidate.status_changed").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this event type is muted for this user.
    /// </summary>
    public bool Muted { get; set; }

    /// <summary>
    /// Enabled channels as comma-separated string (e.g., "in_app,email").
    /// </summary>
    public string Channels { get; set; } = "in_app";
}
