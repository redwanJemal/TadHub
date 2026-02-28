using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Document.Contracts;
using Document.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Models;

namespace Document.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<DocumentService> _logger;

    private const int ExpiringSoonThresholdDays = 30;

    private static readonly Dictionary<string, Expression<Func<WorkerDocument, object>>> FilterableFields = new()
    {
        ["documentType"] = x => x.DocumentType,
        ["status"] = x => x.Status,
        ["workerId"] = x => x.WorkerId,
    };

    private static readonly Dictionary<string, Expression<Func<WorkerDocument, object>>> SortableFields = new()
    {
        ["documentType"] = x => x.DocumentType,
        ["status"] = x => x.Status,
        ["expiresAt"] = x => x.ExpiresAt!,
        ["issuedAt"] = x => x.IssuedAt!,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public DocumentService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<DocumentService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PagedList<WorkerDocumentListDto>> ListByWorkerAsync(
        Guid tenantId, Guid workerId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = BaseQuery(tenantId)
            .Where(x => x.WorkerId == workerId)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                (x.DocumentNumber != null && x.DocumentNumber.ToLower().Contains(searchLower)) ||
                (x.IssuingAuthority != null && x.IssuingAuthority.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<WorkerDocumentListDto>> ListAllAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = BaseQuery(tenantId)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                (x.DocumentNumber != null && x.DocumentNumber.ToLower().Contains(searchLower)) ||
                (x.IssuingAuthority != null && x.IssuingAuthority.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<WorkerDocumentDto>> GetByIdAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var doc = await BaseQuery(tenantId)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (doc is null)
            return Result<WorkerDocumentDto>.NotFound($"Document with ID {id} not found");

        return Result<WorkerDocumentDto>.Success(MapToDto(doc));
    }

    public async Task<Result<WorkerDocumentDto>> CreateAsync(
        Guid tenantId, CreateWorkerDocumentRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<DocumentType>(request.DocumentType, ignoreCase: true, out var docType))
            return Result<WorkerDocumentDto>.ValidationError($"Invalid document type '{request.DocumentType}'");

        var status = DocumentStatus.Pending;
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (!Enum.TryParse<DocumentStatus>(request.Status, ignoreCase: true, out status))
                return Result<WorkerDocumentDto>.ValidationError($"Invalid status '{request.Status}'");
        }

        var doc = new WorkerDocument
        {
            WorkerId = request.WorkerId,
            DocumentType = docType,
            DocumentNumber = request.DocumentNumber,
            IssuedAt = request.IssuedAt,
            ExpiresAt = request.ExpiresAt,
            Status = status,
            IssuingAuthority = request.IssuingAuthority,
            Notes = request.Notes,
        };

        _db.Set<WorkerDocument>().Add(doc);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created document {DocumentId} type {DocumentType} for worker {WorkerId}",
            doc.Id, doc.DocumentType, doc.WorkerId);

        await _publisher.Publish(new DocumentCreatedEvent
        {
            OccurredAt = _clock.UtcNow,
            TenantId = tenantId,
            WorkerDocumentId = doc.Id,
            WorkerId = doc.WorkerId,
            DocumentType = doc.DocumentType.ToString(),
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result<WorkerDocumentDto>.Success(MapToDto(doc));
    }

    public async Task<Result<WorkerDocumentDto>> UpdateAsync(
        Guid tenantId, Guid id, UpdateWorkerDocumentRequest request, CancellationToken ct = default)
    {
        var doc = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (doc is null)
            return Result<WorkerDocumentDto>.NotFound($"Document with ID {id} not found");

        if (request.DocumentNumber is not null)
            doc.DocumentNumber = request.DocumentNumber;
        if (request.IssuedAt.HasValue)
            doc.IssuedAt = request.IssuedAt;
        if (request.ExpiresAt.HasValue)
            doc.ExpiresAt = request.ExpiresAt;
        if (request.IssuingAuthority is not null)
            doc.IssuingAuthority = request.IssuingAuthority;
        if (request.Notes is not null)
            doc.Notes = request.Notes;
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (!Enum.TryParse<DocumentStatus>(request.Status, ignoreCase: true, out var status))
                return Result<WorkerDocumentDto>.ValidationError($"Invalid status '{request.Status}'");
            doc.Status = status;
        }

        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new DocumentUpdatedEvent
        {
            OccurredAt = _clock.UtcNow,
            TenantId = tenantId,
            WorkerDocumentId = doc.Id,
            WorkerId = doc.WorkerId,
            DocumentType = doc.DocumentType.ToString(),
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result<WorkerDocumentDto>.Success(MapToDto(doc));
    }

    public async Task<Result> DeleteAsync(
        Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var doc = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (doc is null)
            return Result.NotFound($"Document with ID {id} not found");

        doc.IsDeleted = true;
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new DocumentDeletedEvent
        {
            OccurredAt = _clock.UtcNow,
            TenantId = tenantId,
            WorkerDocumentId = doc.Id,
            WorkerId = doc.WorkerId,
            DocumentType = doc.DocumentType.ToString(),
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result.Success();
    }

    public async Task<Result> SetFileUrlAsync(
        Guid tenantId, Guid id, string fileUrl, CancellationToken ct = default)
    {
        var doc = await _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (doc is null)
            return Result.NotFound($"Document with ID {id} not found");

        doc.FileUrl = fileUrl;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<PagedList<WorkerDocumentListDto>> GetExpiringDocumentsAsync(
        Guid tenantId, int withinDays, QueryParameters qp, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.DateTime);
        var threshold = today.AddDays(withinDays);

        var query = BaseQuery(tenantId)
            .Where(x => x.ExpiresAt.HasValue
                && x.ExpiresAt.Value <= threshold
                && x.Status != DocumentStatus.Revoked)
            .ApplySort(qp.GetSortFields(), SortableFields);

        // Default sort by expiry ascending (soonest first) if no sort specified
        if (qp.GetSortFields().Count == 0)
        {
            query = query.OrderBy(x => x.ExpiresAt);
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<ComplianceSummaryDto> GetComplianceSummaryAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.DateTime);
        var threshold = today.AddDays(ExpiringSoonThresholdDays);

        var docs = await BaseQuery(tenantId)
            .Select(x => new { x.DocumentType, x.Status, x.ExpiresAt })
            .ToListAsync(ct);

        var byType = docs
            .GroupBy(x => x.DocumentType)
            .Select(g => new ComplianceByTypeDto
            {
                DocumentType = g.Key.ToString(),
                Valid = g.Count(d => d.Status == DocumentStatus.Valid && (!d.ExpiresAt.HasValue || d.ExpiresAt.Value > threshold)),
                ExpiringSoon = g.Count(d => d.Status == DocumentStatus.Valid && d.ExpiresAt.HasValue && d.ExpiresAt.Value <= threshold && d.ExpiresAt.Value >= today),
                Expired = g.Count(d => d.Status == DocumentStatus.Expired || (d.Status == DocumentStatus.Valid && d.ExpiresAt.HasValue && d.ExpiresAt.Value < today)),
                Pending = g.Count(d => d.Status == DocumentStatus.Pending),
            })
            .OrderBy(x => x.DocumentType)
            .ToList();

        return new ComplianceSummaryDto
        {
            TotalDocuments = docs.Count,
            Valid = byType.Sum(x => x.Valid),
            ExpiringSoon = byType.Sum(x => x.ExpiringSoon),
            Expired = byType.Sum(x => x.Expired),
            Pending = byType.Sum(x => x.Pending),
            ByType = byType,
        };
    }

    // ──── Helpers ────

    private IQueryable<WorkerDocument> BaseQuery(Guid tenantId) =>
        _db.Set<WorkerDocument>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

    private static string ComputeEffectiveStatus(WorkerDocument doc)
    {
        if (doc.Status == DocumentStatus.Revoked) return "Revoked";
        if (doc.Status == DocumentStatus.Pending) return "Pending";
        if (doc.IsExpired) return "Expired";
        if (doc.IsExpiringSoon()) return "ExpiringSoon";
        return doc.Status.ToString();
    }

    private static WorkerDocumentDto MapToDto(WorkerDocument doc) => new()
    {
        Id = doc.Id,
        TenantId = doc.TenantId,
        WorkerId = doc.WorkerId,
        DocumentType = doc.DocumentType.ToString(),
        DocumentNumber = doc.DocumentNumber,
        IssuedAt = doc.IssuedAt,
        ExpiresAt = doc.ExpiresAt,
        Status = doc.Status.ToString(),
        EffectiveStatus = ComputeEffectiveStatus(doc),
        DaysUntilExpiry = doc.DaysUntilExpiry,
        IssuingAuthority = doc.IssuingAuthority,
        Notes = doc.Notes,
        FileUrl = doc.FileUrl,
        CreatedBy = doc.CreatedBy,
        UpdatedBy = doc.UpdatedBy,
        CreatedAt = doc.CreatedAt,
        UpdatedAt = doc.UpdatedAt,
    };

    private static WorkerDocumentListDto MapToListDto(WorkerDocument doc) => new()
    {
        Id = doc.Id,
        WorkerId = doc.WorkerId,
        DocumentType = doc.DocumentType.ToString(),
        DocumentNumber = doc.DocumentNumber,
        IssuedAt = doc.IssuedAt,
        ExpiresAt = doc.ExpiresAt,
        Status = doc.Status.ToString(),
        EffectiveStatus = ComputeEffectiveStatus(doc),
        DaysUntilExpiry = doc.DaysUntilExpiry,
        HasFile = !string.IsNullOrEmpty(doc.FileUrl),
        CreatedAt = doc.CreatedAt,
    };
}
