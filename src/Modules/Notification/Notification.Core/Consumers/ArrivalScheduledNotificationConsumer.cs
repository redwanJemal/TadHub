using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies driver and admin of a new pickup assignment when arrival is scheduled.
/// </summary>
public sealed class ArrivalScheduledNotificationConsumer : IConsumer<ArrivalScheduledEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<ArrivalScheduledNotificationConsumer> _logger;

    public ArrivalScheduledNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<ArrivalScheduledNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ArrivalScheduledEvent> context)
    {
        var evt = context.Message;

        var flightInfo = string.IsNullOrEmpty(evt.FlightNumber) ? "" : $" (Flight: {evt.FlightNumber})";
        var title = "Arrival Scheduled";
        var body = $"New arrival scheduled for {evt.ScheduledArrivalDate:yyyy-MM-dd}{flightInfo}. Pickup assignment pending.";
        var link = $"/arrivals/{evt.ArrivalId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "info", link,
            "arrival.scheduled", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched arrival scheduled notification for {ArrivalId}", evt.ArrivalId);
    }
}
