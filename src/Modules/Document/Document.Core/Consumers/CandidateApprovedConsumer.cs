using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Document.Core.Entities;
using Worker.Contracts;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;

namespace Document.Core.Consumers;

/// <summary>
/// Consumes CandidateApprovedEvent to auto-create a Passport document
/// from the candidate snapshot data (if passport info is present).
/// </summary>
public class CandidateApprovedDocumentConsumer : IConsumer<CandidateApprovedEvent>
{
    private readonly AppDbContext _db;
    private readonly IWorkerService _workerService;
    private readonly ILogger<CandidateApprovedDocumentConsumer> _logger;

    public CandidateApprovedDocumentConsumer(
        AppDbContext db,
        IWorkerService workerService,
        ILogger<CandidateApprovedDocumentConsumer> logger)
    {
        _db = db;
        _workerService = workerService;
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

        // Find the worker by searching for the passport number
        var workers = await _workerService.ListAsync(message.TenantId,
            new QueryParameters { Search = data.PassportNumber, PageSize = 1 }, ct);

        var worker = workers.Items.FirstOrDefault();
        if (worker is null)
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
                && x.WorkerId == worker.Id
                && x.DocumentType == DocumentType.Passport
                && x.DocumentNumber == data.PassportNumber
                && !x.IsDeleted, ct);

        if (exists)
        {
            _logger.LogInformation(
                "Passport document already exists for worker {WorkerId}, skipping",
                worker.Id);
            return;
        }

        var doc = new WorkerDocument
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            WorkerId = worker.Id,
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
            doc.Id, worker.Id, message.CandidateId);
    }
}
