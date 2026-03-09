using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin and supplier on placement deployment and status changes.
/// </summary>
public sealed class PlacementStatusChangedNotificationConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<PlacementStatusChangedNotificationConsumer> _logger;

    public PlacementStatusChangedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<PlacementStatusChangedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementStatusChangedEvent> context)
    {
        var evt = context.Message;

        var title = $"Placement Status: {evt.ToStatus}";
        var body = $"Placement {evt.PlacementId.ToString()[..8]} changed from {evt.FromStatus} to {evt.ToStatus}.";
        var link = $"/placements/{evt.PlacementId}";
        var type = evt.ToStatus == "Deployed" ? "success" : "info";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, type, link,
            "placement.status_changed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched placement status change notification ({FromStatus} -> {ToStatus})",
            evt.FromStatus, evt.ToStatus);
    }
}
