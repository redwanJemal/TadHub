using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;

namespace Notification.Core.Channels;

/// <summary>
/// Dispatches notifications to all enabled channels for a tenant.
/// In-app is always included. Other channels are determined by tenant settings.
/// Per-channel errors are caught without blocking other channels.
/// </summary>
public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ITenantNotificationSettingsProvider _settingsProvider;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IEnumerable<INotificationChannel> channels,
        ITenantNotificationSettingsProvider settingsProvider,
        ILogger<NotificationDispatcher> logger)
    {
        _channels = channels;
        _settingsProvider = settingsProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(NotificationContext context, CancellationToken ct = default)
    {
        var enabledChannelTypes = await GetEnabledChannelsAsync(context.TenantId, context.EventType, ct);

        foreach (var channel in _channels)
        {
            if (!enabledChannelTypes.Contains(channel.ChannelType))
                continue;

            try
            {
                if (await channel.IsAvailableAsync(context.TenantId, ct))
                {
                    await channel.SendAsync(context, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to dispatch notification via {ChannelType} for event {EventType} to user {UserId}",
                    channel.ChannelType, context.EventType, context.RecipientUserId);
            }
        }
    }

    public async Task DispatchToManyAsync(
        Guid tenantId,
        IEnumerable<RecipientInfo> recipients,
        string title,
        string body,
        string type,
        string? link,
        string eventType,
        Dictionary<string, string>? templateData = null,
        CancellationToken ct = default)
    {
        foreach (var recipient in recipients)
        {
            var context = new NotificationContext
            {
                TenantId = tenantId,
                RecipientUserId = recipient.UserId,
                RecipientEmail = recipient.Email,
                Title = title,
                Body = body,
                Type = type,
                Link = link,
                EventType = eventType,
                TemplateData = templateData ?? new()
            };

            await DispatchAsync(context, ct);
        }
    }

    private async Task<HashSet<ChannelType>> GetEnabledChannelsAsync(
        Guid tenantId, string eventType, CancellationToken ct)
    {
        var result = new HashSet<ChannelType> { ChannelType.InApp };

        var settings = await _settingsProvider.GetSettingsAsync(tenantId, ct);
        if (settings == null) return result;

        // Check if event type has specific preferences
        if (settings.EventPreferences.TryGetValue(eventType, out var pref))
        {
            if (pref.Muted) return new HashSet<ChannelType>();

            foreach (var channelName in pref.Channels)
            {
                if (Enum.TryParse<ChannelType>(channelName, true, out var channelType))
                    result.Add(channelType);
                else if (channelName == "in_app")
                    result.Add(ChannelType.InApp);
                else if (channelName == "email")
                    result.Add(ChannelType.Email);
            }
        }
        else
        {
            // Default: in-app + email if email is enabled
            if (settings.Email.Enabled)
                result.Add(ChannelType.Email);
        }

        return result;
    }
}
