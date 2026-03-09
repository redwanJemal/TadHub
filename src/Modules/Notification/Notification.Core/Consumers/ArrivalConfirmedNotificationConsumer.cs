using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin and accommodation staff when arrival is confirmed.
/// </summary>
public sealed class ArrivalConfirmedNotificationConsumer : IConsumer<ArrivalConfirmedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<ArrivalConfirmedNotificationConsumer> _logger;

    public ArrivalConfirmedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<ArrivalConfirmedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ArrivalConfirmedEvent> context)
    {
        var evt = context.Message;

        var title = "Arrival Confirmed";
        var body = $"Arrival confirmed at {evt.ActualArrivalTime:yyyy-MM-dd HH:mm}. Accommodation preparation needed.";
        var link = $"/arrivals/{evt.ArrivalId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "success", link,
            "arrival.confirmed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched arrival confirmed notification for {ArrivalId}", evt.ArrivalId);
    }
}
