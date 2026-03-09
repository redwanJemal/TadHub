using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies finance team when a supplier commission is due.
/// </summary>
public sealed class SupplierCommissionDueNotificationConsumer : IConsumer<SupplierCommissionDueEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<SupplierCommissionDueNotificationConsumer> _logger;

    public SupplierCommissionDueNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<SupplierCommissionDueNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SupplierCommissionDueEvent> context)
    {
        var evt = context.Message;

        var title = "Supplier Commission Due";
        var body = $"Commission of {evt.Amount:N2} {evt.Currency} is due for supplier {evt.SupplierNameEn}.";
        var link = "/supplier-payments";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "warning", link,
            "supplier.commission_due", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched supplier commission due notification for {SupplierId}", evt.SupplierId);
    }
}
