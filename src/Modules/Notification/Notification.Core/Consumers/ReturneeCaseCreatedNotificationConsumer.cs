using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin when a returnee case is filed.
/// </summary>
public sealed class ReturneeCaseCreatedNotificationConsumer : IConsumer<ReturneeCaseCreatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<ReturneeCaseCreatedNotificationConsumer> _logger;

    public ReturneeCaseCreatedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<ReturneeCaseCreatedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReturneeCaseCreatedEvent> context)
    {
        var evt = context.Message;

        var title = "Returnee Case Filed";
        var body = $"A new returnee case has been filed. Type: {evt.ReturnType}. Reason: {evt.ReturnReason}.";
        var link = $"/returnee-cases/{evt.ReturneeCaseId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "warning", link,
            "returnee.created", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched returnee case created notification for {ReturneeCaseId}", evt.ReturneeCaseId);
    }
}
