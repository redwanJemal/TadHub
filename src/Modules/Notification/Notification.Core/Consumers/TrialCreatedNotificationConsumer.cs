using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin when a trial period starts.
/// </summary>
public sealed class TrialCreatedNotificationConsumer : IConsumer<TrialCreatedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<TrialCreatedNotificationConsumer> _logger;

    public TrialCreatedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<TrialCreatedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrialCreatedEvent> context)
    {
        var evt = context.Message;

        var title = "Trial Period Started";
        var body = $"Trial period started from {evt.StartDate:yyyy-MM-dd} to {evt.EndDate:yyyy-MM-dd}.";
        var link = $"/trials/{evt.TrialId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "info", link,
            "trial.created", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched trial created notification for {TrialId}", evt.TrialId);
    }
}
