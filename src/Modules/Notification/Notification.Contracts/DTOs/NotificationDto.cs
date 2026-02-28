namespace Notification.Contracts.DTOs;

/// <summary>
/// DTO for notification data.
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public string? Link { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request to create a notification.
/// </summary>
public record CreateNotificationRequest
{
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public string? Link { get; init; }
}

/// <summary>
/// Unread count response.
/// </summary>
public record UnreadCountDto
{
    public int Count { get; init; }
}

/// <summary>
/// Admin request to send notifications to tenant members.
/// </summary>
public record AdminSendNotificationRequest
{
    public Guid TenantId { get; init; }
    public List<Guid>? UserIds { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public string? Link { get; init; }
}

/// <summary>
/// Response after sending admin notifications.
/// </summary>
public record AdminSendNotificationResponse
{
    public int RecipientCount { get; init; }
    public int TenantCount { get; init; }
}
