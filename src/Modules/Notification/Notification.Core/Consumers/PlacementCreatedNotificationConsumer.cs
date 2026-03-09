using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies office staff when a booking/placement is created.
/// </summary>
public sealed class PlacementCreatedNotificationConsumer : IConsumer<PlacementCreatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<PlacementCreatedNotificationConsumer> _logger;

    public PlacementCreatedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<PlacementCreatedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementCreatedEvent> context)
    {
        var evt = context.Message;

        var title = "New Booking Confirmed";
        var body = $"A new placement has been created (ID: {evt.PlacementId.ToString()[..8]}).";
        var link = $"/placements/{evt.PlacementId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "success", link,
            "placement.created", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched placement created notification for {PlacementId}", evt.PlacementId);
    }
}
