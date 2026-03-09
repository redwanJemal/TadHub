using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies office staff on visa approval, rejection, or status change.
/// </summary>
public sealed class VisaStatusChangedNotificationConsumer : IConsumer<VisaStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<VisaStatusChangedNotificationConsumer> _logger;

    public VisaStatusChangedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<VisaStatusChangedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VisaStatusChangedEvent> context)
    {
        var evt = context.Message;

        var type = evt.ToStatus switch
        {
            "Approved" or "Issued" => "success",
            "Rejected" => "error",
            _ => "info"
        };

        var title = $"Visa {evt.ToStatus}: {evt.VisaType}";
        var body = $"Visa application ({evt.VisaType}) changed from {evt.FromStatus} to {evt.ToStatus}.";
        var link = $"/visa-applications/{evt.VisaApplicationId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, type, link,
            "visa.status_changed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched visa status change notification ({FromStatus} -> {ToStatus})",
            evt.FromStatus, evt.ToStatus);
    }
}
