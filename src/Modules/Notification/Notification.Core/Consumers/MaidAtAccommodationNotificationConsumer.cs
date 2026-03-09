using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin when driver confirms pickup (maid at accommodation).
/// </summary>
public sealed class MaidAtAccommodationNotificationConsumer : IConsumer<MaidAtAccommodationEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<MaidAtAccommodationNotificationConsumer> _logger;

    public MaidAtAccommodationNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<MaidAtAccommodationNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MaidAtAccommodationEvent> context)
    {
        var evt = context.Message;

        var title = "Pickup Confirmed";
        var body = "Driver has confirmed pickup. Maid has arrived at accommodation.";
        var link = $"/arrivals/{evt.ArrivalId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "success", link,
            "arrival.pickup_confirmed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched pickup confirmed notification for arrival {ArrivalId}", evt.ArrivalId);
    }
}
