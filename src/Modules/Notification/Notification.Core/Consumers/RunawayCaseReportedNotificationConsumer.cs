using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// URGENT: Notifies admin and supplier when a runaway case is reported.
/// </summary>
public sealed class RunawayCaseReportedNotificationConsumer : IConsumer<RunawayCaseReportedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<RunawayCaseReportedNotificationConsumer> _logger;

    public RunawayCaseReportedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<RunawayCaseReportedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RunawayCaseReportedEvent> context)
    {
        var evt = context.Message;

        var guaranteeNote = evt.IsWithinGuarantee ? " (within guarantee period)" : "";
        var title = "⚠ Runaway Case Reported";
        var body = $"A runaway case has been reported{guaranteeNote}. Immediate action required.";
        var link = $"/runaway-cases/{evt.RunawayCaseId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "error", link,
            "runaway.reported", priority: "urgent", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched URGENT runaway case reported notification for {RunawayCaseId}", evt.RunawayCaseId);
    }
}
