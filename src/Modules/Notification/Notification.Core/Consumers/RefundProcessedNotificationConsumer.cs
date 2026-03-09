using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin when a refund is processed.
/// </summary>
public sealed class RefundProcessedNotificationConsumer : IConsumer<RefundProcessedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<RefundProcessedNotificationConsumer> _logger;

    public RefundProcessedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<RefundProcessedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RefundProcessedEvent> context)
    {
        var evt = context.Message;

        var title = "Refund Processed";
        var body = $"Refund of {evt.Amount:N2} {evt.Currency} has been processed.";
        if (!string.IsNullOrEmpty(evt.Reason))
            body += $" Reason: {evt.Reason}";
        var link = evt.InvoiceId.HasValue ? $"/invoices/{evt.InvoiceId}" : "/payments";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "info", link,
            "refund.processed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched refund processed notification for {RefundId}", evt.RefundId);
    }
}
