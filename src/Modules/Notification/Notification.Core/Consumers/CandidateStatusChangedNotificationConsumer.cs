using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Creates in-app notifications when a candidate's status changes.
/// </summary>
public sealed class CandidateStatusChangedNotificationConsumer : IConsumer<CandidateStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<CandidateStatusChangedNotificationConsumer> _logger;

    public CandidateStatusChangedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<CandidateStatusChangedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CandidateStatusChangedEvent> context)
    {
        var evt = context.Message;

        var title = $"Candidate Status Changed: {evt.FullNameEn}";
        var body = $"{evt.FullNameEn} status changed from {evt.FromStatus} to {evt.ToStatus}.";
        var link = $"/candidates/{evt.CandidateId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId,
            recipients,
            title,
            body,
            "info",
            link,
            "candidate.status_changed",
            ct: context.CancellationToken);

        _logger.LogInformation(
            "Dispatched candidate status change notification for {CandidateName} ({FromStatus} -> {ToStatus})",
            evt.FullNameEn, evt.FromStatus, evt.ToStatus);
    }
}
