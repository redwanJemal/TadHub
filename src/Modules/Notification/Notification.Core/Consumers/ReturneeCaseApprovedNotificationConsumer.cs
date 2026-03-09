using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Notifies office staff and supplier when a returnee case is approved.
/// Also notifies supplier of cost recovery if within guarantee.
/// </summary>
public sealed class ReturneeCaseApprovedNotificationConsumer : IConsumer<ReturneeCaseApprovedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<ReturneeCaseApprovedNotificationConsumer> _logger;

    public ReturneeCaseApprovedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<ReturneeCaseApprovedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReturneeCaseApprovedEvent> context)
    {
        var evt = context.Message;

        var title = "Returnee Case Approved";
        var body = $"Returnee case approved. Type: {evt.ReturnType}.";
        if (evt.IsWithinGuarantee && evt.RefundAmount.HasValue)
            body += $" Cost recovery: {evt.RefundAmount:N2} (within guarantee, {evt.MonthsWorked} months worked).";
        var link = $"/returnee-cases/{evt.ReturneeCaseId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        var priority = evt.IsWithinGuarantee ? "urgent" : "normal";
        await _dispatcher.DispatchToManyAsync(
            evt.TenantId, recipients, title, body, "info", link,
            "returnee.approved", priority: priority, ct: context.CancellationToken);

        _logger.LogInformation("Dispatched returnee case approved notification for {ReturneeCaseId}", evt.ReturneeCaseId);
    }
}
