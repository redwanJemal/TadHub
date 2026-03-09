using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Reminds office staff before trial period ends (e.g., day 4 of 5).
/// </summary>
public sealed class TrialCompletionDueNotificationConsumer : IConsumer<TrialCompletionDueEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<TrialCompletionDueNotificationConsumer> _logger;

    public TrialCompletionDueNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<TrialCompletionDueNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrialCompletionDueEvent> context)
    {
        var evt = context.Message;

        var title = "Trial Period Ending Soon";
        var body = $"Trial period ends on {evt.EndDate:yyyy-MM-dd} ({evt.DaysRemaining} day(s) remaining). Please evaluate.";
        var link = $"/trials/{evt.TrialId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "warning", link,
            "trial.completion_due", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched trial completion due notification for {TrialId}, {DaysRemaining} days remaining",
            evt.TrialId, evt.DaysRemaining);
    }
}
