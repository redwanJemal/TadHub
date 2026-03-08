using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Visa.Contracts;
using Visa.Contracts.DTOs;
using Visa.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Visa.Core.Services;

public class VisaApplicationService : IVisaApplicationService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<VisaApplicationService> _logger;

    private static readonly Dictionary<string, Expression<Func<VisaApplication, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["visaType"] = x => x.VisaType,
        ["workerId"] = x => x.WorkerId,
        ["clientId"] = x => x.ClientId,
        ["placementId"] = x => x.PlacementId!,
    };

    private static readonly Dictionary<string, Expression<Func<VisaApplication, object>>> SortableFields = new()
    {
        ["applicationCode"] = x => x.ApplicationCode,
        ["status"] = x => x.Status,
        ["visaType"] = x => x.VisaType,
        ["applicationDate"] = x => x.ApplicationDate!,
        ["statusChangedAt"] = x => x.StatusChangedAt,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public VisaApplicationService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<VisaApplicationService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PagedList<VisaApplicationListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.ApplicationCode.ToLower().Contains(searchLower) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.ToLower().Contains(searchLower)) ||
                (x.VisaNumber != null && x.VisaNumber.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => new VisaApplicationListDto
            {
                Id = x.Id,
                ApplicationCode = x.ApplicationCode,
                VisaType = x.VisaType.ToString(),
                Status = x.Status.ToString(),
                StatusChangedAt = x.StatusChangedAt,
                WorkerId = x.WorkerId,
                ClientId = x.ClientId,
                PlacementId = x.PlacementId,
                ApplicationDate = x.ApplicationDate,
                ExpiryDate = x.ExpiryDate,
                ReferenceNumber = x.ReferenceNumber,
                DocumentCount = x.Documents.Count,
                CreatedAt = x.CreatedAt,
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<VisaApplicationDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);
        var includeDocuments = includes.Contains("documents", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));

        if (includeDocuments)
            query = query.Include(x => x.Documents);

        var visa = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (visa is null)
            return Result<VisaApplicationDto>.NotFound($"Visa application with ID {id} not found");

        return Result<VisaApplicationDto>.Success(MapToDto(visa, includeStatusHistory, includeDocuments));
    }

    public async Task<Result<VisaApplicationDto>> CreateAsync(Guid tenantId, CreateVisaApplicationRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<VisaType>(request.VisaType, ignoreCase: true, out var visaType))
            return Result<VisaApplicationDto>.ValidationError($"Invalid visa type '{request.VisaType}'");

        // Enforce sequencing: ResidenceVisa requires completed EmploymentVisa
        if (visaType == Entities.VisaType.ResidenceVisa)
        {
            var hasApprovedEmploymentVisa = await _db.Set<VisaApplication>()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId
                    && !x.IsDeleted
                    && x.WorkerId == request.WorkerId
                    && x.VisaType == Entities.VisaType.EmploymentVisa
                    && (x.Status == VisaApplicationStatus.Approved || x.Status == VisaApplicationStatus.Issued), ct);

            if (!hasApprovedEmploymentVisa)
                return Result<VisaApplicationDto>.ValidationError("Employment Visa must be approved or issued before creating a Residence Visa");
        }

        // Check for duplicate active visa of same type for same worker
        var hasActiveVisa = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.WorkerId == request.WorkerId
                && x.VisaType == visaType
                && x.Status != VisaApplicationStatus.Cancelled
                && x.Status != VisaApplicationStatus.Expired
                && x.Status != VisaApplicationStatus.Rejected, ct);

        if (hasActiveVisa)
            return Result<VisaApplicationDto>.Conflict($"Worker already has an active {request.VisaType} application");

        // Generate application code
        var lastCode = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ApplicationCode)
            .Select(x => x.ApplicationCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("VSA-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var applicationCode = $"VSA-{nextNumber:D6}";

        var now = _clock.UtcNow;
        var visa = new VisaApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationCode = applicationCode,
            VisaType = visaType,
            Status = VisaApplicationStatus.NotStarted,
            StatusChangedAt = now,
            WorkerId = request.WorkerId,
            ClientId = request.ClientId,
            ContractId = request.ContractId,
            PlacementId = request.PlacementId,
            ApplicationDate = request.ApplicationDate,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        var history = new VisaApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VisaApplicationId = visa.Id,
            FromStatus = null,
            ToStatus = VisaApplicationStatus.NotStarted,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = "Visa application created",
        };

        _db.Set<VisaApplication>().Add(visa);
        _db.Set<VisaApplicationStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created visa application {ApplicationCode} ({VisaType}) for worker {WorkerId}",
            applicationCode, visaType, request.WorkerId);

        await _publisher.Publish(new VisaApplicationCreatedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            VisaApplicationId = visa.Id,
            WorkerId = request.WorkerId,
            ClientId = request.ClientId,
            VisaType = visaType.ToString(),
            PlacementId = request.PlacementId,
            CreatedBy = _currentUser.UserId,
        }, ct);

        return Result<VisaApplicationDto>.Success(MapToDto(visa, includeStatusHistory: false, includeDocuments: false));
    }

    public async Task<Result<VisaApplicationDto>> UpdateAsync(Guid tenantId, Guid id, UpdateVisaApplicationRequest request, CancellationToken ct = default)
    {
        var visa = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (visa is null)
            return Result<VisaApplicationDto>.NotFound($"Visa application with ID {id} not found");

        if (request.ApplicationDate.HasValue) visa.ApplicationDate = request.ApplicationDate.Value;
        if (request.ApprovalDate.HasValue) visa.ApprovalDate = request.ApprovalDate.Value;
        if (request.IssuanceDate.HasValue) visa.IssuanceDate = request.IssuanceDate.Value;
        if (request.ExpiryDate.HasValue) visa.ExpiryDate = request.ExpiryDate.Value;
        if (request.ReferenceNumber is not null) visa.ReferenceNumber = request.ReferenceNumber;
        if (request.VisaNumber is not null) visa.VisaNumber = request.VisaNumber;
        if (request.Notes is not null) visa.Notes = request.Notes;
        if (request.ContractId.HasValue) visa.ContractId = request.ContractId.Value;
        if (request.PlacementId.HasValue) visa.PlacementId = request.PlacementId.Value;

        visa.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated visa application {VisaApplicationId}", id);

        return Result<VisaApplicationDto>.Success(MapToDto(visa, includeStatusHistory: false, includeDocuments: false));
    }

    public async Task<Result<VisaApplicationDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionVisaStatusRequest request, CancellationToken ct = default)
    {
        var visa = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (visa is null)
            return Result<VisaApplicationDto>.NotFound($"Visa application with ID {id} not found");

        if (!Enum.TryParse<VisaApplicationStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<VisaApplicationDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = VisaApplicationStatusMachine.Validate(visa.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<VisaApplicationDto>.ValidationError(error);

        var now = _clock.UtcNow;
        var fromStatus = visa.Status;

        visa.Status = targetStatus;
        visa.StatusChangedAt = now;
        visa.StatusReason = request.Reason;
        visa.UpdatedBy = _currentUser.UserId;

        // Set specific fields based on status
        switch (targetStatus)
        {
            case VisaApplicationStatus.Rejected:
                visa.RejectionReason = request.Reason;
                break;
        }

        var history = new VisaApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VisaApplicationId = visa.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Reason = request.Reason,
            Notes = request.Notes,
        };

        _db.Set<VisaApplicationStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned visa application {VisaApplicationId} from {FromStatus} to {ToStatus}",
            id, fromStatus, targetStatus);

        await _publisher.Publish(new VisaStatusChangedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            VisaApplicationId = visa.Id,
            WorkerId = visa.WorkerId,
            VisaType = visa.VisaType.ToString(),
            FromStatus = fromStatus.ToString(),
            ToStatus = targetStatus.ToString(),
            Reason = request.Reason,
            PlacementId = visa.PlacementId,
        }, ct);

        // Publish issued event if transitioning to Issued
        if (targetStatus == VisaApplicationStatus.Issued)
        {
            await _publisher.Publish(new VisaIssuedEvent
            {
                OccurredAt = now,
                TenantId = tenantId,
                VisaApplicationId = visa.Id,
                WorkerId = visa.WorkerId,
                VisaType = visa.VisaType.ToString(),
                VisaNumber = visa.VisaNumber,
                ExpiryDate = visa.ExpiryDate,
                PlacementId = visa.PlacementId,
            }, ct);
        }

        return Result<VisaApplicationDto>.Success(MapToDto(visa, includeStatusHistory: false, includeDocuments: false));
    }

    public async Task<Result<List<VisaApplicationStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!exists)
            return Result<List<VisaApplicationStatusHistoryDto>>.NotFound($"Visa application with ID {id} not found");

        var history = await _db.Set<VisaApplicationStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.VisaApplicationId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<VisaApplicationStatusHistoryDto>>.Success(history);
    }

    public async Task<Result<VisaApplicationDocumentDto>> UploadDocumentAsync(Guid tenantId, Guid id, UploadVisaDocumentRequest request, CancellationToken ct = default)
    {
        var visa = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (visa is null)
            return Result<VisaApplicationDocumentDto>.NotFound($"Visa application with ID {id} not found");

        if (!Enum.TryParse<VisaDocumentType>(request.DocumentType, ignoreCase: true, out var docType))
            return Result<VisaApplicationDocumentDto>.ValidationError($"Invalid document type '{request.DocumentType}'");

        var now = _clock.UtcNow;
        var document = new VisaApplicationDocument
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VisaApplicationId = visa.Id,
            DocumentType = docType,
            FileUrl = request.FileUrl,
            UploadedAt = now,
            IsVerified = false,
        };

        _db.Set<VisaApplicationDocument>().Add(document);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Uploaded document {DocumentType} to visa application {VisaApplicationId}",
            docType, id);

        return Result<VisaApplicationDocumentDto>.Success(MapToDocumentDto(document));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var visa = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (visa is null)
            return Result.NotFound($"Visa application with ID {id} not found");

        visa.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted visa application {VisaApplicationId}", id);

        return Result.Success();
    }

    public async Task<Result<List<VisaApplicationListDto>>> GetByWorkerAsync(Guid tenantId, Guid workerId, CancellationToken ct = default)
    {
        var visas = await _db.Set<VisaApplication>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Documents)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.WorkerId == workerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new VisaApplicationListDto
            {
                Id = x.Id,
                ApplicationCode = x.ApplicationCode,
                VisaType = x.VisaType.ToString(),
                Status = x.Status.ToString(),
                StatusChangedAt = x.StatusChangedAt,
                WorkerId = x.WorkerId,
                ClientId = x.ClientId,
                PlacementId = x.PlacementId,
                ApplicationDate = x.ApplicationDate,
                ExpiryDate = x.ExpiryDate,
                ReferenceNumber = x.ReferenceNumber,
                DocumentCount = x.Documents.Count,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(ct);

        return Result<List<VisaApplicationListDto>>.Success(visas);
    }

    #region Mapping

    private static VisaApplicationDto MapToDto(VisaApplication v, bool includeStatusHistory, bool includeDocuments)
    {
        return new VisaApplicationDto
        {
            Id = v.Id,
            TenantId = v.TenantId,
            ApplicationCode = v.ApplicationCode,
            VisaType = v.VisaType.ToString(),
            Status = v.Status.ToString(),
            StatusChangedAt = v.StatusChangedAt,
            StatusReason = v.StatusReason,
            WorkerId = v.WorkerId,
            ClientId = v.ClientId,
            ContractId = v.ContractId,
            PlacementId = v.PlacementId,
            ApplicationDate = v.ApplicationDate,
            ApprovalDate = v.ApprovalDate,
            IssuanceDate = v.IssuanceDate,
            ExpiryDate = v.ExpiryDate,
            ReferenceNumber = v.ReferenceNumber,
            VisaNumber = v.VisaNumber,
            Notes = v.Notes,
            RejectionReason = v.RejectionReason,
            CreatedBy = v.CreatedBy,
            UpdatedBy = v.UpdatedBy,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            StatusHistory = includeStatusHistory
                ? v.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
            Documents = includeDocuments
                ? (v.Documents ?? []).Select(MapToDocumentDto).ToList()
                : null,
        };
    }

    private static VisaApplicationStatusHistoryDto MapToHistoryDto(VisaApplicationStatusHistory h)
    {
        return new VisaApplicationStatusHistoryDto
        {
            Id = h.Id,
            VisaApplicationId = h.VisaApplicationId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    private static VisaApplicationDocumentDto MapToDocumentDto(VisaApplicationDocument d)
    {
        return new VisaApplicationDocumentDto
        {
            Id = d.Id,
            VisaApplicationId = d.VisaApplicationId,
            DocumentType = d.DocumentType.ToString(),
            FileUrl = d.FileUrl,
            UploadedAt = d.UploadedAt,
            IsVerified = d.IsVerified,
        };
    }

    #endregion
}
