using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Document.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;

namespace Document.Core.Consumers;

/// <summary>
/// Projection record for raw SQL reads from workers table (Worker module).
/// </summary>
file sealed record WorkerRef
{
    public Guid Id { get; init; }
}

/// <summary>
/// Consumes CandidateApprovedEvent to auto-create a Passport document
/// from the candidate snapshot data (if passport info is present).
/// </summary>
public class CandidateApprovedDocumentConsumer : IConsumer<CandidateApprovedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<CandidateApprovedDocumentConsumer> _logger;

    public CandidateApprovedDocumentConsumer(
        AppDbContext db,
        ILogger<CandidateApprovedDocumentConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CandidateApprovedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        var data = message.CandidateData;

        // Only create passport doc if candidate has passport info
        if (string.IsNullOrEmpty(data.PassportNumber))
        {
            _logger.LogInformation(
                "No passport number in candidate {CandidateId}, skipping document creation",
                message.CandidateId);
            return;
        }

        // Find the worker by passport number via raw SQL (cross-module read)
        var workerRef = await _db.Database.SqlQueryRaw<WorkerRef>(
            @"SELECT id AS ""Id"" FROM workers
              WHERE tenant_id = {0} AND passport_number = {1} AND is_deleted = false
              LIMIT 1",
            message.TenantId, data.PassportNumber).FirstOrDefaultAsync(ct);

        if (workerRef is null)
        {
            _logger.LogWarning(
                "Worker not found for candidate {CandidateId} in tenant {TenantId}, skipping document creation",
                message.CandidateId, message.TenantId);
            return;
        }

        // Idempotency: skip if passport doc already exists
        var exists = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == message.TenantId
                && x.WorkerId == workerRef.Id
                && x.DocumentType == DocumentType.Passport
                && x.DocumentNumber == data.PassportNumber
                && !x.IsDeleted, ct);

        if (exists)
        {
            _logger.LogInformation(
                "Passport document already exists for worker {WorkerId}, skipping",
                workerRef.Id);
            return;
        }

        var doc = new WorkerDocument
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            WorkerId = workerRef.Id,
            DocumentType = DocumentType.Passport,
            DocumentNumber = data.PassportNumber,
            ExpiresAt = data.PassportExpiry,
            Status = data.PassportExpiry.HasValue && data.PassportExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow)
                ? DocumentStatus.Expired
                : DocumentStatus.Valid,
            FileUrl = data.PassportDocumentUrl,
            Notes = "Auto-created from candidate approval",
        };

        _db.Set<WorkerDocument>().Add(doc);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created passport document {DocId} for worker {WorkerId} from candidate {CandidateId}",
            doc.Id, workerRef.Id, message.CandidateId);
    }
}
