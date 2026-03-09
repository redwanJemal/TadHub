namespace Notification.Contracts.DTOs;

public sealed record UserNotificationPreferenceDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public bool Muted { get; init; }
    public string Channels { get; init; } = "in_app";
}

public sealed record UpdateUserNotificationPreferenceRequest
{
    public string EventType { get; init; } = string.Empty;
    public bool Muted { get; init; }
    public string Channels { get; init; } = "in_app";
}

public sealed record BulkUpdateUserPreferencesRequest
{
    public List<UpdateUserNotificationPreferenceRequest> Preferences { get; init; } = new();
}
