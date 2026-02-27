using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Candidate.Contracts;
using Candidate.Contracts.DTOs;
using Candidate.Core.Entities;
using Supplier.Contracts;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Candidate.Core.Services;

/// <summary>
/// Service for managing candidates within a tenant.
/// </summary>
public class CandidateService : ICandidateService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ISupplierService _supplierService;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CandidateService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Candidate, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["sourceType"] = x => x.SourceType,
        ["nationality"] = x => x.Nationality,
        ["tenantSupplierId"] = x => x.TenantSupplierId!,
        ["gender"] = x => x.Gender!,
        ["createdAt"] = x => x.CreatedAt,
        ["createdBy"] = x => x.CreatedBy!,
        ["passportNumber"] = x => x.PassportNumber!,
        ["externalReference"] = x => x.ExternalReference!,
        ["jobCategoryId"] = x => x.JobCategoryId!,
        ["religion"] = x => x.Religion!,
        ["maritalStatus"] = x => x.MaritalStatus!,
        ["educationLevel"] = x => x.EducationLevel!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Candidate, object>>> SortableFields = new()
    {
        ["fullNameEn"] = x => x.FullNameEn,
        ["nationality"] = x => x.Nationality,
        ["status"] = x => x.Status,
        ["sourceType"] = x => x.SourceType,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["statusChangedAt"] = x => x.StatusChangedAt!,
    };

    public CandidateService(
        AppDbContext db,
        IPublishEndpoint publisher,
        ISupplierService supplierService,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<CandidateService> logger)
    {
        _db = db;
        _publisher = publisher;
        _supplierService = supplierService;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<CandidateListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        // Exclude Approved candidates by default unless status filter is explicitly provided
        var hasStatusFilter = qp.Filters.Any(f => f.Name == "status");
        if (!hasStatusFilter)
        {
            query = query.Where(x => x.Status != CandidateStatus.Approved);
        }

        query = query
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.FullNameEn.ToLower().Contains(searchLower) ||
                (x.FullNameAr != null && x.FullNameAr.ToLower().Contains(searchLower)) ||
                (x.PassportNumber != null && x.PassportNumber.ToLower().Contains(searchLower)) ||
                (x.ExternalReference != null && x.ExternalReference.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<CandidateDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
        {
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        query = query.Include(x => x.Skills).Include(x => x.Languages);

        var candidate = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (candidate is null)
            return Result<CandidateDto>.NotFound($"Candidate with ID {id} not found");

        return Result<CandidateDto>.Success(MapToDto(candidate, includeStatusHistory));
    }

    public async Task<Result<CandidateDto>> CreateAsync(Guid tenantId, CreateCandidateRequest request, CancellationToken ct = default)
    {
        // Parse and validate source type
        if (!Enum.TryParse<CandidateSourceType>(request.SourceType, ignoreCase: true, out var sourceType))
            return Result<CandidateDto>.ValidationError($"Invalid source type '{request.SourceType}'. Valid values: Supplier, Local");

        // Validate supplier relationship
        if (sourceType == CandidateSourceType.Supplier)
        {
            if (request.TenantSupplierId is null)
                return Result<CandidateDto>.ValidationError("TenantSupplierId is required for supplier-sourced candidates");

            var supplierResult = await _supplierService.GetTenantSupplierByIdAsync(tenantId, request.TenantSupplierId.Value, ct: ct);

            if (!supplierResult.IsSuccess)
                return Result<CandidateDto>.NotFound($"TenantSupplier with ID {request.TenantSupplierId} not found in this tenant");
        }
        else
        {
            if (request.TenantSupplierId is not null)
                return Result<CandidateDto>.ValidationError("TenantSupplierId must be null for local candidates");
        }

        // Check duplicate passport number within tenant
        if (!string.IsNullOrWhiteSpace(request.PassportNumber))
        {
            var existsByPassport = await _db.Set<Entities.Candidate>()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.PassportNumber == request.PassportNumber, ct);

            if (existsByPassport)
                return Result<CandidateDto>.Conflict($"Candidate with passport number '{request.PassportNumber}' already exists in this tenant");
        }

        var now = _clock.UtcNow;
        var candidate = new Entities.Candidate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FullNameEn = request.FullNameEn,
            FullNameAr = request.FullNameAr,
            Nationality = request.Nationality,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PassportNumber = request.PassportNumber,
            Phone = request.Phone,
            Email = request.Email,
            SourceType = sourceType,
            TenantSupplierId = request.TenantSupplierId,
            Status = CandidateStatus.Received,
            StatusChangedAt = now,
            Religion = request.Religion,
            MaritalStatus = request.MaritalStatus,
            EducationLevel = request.EducationLevel,
            JobCategoryId = request.JobCategoryId,
            ExperienceYears = request.ExperienceYears,
            MonthlySalary = request.MonthlySalary,
            PassportExpiry = request.PassportExpiry,
            MedicalStatus = request.MedicalStatus,
            VisaStatus = request.VisaStatus,
            ExpectedArrivalDate = request.ExpectedArrivalDate,
            Notes = request.Notes,
            ExternalReference = request.ExternalReference,
        };

        // Create child skill entities (deduplicate by name to avoid unique constraint violation)
        if (request.Skills is { Count: > 0 })
        {
            foreach (var skill in request.Skills.DistinctBy(s => s.SkillName, StringComparer.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<SkillProficiency>(skill.ProficiencyLevel, ignoreCase: true, out var proficiency))
                    proficiency = SkillProficiency.Basic;

                candidate.Skills.Add(new CandidateSkill
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CandidateId = candidate.Id,
                    SkillName = skill.SkillName,
                    ProficiencyLevel = proficiency,
                });
            }
        }

        // Create child language entities (deduplicate by language to avoid unique constraint violation)
        if (request.Languages is { Count: > 0 })
        {
            foreach (var lang in request.Languages.DistinctBy(l => l.Language, StringComparer.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<LanguageProficiency>(lang.ProficiencyLevel, ignoreCase: true, out var proficiency))
                    proficiency = LanguageProficiency.Basic;

                candidate.Languages.Add(new CandidateLanguage
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CandidateId = candidate.Id,
                    Language = lang.Language,
                    ProficiencyLevel = proficiency,
                });
            }
        }

        // Initial status history entry
        var history = new CandidateStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FromStatus = null,
            ToStatus = CandidateStatus.Received,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
        };

        _db.Set<Entities.Candidate>().Add(candidate);
        _db.Set<CandidateStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created candidate {CandidateId} ({FullNameEn}) for tenant {TenantId}", candidate.Id, candidate.FullNameEn, tenantId);

        await _publisher.Publish(new CandidateCreatedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FullNameEn = candidate.FullNameEn,
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result<CandidateDto>.Success(MapToDto(candidate, includeStatusHistory: false));
    }

    public async Task<Result<CandidateDto>> UpdateAsync(Guid tenantId, Guid id, UpdateCandidateRequest request, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        var candidate = await query.FirstOrDefaultAsync(ct);

        if (candidate is null)
            return Result<CandidateDto>.NotFound($"Candidate with ID {id} not found");

        // Check duplicate passport number if being changed
        if (request.PassportNumber is not null && request.PassportNumber != candidate.PassportNumber)
        {
            if (!string.IsNullOrWhiteSpace(request.PassportNumber))
            {
                var existsByPassport = await _db.Set<Entities.Candidate>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.PassportNumber == request.PassportNumber && x.Id != id, ct);

                if (existsByPassport)
                    return Result<CandidateDto>.Conflict($"Candidate with passport number '{request.PassportNumber}' already exists in this tenant");
            }
        }

        // Apply updates (only non-null values)
        if (request.FullNameEn is not null)
            candidate.FullNameEn = request.FullNameEn;
        if (request.FullNameAr is not null)
            candidate.FullNameAr = request.FullNameAr;
        if (request.Nationality is not null)
            candidate.Nationality = request.Nationality;
        if (request.DateOfBirth.HasValue)
            candidate.DateOfBirth = request.DateOfBirth;
        if (request.Gender is not null)
            candidate.Gender = request.Gender;
        if (request.PassportNumber is not null)
            candidate.PassportNumber = request.PassportNumber;
        if (request.Phone is not null)
            candidate.Phone = request.Phone;
        if (request.Email is not null)
            candidate.Email = request.Email;
        if (request.PassportExpiry.HasValue)
            candidate.PassportExpiry = request.PassportExpiry;
        if (request.MedicalStatus is not null)
            candidate.MedicalStatus = request.MedicalStatus;
        if (request.VisaStatus is not null)
            candidate.VisaStatus = request.VisaStatus;
        if (request.ExpectedArrivalDate.HasValue)
            candidate.ExpectedArrivalDate = request.ExpectedArrivalDate;
        if (request.ActualArrivalDate.HasValue)
            candidate.ActualArrivalDate = request.ActualArrivalDate;
        if (request.Notes is not null)
            candidate.Notes = request.Notes;
        if (request.ExternalReference is not null)
            candidate.ExternalReference = request.ExternalReference;

        // Professional profile fields
        if (request.Religion is not null)
            candidate.Religion = request.Religion;
        if (request.MaritalStatus is not null)
            candidate.MaritalStatus = request.MaritalStatus;
        if (request.EducationLevel is not null)
            candidate.EducationLevel = request.EducationLevel;
        if (request.JobCategoryId.HasValue)
            candidate.JobCategoryId = request.JobCategoryId;
        if (request.ExperienceYears.HasValue)
            candidate.ExperienceYears = request.ExperienceYears;
        if (request.MonthlySalary.HasValue)
            candidate.MonthlySalary = request.MonthlySalary;
        if (request.ProcurementCost.HasValue)
            candidate.ProcurementCost = request.ProcurementCost;

        // Sourcing
        if (request.SourceType is not null)
        {
            if (Enum.TryParse<CandidateSourceType>(request.SourceType, ignoreCase: true, out var sourceType))
                candidate.SourceType = sourceType;
        }
        if (request.TenantSupplierId.HasValue)
            candidate.TenantSupplierId = request.TenantSupplierId;

        // Media
        if (request.PhotoUrl is not null)
            candidate.PhotoUrl = request.PhotoUrl;
        if (request.VideoUrl is not null)
            candidate.VideoUrl = request.VideoUrl;
        if (request.PassportDocumentUrl is not null)
            candidate.PassportDocumentUrl = request.PassportDocumentUrl;

        // Full-replacement semantics for Skills (if provided, remove old + add new; if null, leave untouched)
        // Use ExecuteDeleteAsync to avoid EF Core batch concurrency issues with mixed DELETE+INSERT
        if (request.Skills is not null)
        {
            await _db.Set<CandidateSkill>()
                .IgnoreQueryFilters()
                .Where(x => x.CandidateId == candidate.Id && x.TenantId == tenantId)
                .ExecuteDeleteAsync(ct);

            // Deduplicate by skill name (last wins) to avoid unique constraint violation
            foreach (var skill in request.Skills.DistinctBy(s => s.SkillName, StringComparer.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<SkillProficiency>(skill.ProficiencyLevel, ignoreCase: true, out var proficiency))
                    proficiency = SkillProficiency.Basic;

                _db.Set<CandidateSkill>().Add(new CandidateSkill
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CandidateId = candidate.Id,
                    SkillName = skill.SkillName,
                    ProficiencyLevel = proficiency,
                });
            }
        }

        // Full-replacement semantics for Languages
        if (request.Languages is not null)
        {
            await _db.Set<CandidateLanguage>()
                .IgnoreQueryFilters()
                .Where(x => x.CandidateId == candidate.Id && x.TenantId == tenantId)
                .ExecuteDeleteAsync(ct);

            // Deduplicate by language (last wins) to avoid unique constraint violation
            foreach (var lang in request.Languages.DistinctBy(l => l.Language, StringComparer.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<LanguageProficiency>(lang.ProficiencyLevel, ignoreCase: true, out var proficiency))
                    proficiency = LanguageProficiency.Basic;

                _db.Set<CandidateLanguage>().Add(new CandidateLanguage
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CandidateId = candidate.Id,
                    Language = lang.Language,
                    ProficiencyLevel = proficiency,
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated candidate {CandidateId}", id);

        await _publisher.Publish(new CandidateUpdatedEvent
        {
            OccurredAt = _clock.UtcNow,
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FullNameEn = candidate.FullNameEn,
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result<CandidateDto>.Success(MapToDto(candidate, includeStatusHistory: false));
    }

    public async Task<Result<CandidateDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionStatusRequest request, CancellationToken ct = default)
    {
        var candidate = await _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (candidate is null)
            return Result<CandidateDto>.NotFound($"Candidate with ID {id} not found");

        if (!Enum.TryParse<CandidateStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<CandidateDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = CandidateStatusMachine.Validate(candidate.SourceType, candidate.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<CandidateDto>.ValidationError(error);

        var now = _clock.UtcNow;
        var fromStatus = candidate.Status;

        candidate.Status = targetStatus;
        candidate.StatusChangedAt = now;
        candidate.StatusReason = request.Reason;

        var history = new CandidateStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
            Reason = request.Reason,
            Notes = request.Notes,
        };

        _db.Set<CandidateStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned candidate {CandidateId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        // Publish status change event for audit trail
        await _publisher.Publish(new CandidateStatusChangedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FullNameEn = candidate.FullNameEn,
            FromStatus = fromStatus.ToString(),
            ToStatus = targetStatus.ToString(),
            Reason = request.Reason,
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        // Publish CandidateApprovedEvent when status transitions to Approved
        if (targetStatus == CandidateStatus.Approved)
        {
            var full = await _db.Set<Entities.Candidate>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Skills)
                .Include(x => x.Languages)
                .FirstAsync(x => x.Id == id, ct);

            var snapshot = new CandidateSnapshotDto
            {
                FullNameEn = full.FullNameEn,
                FullNameAr = full.FullNameAr,
                Nationality = full.Nationality,
                DateOfBirth = full.DateOfBirth,
                Gender = full.Gender,
                PassportNumber = full.PassportNumber,
                PassportExpiry = full.PassportExpiry,
                Phone = full.Phone,
                Email = full.Email,
                Religion = full.Religion,
                MaritalStatus = full.MaritalStatus,
                EducationLevel = full.EducationLevel,
                JobCategoryId = full.JobCategoryId,
                ExperienceYears = full.ExperienceYears,
                MonthlySalary = full.MonthlySalary,
                PhotoUrl = full.PhotoUrl,
                VideoUrl = full.VideoUrl,
                PassportDocumentUrl = full.PassportDocumentUrl,
                SourceType = full.SourceType.ToString(),
                TenantSupplierId = full.TenantSupplierId,
                Skills = full.Skills.Select(s => new CandidateSnapshotSkill
                {
                    SkillName = s.SkillName,
                    ProficiencyLevel = s.ProficiencyLevel.ToString(),
                }).ToList(),
                Languages = full.Languages.Select(l => new CandidateSnapshotLanguage
                {
                    Language = l.Language,
                    ProficiencyLevel = l.ProficiencyLevel.ToString(),
                }).ToList(),
            };

            await _publisher.Publish(new CandidateApprovedEvent
            {
                TenantId = tenantId,
                CandidateId = id,
                CandidateData = snapshot,
                OccurredAt = now,
            }, ct);

            _logger.LogInformation("Published CandidateApprovedEvent for candidate {CandidateId}", id);
        }

        return Result<CandidateDto>.Success(MapToDto(candidate, includeStatusHistory: false));
    }

    public async Task<Result<List<CandidateStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var candidateExists = await _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!candidateExists)
            return Result<List<CandidateStatusHistoryDto>>.NotFound($"Candidate with ID {id} not found");

        var history = await _db.Set<CandidateStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.CandidateId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<CandidateStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var candidate = await _db.Set<Entities.Candidate>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (candidate is null)
            return Result.NotFound($"Candidate with ID {id} not found");

        candidate.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted candidate {CandidateId}", id);

        await _publisher.Publish(new CandidateDeletedEvent
        {
            OccurredAt = _clock.UtcNow,
            TenantId = tenantId,
            CandidateId = candidate.Id,
            FullNameEn = candidate.FullNameEn,
            ChangedByUserId = _currentUser.UserId.ToString(),
        }, ct);

        return Result.Success();
    }

    #region Mapping

    private static CandidateDto MapToDto(Entities.Candidate c, bool includeStatusHistory)
    {
        return new CandidateDto
        {
            Id = c.Id,
            TenantId = c.TenantId,
            FullNameEn = c.FullNameEn,
            FullNameAr = c.FullNameAr,
            Nationality = c.Nationality,
            DateOfBirth = c.DateOfBirth,
            Gender = c.Gender,
            PassportNumber = c.PassportNumber,
            Phone = c.Phone,
            Email = c.Email,
            SourceType = c.SourceType.ToString(),
            TenantSupplierId = c.TenantSupplierId,
            Status = c.Status.ToString(),
            StatusChangedAt = c.StatusChangedAt,
            StatusReason = c.StatusReason,
            Religion = c.Religion,
            MaritalStatus = c.MaritalStatus,
            EducationLevel = c.EducationLevel,
            JobCategoryId = c.JobCategoryId,
            ExperienceYears = c.ExperienceYears,
            PhotoUrl = c.PhotoUrl,
            VideoUrl = c.VideoUrl,
            PassportDocumentUrl = c.PassportDocumentUrl,
            ProcurementCost = c.ProcurementCost,
            MonthlySalary = c.MonthlySalary,
            PassportExpiry = c.PassportExpiry,
            MedicalStatus = c.MedicalStatus,
            VisaStatus = c.VisaStatus,
            ExpectedArrivalDate = c.ExpectedArrivalDate,
            ActualArrivalDate = c.ActualArrivalDate,
            Notes = c.Notes,
            ExternalReference = c.ExternalReference,
            CreatedBy = c.CreatedBy,
            UpdatedBy = c.UpdatedBy,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Skills = c.Skills.Select(s => new CandidateSkillDto
            {
                Id = s.Id,
                SkillName = s.SkillName,
                ProficiencyLevel = s.ProficiencyLevel.ToString(),
            }).ToList(),
            Languages = c.Languages.Select(l => new CandidateLanguageDto
            {
                Id = l.Id,
                Language = l.Language,
                ProficiencyLevel = l.ProficiencyLevel.ToString(),
            }).ToList(),
            StatusHistory = includeStatusHistory
                ? c.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
        };
    }

    private static CandidateListDto MapToListDto(Entities.Candidate c)
    {
        return new CandidateListDto
        {
            Id = c.Id,
            FullNameEn = c.FullNameEn,
            FullNameAr = c.FullNameAr,
            Nationality = c.Nationality,
            PassportNumber = c.PassportNumber,
            SourceType = c.SourceType.ToString(),
            TenantSupplierId = c.TenantSupplierId,
            Status = c.Status.ToString(),
            Gender = c.Gender,
            ExternalReference = c.ExternalReference,
            JobCategoryId = c.JobCategoryId,
            PhotoUrl = c.PhotoUrl,
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
        };
    }

    private static CandidateStatusHistoryDto MapToHistoryDto(CandidateStatusHistory h)
    {
        return new CandidateStatusHistoryDto
        {
            Id = h.Id,
            CandidateId = h.CandidateId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    #endregion
}
