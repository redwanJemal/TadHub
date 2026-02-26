using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Contracts;
using Worker.Contracts.DTOs;
using Worker.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Worker.Core.Services;

public class WorkerService : IWorkerService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<WorkerService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Worker, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["sourceType"] = x => x.SourceType,
        ["nationality"] = x => x.Nationality,
        ["tenantSupplierId"] = x => x.TenantSupplierId!,
        ["gender"] = x => x.Gender!,
        ["jobCategoryId"] = x => x.JobCategoryId!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Worker, object>>> SortableFields = new()
    {
        ["fullNameEn"] = x => x.FullNameEn,
        ["workerCode"] = x => x.WorkerCode,
        ["nationality"] = x => x.Nationality,
        ["status"] = x => x.Status,
        ["sourceType"] = x => x.SourceType,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["activatedAt"] = x => x.ActivatedAt!,
    };

    public WorkerService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<WorkerService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<WorkerListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.FullNameEn.ToLower().Contains(searchLower) ||
                (x.FullNameAr != null && x.FullNameAr.ToLower().Contains(searchLower)) ||
                x.WorkerCode.ToLower().Contains(searchLower) ||
                (x.PassportNumber != null && x.PassportNumber.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<WorkerDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
        {
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        query = query.Include(x => x.Skills).Include(x => x.Languages);

        var worker = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (worker is null)
            return Result<WorkerDto>.NotFound($"Worker with ID {id} not found");

        return Result<WorkerDto>.Success(MapToDto(worker, includeStatusHistory));
    }

    public async Task<Result<WorkerDto>> UpdateAsync(Guid tenantId, Guid id, UpdateWorkerRequest request, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (worker is null)
            return Result<WorkerDto>.NotFound($"Worker with ID {id} not found");

        // Apply updates (only non-null values)
        if (request.FullNameEn is not null) worker.FullNameEn = request.FullNameEn;
        if (request.FullNameAr is not null) worker.FullNameAr = request.FullNameAr;
        if (request.Phone is not null) worker.Phone = request.Phone;
        if (request.Email is not null) worker.Email = request.Email;
        if (request.Notes is not null) worker.Notes = request.Notes;
        if (request.Religion is not null) worker.Religion = request.Religion;
        if (request.MaritalStatus is not null) worker.MaritalStatus = request.MaritalStatus;
        if (request.EducationLevel is not null) worker.EducationLevel = request.EducationLevel;
        if (request.JobCategoryId.HasValue) worker.JobCategoryId = request.JobCategoryId;
        if (request.ExperienceYears.HasValue) worker.ExperienceYears = request.ExperienceYears;
        if (request.MonthlySalary.HasValue) worker.MonthlySalary = request.MonthlySalary;

        // Full-replacement for Skills
        if (request.Skills is not null)
        {
            await _db.Set<WorkerSkill>()
                .Where(x => x.WorkerId == worker.Id && x.TenantId == tenantId)
                .ExecuteDeleteAsync(ct);

            foreach (var skill in request.Skills)
            {
                _db.Set<WorkerSkill>().Add(new WorkerSkill
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkerId = worker.Id,
                    SkillName = skill.SkillName,
                    ProficiencyLevel = skill.ProficiencyLevel,
                });
            }
        }

        // Full-replacement for Languages
        if (request.Languages is not null)
        {
            await _db.Set<WorkerLanguage>()
                .Where(x => x.WorkerId == worker.Id && x.TenantId == tenantId)
                .ExecuteDeleteAsync(ct);

            foreach (var lang in request.Languages)
            {
                _db.Set<WorkerLanguage>().Add(new WorkerLanguage
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkerId = worker.Id,
                    Language = lang.Language,
                    ProficiencyLevel = lang.ProficiencyLevel,
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated worker {WorkerId}", id);

        return Result<WorkerDto>.Success(MapToDto(worker, includeStatusHistory: false));
    }

    public async Task<Result<WorkerDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionWorkerStatusRequest request, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (worker is null)
            return Result<WorkerDto>.NotFound($"Worker with ID {id} not found");

        if (!Enum.TryParse<WorkerStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<WorkerDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = WorkerStatusMachine.Validate(worker.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<WorkerDto>.ValidationError(error);

        var now = _clock.UtcNow;
        var fromStatus = worker.Status;

        worker.Status = targetStatus;
        worker.StatusChangedAt = now;
        worker.StatusReason = request.Reason;

        if (targetStatus == WorkerStatus.Terminated)
        {
            worker.TerminatedAt = now;
            worker.TerminationReason = request.Reason;
        }

        var history = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkerId = worker.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
            Reason = request.Reason,
            Notes = request.Notes,
        };

        _db.Set<WorkerStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned worker {WorkerId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<WorkerDto>.Success(MapToDto(worker, includeStatusHistory: false));
    }

    public async Task<Result<List<WorkerStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var workerExists = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!workerExists)
            return Result<List<WorkerStatusHistoryDto>>.NotFound($"Worker with ID {id} not found");

        var history = await _db.Set<WorkerStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.WorkerId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<WorkerStatusHistoryDto>>.Success(history);
    }

    public async Task<Result<WorkerCvDto>> GetCvAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Skills)
            .Include(x => x.Languages)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (worker is null)
            return Result<WorkerCvDto>.NotFound($"Worker with ID {id} not found");

        return Result<WorkerCvDto>.Success(MapToCvDto(worker));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (worker is null)
            return Result.NotFound($"Worker with ID {id} not found");

        worker.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted worker {WorkerId}", id);

        return Result.Success();
    }

    #region Mapping

    private static WorkerDto MapToDto(Entities.Worker w, bool includeStatusHistory)
    {
        return new WorkerDto
        {
            Id = w.Id,
            TenantId = w.TenantId,
            CandidateId = w.CandidateId,
            WorkerCode = w.WorkerCode,
            FullNameEn = w.FullNameEn,
            FullNameAr = w.FullNameAr,
            Nationality = w.Nationality,
            DateOfBirth = w.DateOfBirth,
            Gender = w.Gender,
            PassportNumber = w.PassportNumber,
            PassportExpiry = w.PassportExpiry,
            Phone = w.Phone,
            Email = w.Email,
            Religion = w.Religion,
            MaritalStatus = w.MaritalStatus,
            EducationLevel = w.EducationLevel,
            JobCategoryId = w.JobCategoryId,
            ExperienceYears = w.ExperienceYears,
            MonthlySalary = w.MonthlySalary,
            PhotoUrl = w.PhotoUrl,
            VideoUrl = w.VideoUrl,
            PassportDocumentUrl = w.PassportDocumentUrl,
            SourceType = w.SourceType.ToString(),
            TenantSupplierId = w.TenantSupplierId,
            Status = w.Status.ToString(),
            StatusChangedAt = w.StatusChangedAt,
            StatusReason = w.StatusReason,
            ActivatedAt = w.ActivatedAt,
            TerminatedAt = w.TerminatedAt,
            TerminationReason = w.TerminationReason,
            Notes = w.Notes,
            CreatedBy = w.CreatedBy,
            UpdatedBy = w.UpdatedBy,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt,
            Skills = w.Skills.Select(s => new WorkerSkillDto
            {
                Id = s.Id,
                SkillName = s.SkillName,
                ProficiencyLevel = s.ProficiencyLevel,
            }).ToList(),
            Languages = w.Languages.Select(l => new WorkerLanguageDto
            {
                Id = l.Id,
                Language = l.Language,
                ProficiencyLevel = l.ProficiencyLevel,
            }).ToList(),
            StatusHistory = includeStatusHistory
                ? w.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
        };
    }

    private static WorkerListDto MapToListDto(Entities.Worker w)
    {
        return new WorkerListDto
        {
            Id = w.Id,
            WorkerCode = w.WorkerCode,
            FullNameEn = w.FullNameEn,
            FullNameAr = w.FullNameAr,
            Nationality = w.Nationality,
            Gender = w.Gender,
            SourceType = w.SourceType.ToString(),
            TenantSupplierId = w.TenantSupplierId,
            JobCategoryId = w.JobCategoryId,
            Status = w.Status.ToString(),
            PhotoUrl = w.PhotoUrl,
            ActivatedAt = w.ActivatedAt,
            CreatedAt = w.CreatedAt,
        };
    }

    private static WorkerStatusHistoryDto MapToHistoryDto(WorkerStatusHistory h)
    {
        return new WorkerStatusHistoryDto
        {
            Id = h.Id,
            WorkerId = h.WorkerId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    private static WorkerCvDto MapToCvDto(Entities.Worker w)
    {
        return new WorkerCvDto
        {
            Id = w.Id,
            WorkerCode = w.WorkerCode,
            FullNameEn = w.FullNameEn,
            FullNameAr = w.FullNameAr,
            Nationality = w.Nationality,
            DateOfBirth = w.DateOfBirth,
            Gender = w.Gender,
            PassportNumber = w.PassportNumber,
            PassportExpiry = w.PassportExpiry,
            Phone = w.Phone,
            Email = w.Email,
            Religion = w.Religion,
            MaritalStatus = w.MaritalStatus,
            EducationLevel = w.EducationLevel,
            ExperienceYears = w.ExperienceYears,
            MonthlySalary = w.MonthlySalary,
            PhotoUrl = w.PhotoUrl,
            VideoUrl = w.VideoUrl,
            PassportDocumentUrl = w.PassportDocumentUrl,
            Skills = w.Skills.Select(s => new WorkerSkillDto
            {
                Id = s.Id,
                SkillName = s.SkillName,
                ProficiencyLevel = s.ProficiencyLevel,
            }).ToList(),
            Languages = w.Languages.Select(l => new WorkerLanguageDto
            {
                Id = l.Id,
                Language = l.Language,
                ProficiencyLevel = l.ProficiencyLevel,
            }).ToList(),
        };
    }

    #endregion
}
