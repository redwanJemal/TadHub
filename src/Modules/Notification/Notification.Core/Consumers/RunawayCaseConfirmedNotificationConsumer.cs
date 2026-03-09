using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies admin and supplier when a runaway case is confirmed.
/// </summary>
public sealed class RunawayCaseConfirmedNotificationConsumer : IConsumer<RunawayCaseConfirmedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<RunawayCaseConfirmedNotificationConsumer> _logger;

    public RunawayCaseConfirmedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<RunawayCaseConfirmedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RunawayCaseConfirmedEvent> context)
    {
        var evt = context.Message;

        var guaranteeNote = evt.IsWithinGuarantee ? " Cost recovery from supplier required." : "";
        var title = "Runaway Case Confirmed";
        var body = $"Runaway case has been officially confirmed.{guaranteeNote}";
        var link = $"/runaway-cases/{evt.RunawayCaseId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "error", link,
            "runaway.confirmed", priority: "urgent", ct: context.CancellationToken);

        _logger.LogInformation("Dispatched runaway case confirmed notification for {RunawayCaseId}", evt.RunawayCaseId);
    }
}
