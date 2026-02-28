using Microsoft.Extensions.Logging;
using Notification.Contracts;
using Notification.Contracts.Channels;
using Notification.Contracts.DTOs;

namespace Notification.Core.Channels;

/// <summary>
/// In-app notification channel. Creates a notification record and pushes via SSE.
/// Always available.
/// </summary>
public sealed class InAppNotificationChannel : INotificationChannel
{
    private readonly INotificationModuleService _notificationService;
    private readonly ILogger<InAppNotificationChannel> _logger;

    public ChannelType ChannelType => ChannelType.InApp;

    public InAppNotificationChannel(
        INotificationModuleService notificationService,
        ILogger<InAppNotificationChannel> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendAsync(NotificationContext context, CancellationToken ct = default)
    {
        var result = await _notificationService.CreateAsync(
            context.TenantId,
            new CreateNotificationRequest
            {
                UserId = context.RecipientUserId,
                Title = context.Title,
                Body = context.Body,
                Type = context.Type,
                Link = context.Link
            },
            ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create in-app notification for user {UserId}: {Error}",
                context.RecipientUserId, result.Error);
        }
    }

    public Task<bool> IsAvailableAsync(Guid tenantId, CancellationToken ct = default)
    {
        return Task.FromResult(true);
    }
}
