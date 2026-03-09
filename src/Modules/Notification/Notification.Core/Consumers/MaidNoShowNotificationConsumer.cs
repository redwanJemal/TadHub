using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// URGENT: Notifies admin and supplier when maid doesn't arrive (no-show).
/// </summary>
public sealed class MaidNoShowNotificationConsumer : IConsumer<MaidNoShowEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<MaidNoShowNotificationConsumer> _logger;

    public MaidNoShowNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<MaidNoShowNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MaidNoShowEvent> context)
    {
        var evt = context.Message;

        var title = "⚠ Missing Arrival — No-Show";
        var body = $"Maid did not arrive on scheduled date {evt.ScheduledArrivalDate:yyyy-MM-dd}. Immediate action required.";
        var link = $"/arrivals/{evt.ArrivalId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "error", link,
            "arrival.no_show", priority: "urgent", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched URGENT maid no-show notification for arrival {ArrivalId}", evt.ArrivalId);
    }
}
