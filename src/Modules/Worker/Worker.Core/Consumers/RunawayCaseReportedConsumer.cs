using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Worker.Core.Consumers;

/// <summary>
/// Consumes RunawayCaseReportedEvent to set the worker's status to Absconded
/// via EF Core change tracking (so audit interceptors fire correctly).
/// </summary>
public class RunawayCaseReportedConsumer : IConsumer<RunawayCaseReportedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<RunawayCaseReportedConsumer> _logger;

    public RunawayCaseReportedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<RunawayCaseReportedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RunawayCaseReportedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Received RunawayCaseReportedEvent for worker {WorkerId} in tenant {TenantId}",
            message.WorkerId, message.TenantId);

        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == message.WorkerId && x.TenantId == message.TenantId && !x.IsDeleted, ct);

        if (worker is null)
        {
            _logger.LogWarning("Worker {WorkerId} not found in tenant {TenantId}, skipping", message.WorkerId, message.TenantId);
            return;
        }

        if (worker.Status == WorkerStatus.Absconded)
        {
            _logger.LogWarning("Worker {WorkerId} is already Absconded, skipping", message.WorkerId);
            return;
        }

        var now = _clock.UtcNow;
        var fromStatus = worker.Status;

        worker.Status = WorkerStatus.Absconded;
        worker.StatusChangedAt = now;

        var history = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            WorkerId = worker.Id,
            FromStatus = fromStatus,
            ToStatus = WorkerStatus.Absconded,
            ChangedAt = now,
            ChangedBy = null, // System action triggered by runaway case
            Notes = $"Auto-set to Absconded from runaway case {message.RunawayCaseId}",
        };

        _db.Set<WorkerStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Worker {WorkerId} status changed from {FromStatus} to Absconded due to runaway case {RunawayCaseId}",
            worker.Id, fromStatus, message.RunawayCaseId);
    }
}
