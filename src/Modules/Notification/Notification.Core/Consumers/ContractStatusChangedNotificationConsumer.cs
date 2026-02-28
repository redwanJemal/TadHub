using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.SharedKernel.Events;

namespace Notification.Core.Consumers;

/// <summary>
/// Creates in-app notifications when a contract's status changes.
/// </summary>
public sealed class ContractStatusChangedNotificationConsumer : IConsumer<ContractStatusChangedEvent>
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ILogger<ContractStatusChangedNotificationConsumer> _logger;

    public ContractStatusChangedNotificationConsumer(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ILogger<ContractStatusChangedNotificationConsumer> logger)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContractStatusChangedEvent> context)
    {
        var evt = context.Message;

        var title = $"Contract Status Changed";
        var body = $"Contract {evt.ContractId.ToString()[..8]} status changed from {evt.FromStatus} to {evt.ToStatus}.";
        var link = $"/contracts/{evt.ContractId}";

        var recipients = await _recipientResolver.GetAllMembersAsync(evt.TenantId, context.CancellationToken);

        await _dispatcher.DispatchToManyAsync(
            evt.TenantId,
            recipients,
            title,
            body,
            "info",
            link,
            "contract.status_changed",
            ct: context.CancellationToken);

        _logger.LogInformation(
            "Dispatched contract status change notification for contract {ContractId} ({FromStatus} -> {ToStatus})",
            evt.ContractId, evt.FromStatus, evt.ToStatus);
    }
}
