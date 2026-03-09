using TadHub.SharedKernel.Entities;

namespace Notification.Core.Entities;

/// <summary>
/// Configurable notification template per tenant and event type.
/// </summary>
public class NotificationTemplate : SoftDeletableEntity
{
    /// <summary>
    /// The event type this template is for (e.g., "candidate.status_changed").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Template title in English. Supports placeholders like {WorkerName}.
    /// </summary>
    public string TitleEn { get; set; } = string.Empty;

    /// <summary>
    /// Template title in Arabic.
    /// </summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// Template body in English. Supports placeholders.
    /// </summary>
    public string BodyEn { get; set; } = string.Empty;

    /// <summary>
    /// Template body in Arabic.
    /// </summary>
    public string BodyAr { get; set; } = string.Empty;

    /// <summary>
    /// Default priority for this event type: normal, urgent.
    /// </summary>
    public string DefaultPriority { get; set; } = "normal";

    /// <summary>
    /// Whether this template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
