using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Document.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Document.Core.Jobs;

/// <summary>
/// Daily Hangfire job that checks for expiring and expired documents,
/// updates statuses, and publishes domain events for audit trail.
/// </summary>
public class DocumentExpiryJob
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IClock _clock;
    private readonly ILogger<DocumentExpiryJob> _logger;

    private const int ExpiringSoonDays = 30;

    public DocumentExpiryJob(
        AppDbContext db,
        IPublishEndpoint publisher,
        IClock clock,
        ILogger<DocumentExpiryJob> logger)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.DateTime);
        var threshold = today.AddDays(ExpiringSoonDays);
        var now = _clock.UtcNow;

        _logger.LogInformation("Running document expiry check for {Date}", today);

        // 1. Mark expired documents (status is still Valid but ExpiresAt < today)
        var expiredDocs = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted
                && x.Status == DocumentStatus.Valid
                && x.ExpiresAt.HasValue
                && x.ExpiresAt.Value < today)
            .ToListAsync(ct);

        foreach (var doc in expiredDocs)
        {
            doc.Status = DocumentStatus.Expired;

            await _publisher.Publish(new DocumentExpiredEvent
            {
                TenantId = doc.TenantId,
                WorkerDocumentId = doc.Id,
                WorkerId = doc.WorkerId,
                DocumentType = doc.DocumentType.ToString(),
                OccurredAt = now,
            }, ct);
        }

        if (expiredDocs.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Marked {Count} documents as expired", expiredDocs.Count);
        }

        // 2. Publish expiring-soon events (status Valid, ExpiresAt within threshold)
        var expiringDocs = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                && x.Status == DocumentStatus.Valid
                && x.ExpiresAt.HasValue
                && x.ExpiresAt.Value >= today
                && x.ExpiresAt.Value <= threshold)
            .Select(x => new { x.Id, x.TenantId, x.WorkerId, x.DocumentType, x.ExpiresAt })
            .ToListAsync(ct);

        foreach (var doc in expiringDocs)
        {
            var daysRemaining = (doc.ExpiresAt!.Value.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;

            await _publisher.Publish(new DocumentExpiringEvent
            {
                TenantId = doc.TenantId,
                WorkerDocumentId = doc.Id,
                WorkerId = doc.WorkerId,
                DocumentType = doc.DocumentType.ToString(),
                ExpiresAt = doc.ExpiresAt.Value,
                DaysRemaining = daysRemaining,
                OccurredAt = now,
            }, ct);
        }

        _logger.LogInformation(
            "Document expiry check complete: {Expired} expired, {Expiring} expiring soon",
            expiredDocs.Count, expiringDocs.Count);
    }
}
