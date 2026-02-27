using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Worker.Core.Consumers;

/// <summary>
/// Consumes ContractStatusChangedEvent to sync worker inventory status
/// when a contract transitions between states.
/// </summary>
public class ContractStatusChangedConsumer : IConsumer<ContractStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<ContractStatusChangedConsumer> _logger;

    public ContractStatusChangedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<ContractStatusChangedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContractStatusChangedEvent> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Received ContractStatusChangedEvent: contract {ContractId}, worker {WorkerId}, {From} → {To}",
            msg.ContractId, msg.WorkerId, msg.FromStatus, msg.ToStatus);

        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == msg.WorkerId && x.TenantId == msg.TenantId && !x.IsDeleted, ct);

        if (worker is null)
        {
            _logger.LogWarning("Worker {WorkerId} not found in tenant {TenantId}, skipping", msg.WorkerId, msg.TenantId);
            return;
        }

        var targetWorkerStatus = ResolveTargetStatus(msg.FromStatus, msg.ToStatus, worker.Status);

        if (targetWorkerStatus is null)
        {
            _logger.LogDebug("No worker status change needed for {From} → {To}", msg.FromStatus, msg.ToStatus);
            return;
        }

        var now = _clock.UtcNow;
        var fromWorkerStatus = worker.Status;

        worker.Status = targetWorkerStatus.Value;
        worker.StatusChangedAt = now;
        worker.StatusReason = msg.Reason;

        if (targetWorkerStatus == WorkerStatus.Active && worker.ActivatedAt is null)
            worker.ActivatedAt = now;

        if (targetWorkerStatus is WorkerStatus.PendingReplacement or WorkerStatus.Terminated)
        {
            worker.TerminatedAt = now;
            worker.TerminationReason = msg.Reason;
        }

        var history = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = msg.TenantId,
            WorkerId = worker.Id,
            FromStatus = fromWorkerStatus,
            ToStatus = targetWorkerStatus.Value,
            ChangedAt = now,
            ChangedBy = Guid.TryParse(msg.ChangedByUserId, out var userId) ? userId : null,
            Reason = $"Contract status changed: {msg.FromStatus} → {msg.ToStatus}",
            Notes = msg.Reason,
        };

        _db.Set<WorkerStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Worker {WorkerId} status changed: {From} → {To} (triggered by contract {ContractId})",
            worker.Id, fromWorkerStatus, targetWorkerStatus, msg.ContractId);
    }

    private static WorkerStatus? ResolveTargetStatus(string fromContract, string toContract, WorkerStatus currentWorkerStatus)
    {
        return (fromContract, toContract) switch
        {
            ("Draft", "Confirmed") => WorkerStatus.Booked,
            ("Confirmed", "OnProbation") => WorkerStatus.OnProbation,
            ("Confirmed", "Active") => WorkerStatus.Active,
            ("OnProbation", "Active") => WorkerStatus.Active,
            (_, "Cancelled") when currentWorkerStatus is WorkerStatus.Booked or WorkerStatus.OnProbation => WorkerStatus.Available,
            (_, "Terminated") => WorkerStatus.PendingReplacement,
            ("Completed", "Closed") => WorkerStatus.Available,
            ("Terminated", "Closed") => WorkerStatus.Available,
            _ => null,
        };
    }
}
