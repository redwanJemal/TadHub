using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies finance team when a payment is overdue.
/// </summary>
public sealed class OverduePaymentNotificationConsumer : IConsumer<OverduePaymentEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<OverduePaymentNotificationConsumer> _logger;

    public OverduePaymentNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<OverduePaymentNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OverduePaymentEvent> context)
    {
        var evt = context.Message;

        var title = "Overdue Payment Alert";
        var body = $"Invoice payment of {evt.Amount:N2} {evt.Currency} is overdue by {evt.DaysOverdue} day(s).";
        var link = $"/invoices/{evt.InvoiceId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "error", link,
            "payment.overdue", priority: "urgent", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched overdue payment notification for invoice {InvoiceId}, {DaysOverdue} days overdue",
            evt.InvoiceId, evt.DaysOverdue);
    }
}
