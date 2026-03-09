using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies office staff when a visa or Emirates ID is issued.
/// </summary>
public sealed class VisaIssuedNotificationConsumer : IConsumer<VisaIssuedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<VisaIssuedNotificationConsumer> _logger;

    public VisaIssuedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<VisaIssuedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VisaIssuedEvent> context)
    {
        var evt = context.Message;

        var visaInfo = string.IsNullOrEmpty(evt.VisaNumber) ? "" : $" (No: {evt.VisaNumber})";
        var title = $"{evt.VisaType} Issued";
        var body = $"{evt.VisaType} has been issued{visaInfo}.";
        var link = $"/visa-applications/{evt.VisaApplicationId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "success", link,
            "visa.issued", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched visa issued notification for {VisaApplicationId}", evt.VisaApplicationId);
    }
}
