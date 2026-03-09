using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin on trial outcome (success or failure).
/// </summary>
public sealed class TrialCompletedNotificationConsumer : IConsumer<TrialCompletedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<TrialCompletedNotificationConsumer> _logger;

    public TrialCompletedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<TrialCompletedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrialCompletedEvent> context)
    {
        var evt = context.Message;

        var isSuccess = evt.Outcome.Equals("Success", StringComparison.OrdinalIgnoreCase);
        var type = isSuccess ? "success" : "warning";
        var title = isSuccess ? "Trial Completed Successfully" : "Trial Failed";
        var body = $"Trial period completed with outcome: {evt.Outcome}.";
        if (!string.IsNullOrEmpty(evt.OutcomeNotes))
            body += $" Notes: {evt.OutcomeNotes}";
        var link = $"/trials/{evt.TrialId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, type, link,
            "trial.completed", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched trial completed notification for {TrialId}, outcome: {Outcome}",
            evt.TrialId, evt.Outcome);
    }
}
