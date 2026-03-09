using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin and cashier when a payment is received.
/// </summary>
public sealed class PaymentReceivedNotificationConsumer : IConsumer<PaymentReceivedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<PaymentReceivedNotificationConsumer> _logger;

    public PaymentReceivedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<PaymentReceivedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentReceivedEvent> context)
    {
        var evt = context.Message;

        var title = "Payment Received";
        var body = $"Payment of {evt.Amount:N2} {evt.Currency} received via {evt.PaymentMethod}.";
        var link = $"/payments/{evt.PaymentId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "success", link,
            "payment.received", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched payment received notification for {PaymentId}", evt.PaymentId);
    }
}
