using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Contract.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Contract.Core.Consumers;

/// <summary>
/// Consumes RunawayCaseReportedEvent to terminate the contract
/// via EF Core change tracking (so audit interceptors fire correctly).
/// </summary>
public class RunawayCaseReportedConsumer : IConsumer<RunawayCaseReportedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<RunawayCaseReportedConsumer> _logger;

    public RunawayCaseReportedConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<RunawayCaseReportedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RunawayCaseReportedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Received RunawayCaseReportedEvent for contract {ContractId} in tenant {TenantId}",
            message.ContractId, message.TenantId);

        var contract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == message.ContractId && x.TenantId == message.TenantId && !x.IsDeleted, ct);

        if (contract is null)
        {
            _logger.LogWarning("Contract {ContractId} not found in tenant {TenantId}, skipping", message.ContractId, message.TenantId);
            return;
        }

        if (contract.Status == ContractStatus.Terminated)
        {
            _logger.LogWarning("Contract {ContractId} is already Terminated, skipping", message.ContractId);
            return;
        }

        var now = _clock.UtcNow;
        var fromStatus = contract.Status;

        contract.Status = ContractStatus.Terminated;
        contract.StatusChangedAt = now;
        contract.TerminationReasonType = TerminationReason.Runaway;
        contract.TerminationReason = "Runaway";

        var history = new ContractStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            ContractId = contract.Id,
            FromStatus = fromStatus,
            ToStatus = ContractStatus.Terminated,
            ChangedAt = now,
            ChangedBy = null, // System action triggered by runaway case
            Reason = "Runaway",
            Notes = $"Auto-terminated due to runaway case {message.RunawayCaseId}",
        };

        _db.Set<ContractStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Contract {ContractId} status changed from {FromStatus} to Terminated due to runaway case {RunawayCaseId}",
            contract.Id, fromStatus, message.RunawayCaseId);

        await _publisher.Publish(new ContractStatusChangedEvent
        {
            OccurredAt = now,
            TenantId = message.TenantId,
            ContractId = contract.Id,
            WorkerId = message.WorkerId,
            FromStatus = fromStatus.ToString(),
            ToStatus = ContractStatus.Terminated.ToString(),
            TerminationReason = TerminationReason.Runaway.ToString(),
            ClientId = message.ClientId,
        }, ct);
    }
}
