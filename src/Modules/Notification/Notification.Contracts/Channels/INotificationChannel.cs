namespace Notification.Contracts.Channels;

/// <summary>
/// Represents a notification delivery channel (in-app, email, etc.).
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// The type of this channel.
    /// </summary>
    ChannelType ChannelType { get; }

    /// <summary>
    /// Sends a notification through this channel.
    /// </summary>
    Task SendAsync(NotificationContext context, CancellationToken ct = default);

    /// <summary>
    /// Checks whether this channel is available for the given tenant.
    /// </summary>
    Task<bool> IsAvailableAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Supported notification channel types.
/// </summary>
public enum ChannelType
{
    InApp,
    Email,
    WhatsApp,
    Telegram
}
