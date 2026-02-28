using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Creates notifications when documents are about to expire.
/// Only fires at 30, 7, and 1 day milestones to avoid notification fatigue.
/// </summary>
public sealed class DocumentExpiringNotificationConsumer : IConsumer<DocumentExpiringEvent>
{
    private static readonly int[] MilestoneDays = [30, 7, 1];

    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<DocumentExpiringNotificationConsumer> _logger;

    public DocumentExpiringNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<DocumentExpiringNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentExpiringEvent> context)
    {
        var evt = context.Message;

        // Only fire at milestone days
        if (!MilestoneDays.Contains(evt.DaysRemaining))
        {
            _logger.LogDebug("Skipping document expiry notification for {DaysRemaining} days remaining (not a milestone)",
                evt.DaysRemaining);
            return;
        }

        var type = evt.DaysRemaining <= 1 ? "error" : evt.DaysRemaining <= 7 ? "warning" : "info";
        var title = $"Document Expiring: {evt.DocumentType}";
        var body = $"The {evt.DocumentType} document expires in {evt.DaysRemaining} day(s) on {evt.ExpiresAt:yyyy-MM-dd}.";
        var link = $"/workers/{evt.WorkerId}?tab=documents";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        var templateData = new Dictionary<string, string>
        {
            ["DocumentType"] = evt.DocumentType,
            ["DaysRemaining"] = evt.DaysRemaining.ToString(),
            ["ExpiryDate"] = evt.ExpiresAt.ToString("yyyy-MM-dd"),
            ["WorkerName"] = $"Worker {evt.WorkerId.ToString()[..8]}",
            ["Link"] = link
        };

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId,
            recipients,
            title,
            body,
            type,
            link,
            "document.expiring",
            templateData,
            context.CancellationToken);

        _logger.LogInformation(
            "Dispatched document expiring notification for {DocumentType} ({DaysRemaining}d) to {RecipientCount} recipients",
            evt.DocumentType, evt.DaysRemaining, recipients.Count);
    }
}
