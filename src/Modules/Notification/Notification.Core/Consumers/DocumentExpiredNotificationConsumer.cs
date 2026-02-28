using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Creates notifications when documents have expired.
/// </summary>
public sealed class DocumentExpiredNotificationConsumer : IConsumer<DocumentExpiredEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<DocumentExpiredNotificationConsumer> _logger;

    public DocumentExpiredNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<DocumentExpiredNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentExpiredEvent> context)
    {
        var evt = context.Message;

        var title = $"Document Expired: {evt.DocumentType}";
        var body = $"The {evt.DocumentType} document has expired. Immediate action is required.";
        var link = $"/workers/{evt.WorkerId}?tab=documents";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        var templateData = new Dictionary<string, string>
        {
            ["DocumentType"] = evt.DocumentType,
            ["WorkerName"] = $"Worker {evt.WorkerId.ToString()[..8]}",
            ["Link"] = link
        };

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId,
            recipients,
            title,
            body,
            "error",
            link,
            "document.expired",
            templateData,
            context.CancellationToken);

        _logger.LogInformation(
            "Dispatched document expired notification for {DocumentType} to {RecipientCount} recipients",
            evt.DocumentType, recipients.Count);
    }
}
