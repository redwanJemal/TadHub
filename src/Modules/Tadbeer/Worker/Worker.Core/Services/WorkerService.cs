using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events.Tadbeer.Worker;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Worker.Contracts;
using Worker.Contracts.DTOs;
using Worker.Core.Entities;
using Worker.Core.StateMachine;

// Resolve ambiguous types - use entities
using WorkerStatus = Worker.Core.Entities.WorkerStatus;
using PassportLocation = Worker.Core.Entities.PassportLocation;
using Gender = Worker.Core.Entities.Gender;
using Religion = Worker.Core.Entities.Religion;
using MaritalStatus = Worker.Core.Entities.MaritalStatus;
using EducationLevel = Worker.Core.Entities.EducationLevel;
using LanguageProficiency = Worker.Core.Entities.LanguageProficiency;
using MediaType = Worker.Core.Entities.MediaType;

namespace Worker.Core.Services;

/// <summary>
/// Service implementation for worker management.
/// </summary>
public class WorkerService : IWorkerService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly StateTransitionValidator _transitionValidator;
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(
        AppDbContext db,
        IPublishEndpoint publisher,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IClock clock,
        StateTransitionValidator transitionValidator,
        ILogger<WorkerService> logger)
    {
        _db = db;
        _publisher = publisher;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
        _transitionValidator = transitionValidator;
        _logger = logger;
    }

    #region Worker CRUD

    public async Task<Result<WorkerDto>> CreateAsync(CreateWorkerRequest request, CancellationToken ct = default)
    {
        // Check for duplicate passport within tenant
        var exists = await _db.Set<Entities.Worker>()
            .AnyAsync(w => w.PassportNumber == request.PassportNumber, ct);

        if (exists)
        {
            return Result<WorkerDto>.Failure(
                "Worker with this passport number already exists",
                "DUPLICATE_PASSPORT");
        }

        // Generate CV serial if not provided
        var cvSerial = request.CvSerial ?? await GenerateCvSerialAsync(ct);

        var worker = new Entities.Worker
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            PassportNumber = request.PassportNumber,
            EmiratesId = request.EmiratesId,
            CvSerial = cvSerial,
            FullNameEn = request.FullNameEn,
            FullNameAr = request.FullNameAr,
            Nationality = request.Nationality,
            DateOfBirth = request.DateOfBirth,
            Gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true),
            Religion = Enum.Parse<Religion>(request.Religion, ignoreCase: true),
            MaritalStatus = Enum.Parse<MaritalStatus>(request.MaritalStatus, ignoreCase: true),
            NumberOfChildren = request.NumberOfChildren,
            Education = Enum.Parse<EducationLevel>(request.Education, ignoreCase: true),
            YearsOfExperience = request.YearsOfExperience,
            JobCategoryId = request.JobCategoryId,
            MonthlyBaseSalary = request.MonthlyBaseSalary,
            IsAvailableForFlexible = request.IsAvailableForFlexible,
            PhotoUrl = request.PhotoUrl,
            VideoUrl = request.VideoUrl,
            Notes = request.Notes,
            CurrentStatus = WorkerStatus.NewArrival,
            PassportLocation = PassportLocation.WithAgency,
            CreatedBy = _currentUser.UserId
        };

        // Add skills
        if (request.Skills != null)
        {
            foreach (var skill in request.Skills)
            {
                worker.Skills.Add(new WorkerSkill
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantContext.TenantId,
                    SkillName = skill.SkillName,
                    Rating = Math.Clamp(skill.Rating, 0, 100)
                });
            }
        }

        // Add languages
        if (request.Languages != null)
        {
            foreach (var lang in request.Languages)
            {
                worker.Languages.Add(new WorkerLanguage
                {
                    Id = Guid.NewGuid(),
                    TenantId = _tenantContext.TenantId,
                    Language = lang.Language,
                    Proficiency = Enum.Parse<LanguageProficiency>(lang.Proficiency, ignoreCase: true)
                });
            }
        }

        // Initialize passport custody history
        worker.PassportCustodyHistory.Add(new WorkerPassportCustody
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            Location = PassportLocation.WithAgency,
            ReceivedAt = _clock.UtcNow,
            RecordedByUserId = _currentUser.UserId,
            Notes = "Initial intake"
        });

        _db.Set<Entities.Worker>().Add(worker);
        await _db.SaveChangesAsync(ct);

        // Publish event
        await _publisher.Publish(new WorkerCreatedEvent
        {
            TenantId = _tenantContext.TenantId,
            WorkerId = worker.Id,
            CvSerial = worker.CvSerial,
            FullNameEn = worker.FullNameEn,
            Nationality = worker.Nationality,
            JobCategoryId = worker.JobCategoryId,
            CreatedByUserId = _currentUser.UserId
        }, ct);

        _logger.LogInformation("Worker {WorkerId} created with CV serial {CvSerial}",
            worker.Id, worker.CvSerial);

        return Result<WorkerDto>.Success(MapToDto(worker, IncludeResolver.Parse("skills,languages")));
    }

    public async Task<Result<WorkerDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Worker>().AsQueryable();

        if (includes.Has("skills"))
            query = query.Include(w => w.Skills);
        if (includes.Has("languages"))
            query = query.Include(w => w.Languages);
        if (includes.Has("media"))
            query = query.Include(w => w.Media);
        if (includes.Has("jobCategory"))
            query = query.Include(w => w.JobCategory);

        var worker = await query.FirstOrDefaultAsync(w => w.Id == id, ct);

        if (worker == null)
            return Result<WorkerDto>.Failure("Worker not found", "NOT_FOUND");

        return Result<WorkerDto>.Success(MapToDto(worker, includes));
    }

    public async Task<Result<WorkerDto>> UpdateAsync(Guid id, UpdateWorkerRequest request, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>().FindAsync([id], ct);

        if (worker == null)
            return Result<WorkerDto>.Failure("Worker not found", "NOT_FOUND");

        // Update fields if provided
        if (request.FullNameEn != null) worker.FullNameEn = request.FullNameEn;
        if (request.FullNameAr != null) worker.FullNameAr = request.FullNameAr;
        if (request.EmiratesId != null) worker.EmiratesId = request.EmiratesId;
        if (request.Religion != null) worker.Religion = Enum.Parse<Religion>(request.Religion, ignoreCase: true);
        if (request.MaritalStatus != null) worker.MaritalStatus = Enum.Parse<MaritalStatus>(request.MaritalStatus, ignoreCase: true);
        if (request.NumberOfChildren != null) worker.NumberOfChildren = request.NumberOfChildren;
        if (request.Education != null) worker.Education = Enum.Parse<EducationLevel>(request.Education, ignoreCase: true);
        if (request.YearsOfExperience != null) worker.YearsOfExperience = request.YearsOfExperience;
        if (request.JobCategoryId != null) worker.JobCategoryId = request.JobCategoryId.Value;
        if (request.MonthlyBaseSalary != null) worker.MonthlyBaseSalary = request.MonthlyBaseSalary.Value;
        if (request.IsAvailableForFlexible != null) worker.IsAvailableForFlexible = request.IsAvailableForFlexible.Value;
        if (request.PhotoUrl != null) worker.PhotoUrl = request.PhotoUrl;
        if (request.VideoUrl != null) worker.VideoUrl = request.VideoUrl;
        if (request.Notes != null) worker.Notes = request.Notes;

        worker.UpdatedBy = _currentUser.UserId;
        worker.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Result<WorkerDto>.Success(MapToDto(worker, IncludeResolver.Parse(null)));
    }

    public async Task<PagedList<WorkerDto>> ListAsync(QueryParameters query, CancellationToken ct = default)
    {
        var dbQuery = _db.Set<Entities.Worker>().AsQueryable();

        // Apply filters
        dbQuery = dbQuery.ApplyFilters(query.Filters, GetFilterExpressions());

        // Apply sorting
        dbQuery = dbQuery.ApplySort(query.GetSortFields(), GetSortExpressions());

        var pagedResult = await dbQuery.ToPagedListAsync(query.Page, query.PageSize, ct);

        var includes = query.GetIncludes();
        return new PagedList<WorkerDto>(
            pagedResult.Items.Select(w => MapToDto(w, includes)).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    #endregion

    #region State Machine

    public async Task<Result<WorkerDto>> TransitionStateAsync(Guid id, WorkerStateTransitionRequest request, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>()
            .Include(w => w.StateHistory)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (worker == null)
            return Result<WorkerDto>.Failure("Worker not found", "NOT_FOUND");

        var targetStatus = Enum.Parse<WorkerStatus>(request.TargetState, ignoreCase: true);

        // Build context for validation
        var context = new TransitionContext
        {
            WorkerId = worker.Id,
            CurrentStatus = worker.CurrentStatus,
            TargetStatus = targetStatus,
            RelatedEntityId = request.RelatedEntityId,
            HasValidMedical = true, // TODO: Check actual medical status
            HasValidVisa = true,    // TODO: Check actual visa status
            HasActiveInsurance = true, // TODO: Check actual insurance
            IsClientVerified = true // TODO: Check client verification
        };

        // Validate transition
        var validationResult = _transitionValidator.ValidateTransition(
            worker.CurrentStatus, targetStatus, context);

        if (!validationResult.IsSuccess)
        {
            return Result<WorkerDto>.Failure(validationResult.Error!, validationResult.ErrorCode);
        }

        var fromStatus = worker.CurrentStatus;

        // Append state history (never overwrite)
        worker.StateHistory.Add(new WorkerStateHistory
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            Reason = request.Reason ?? validationResult.Value!.DefaultReason,
            TriggeredByUserId = _currentUser.UserId,
            RelatedEntityId = request.RelatedEntityId,
            OccurredAt = _clock.UtcNow
        });

        // Update current status
        worker.CurrentStatus = targetStatus;
        worker.UpdatedBy = _currentUser.UserId;
        worker.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Publish event
        await _publisher.Publish(new WorkerStatusChangedEvent
        {
            TenantId = _tenantContext.TenantId,
            WorkerId = worker.Id,
            FromStatus = fromStatus.ToString(),
            ToStatus = targetStatus.ToString(),
            Reason = request.Reason,
            RelatedEntityId = request.RelatedEntityId,
            ChangedByUserId = _currentUser.UserId
        }, ct);

        // Publish special events for certain transitions
        if (targetStatus == WorkerStatus.Absconded)
        {
            await _publisher.Publish(new WorkerAbscondedEvent
            {
                TenantId = _tenantContext.TenantId,
                WorkerId = worker.Id,
                ReportedByUserId = _currentUser.UserId
            }, ct);
        }

        _logger.LogInformation("Worker {WorkerId} transitioned from {From} to {To}",
            worker.Id, fromStatus, targetStatus);

        return Result<WorkerDto>.Success(MapToDto(worker, IncludeResolver.Parse(null)));
    }

    public async Task<PagedList<WorkerStateHistoryDto>> GetStateHistoryAsync(Guid id, QueryParameters query, CancellationToken ct = default)
    {
        var pagedResult = await _db.Set<WorkerStateHistory>()
            .Where(h => h.WorkerId == id)
            .OrderByDescending(h => h.OccurredAt)
            .ToPagedListAsync(query.Page, query.PageSize, ct);

        return new PagedList<WorkerStateHistoryDto>(
            pagedResult.Items.Select(MapStateHistoryToDto).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<Result<List<string>>> GetValidTransitionsAsync(Guid id, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>().FindAsync([id], ct);

        if (worker == null)
            return Result<List<string>>.Failure("Worker not found", "NOT_FOUND");

        var validStates = WorkerStateMachine.GetValidTargetStates(worker.CurrentStatus)
            .Select(s => s.ToString().ToLowerInvariant())
            .ToList();

        return Result<List<string>>.Success(validStates);
    }

    #endregion

    #region Passport Custody

    public async Task<Result<PassportCustodyDto>> GetPassportCustodyAsync(Guid id, CancellationToken ct = default)
    {
        var latest = await _db.Set<WorkerPassportCustody>()
            .Where(c => c.WorkerId == id)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latest == null)
            return Result<PassportCustodyDto>.Failure("No custody record found", "NOT_FOUND");

        return Result<PassportCustodyDto>.Success(MapPassportCustodyToDto(latest));
    }

    public async Task<PagedList<PassportCustodyDto>> GetPassportCustodyHistoryAsync(Guid id, QueryParameters query, CancellationToken ct = default)
    {
        var pagedResult = await _db.Set<WorkerPassportCustody>()
            .Where(c => c.WorkerId == id)
            .OrderByDescending(c => c.CreatedAt)
            .ToPagedListAsync(query.Page, query.PageSize, ct);

        return new PagedList<PassportCustodyDto>(
            pagedResult.Items.Select(MapPassportCustodyToDto).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<Result<PassportCustodyDto>> TransferPassportAsync(Guid id, TransferPassportRequest request, CancellationToken ct = default)
    {
        var worker = await _db.Set<Entities.Worker>().FindAsync([id], ct);

        if (worker == null)
            return Result<PassportCustodyDto>.Failure("Worker not found", "NOT_FOUND");

        var newLocation = Enum.Parse<PassportLocation>(request.Location, ignoreCase: true);

        // Create custody record (append-only)
        var custody = new WorkerPassportCustody
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            WorkerId = id,
            Location = newLocation,
            HandedToName = request.HandedToName,
            HandedToEntityId = request.HandedToEntityId,
            HandedAt = _clock.UtcNow,
            ReceivedAt = _clock.UtcNow,
            RecordedByUserId = _currentUser.UserId,
            Notes = request.Notes
        };

        _db.Set<WorkerPassportCustody>().Add(custody);

        // Update worker's current passport location
        worker.PassportLocation = newLocation;
        worker.UpdatedBy = _currentUser.UserId;
        worker.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Worker {WorkerId} passport transferred to {Location}",
            id, newLocation);

        return Result<PassportCustodyDto>.Success(MapPassportCustodyToDto(custody));
    }

    #endregion

    #region Skills & Languages

    public async Task<Result<WorkerSkillDto>> UpsertSkillAsync(Guid workerId, CreateWorkerSkillRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Set<WorkerSkill>()
            .FirstOrDefaultAsync(s => s.WorkerId == workerId && s.SkillName == request.SkillName, ct);

        if (existing != null)
        {
            existing.Rating = Math.Clamp(request.Rating, 0, 100);
            await _db.SaveChangesAsync(ct);
            return Result<WorkerSkillDto>.Success(MapSkillToDto(existing));
        }

        var skill = new WorkerSkill
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            WorkerId = workerId,
            SkillName = request.SkillName,
            Rating = Math.Clamp(request.Rating, 0, 100)
        };

        _db.Set<WorkerSkill>().Add(skill);
        await _db.SaveChangesAsync(ct);

        return Result<WorkerSkillDto>.Success(MapSkillToDto(skill));
    }

    public async Task<Result> RemoveSkillAsync(Guid workerId, string skillName, CancellationToken ct = default)
    {
        var skill = await _db.Set<WorkerSkill>()
            .FirstOrDefaultAsync(s => s.WorkerId == workerId && s.SkillName == skillName, ct);

        if (skill == null)
            return Result.Failure("Skill not found", "NOT_FOUND");

        _db.Set<WorkerSkill>().Remove(skill);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<WorkerLanguageDto>> UpsertLanguageAsync(Guid workerId, CreateWorkerLanguageRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Set<WorkerLanguage>()
            .FirstOrDefaultAsync(l => l.WorkerId == workerId && l.Language == request.Language, ct);

        if (existing != null)
        {
            existing.Proficiency = Enum.Parse<LanguageProficiency>(request.Proficiency, ignoreCase: true);
            await _db.SaveChangesAsync(ct);
            return Result<WorkerLanguageDto>.Success(MapLanguageToDto(existing));
        }

        var language = new WorkerLanguage
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            WorkerId = workerId,
            Language = request.Language,
            Proficiency = Enum.Parse<LanguageProficiency>(request.Proficiency, ignoreCase: true)
        };

        _db.Set<WorkerLanguage>().Add(language);
        await _db.SaveChangesAsync(ct);

        return Result<WorkerLanguageDto>.Success(MapLanguageToDto(language));
    }

    public async Task<Result> RemoveLanguageAsync(Guid workerId, string language, CancellationToken ct = default)
    {
        var lang = await _db.Set<WorkerLanguage>()
            .FirstOrDefaultAsync(l => l.WorkerId == workerId && l.Language == language, ct);

        if (lang == null)
            return Result.Failure("Language not found", "NOT_FOUND");

        _db.Set<WorkerLanguage>().Remove(lang);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    #endregion

    #region Media

    public async Task<Result<WorkerMediaDto>> AddMediaAsync(Guid workerId, AddWorkerMediaRequest request, CancellationToken ct = default)
    {
        var workerExists = await _db.Set<Entities.Worker>().AnyAsync(w => w.Id == workerId, ct);
        if (!workerExists)
            return Result<WorkerMediaDto>.Failure("Worker not found", "NOT_FOUND");

        // If setting as primary, unset existing primary
        if (request.IsPrimary)
        {
            var mediaType = Enum.Parse<MediaType>(request.MediaType, ignoreCase: true);
            var existingPrimary = await _db.Set<WorkerMedia>()
                .Where(m => m.WorkerId == workerId && m.MediaType == mediaType && m.IsPrimary)
                .ToListAsync(ct);

            foreach (var media in existingPrimary)
                media.IsPrimary = false;
        }

        var newMedia = new WorkerMedia
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            WorkerId = workerId,
            MediaType = Enum.Parse<MediaType>(request.MediaType, ignoreCase: true),
            FileUrl = request.FileUrl,
            IsPrimary = request.IsPrimary,
            UploadedAt = _clock.UtcNow,
            UploadedByUserId = _currentUser.UserId
        };

        _db.Set<WorkerMedia>().Add(newMedia);
        await _db.SaveChangesAsync(ct);

        return Result<WorkerMediaDto>.Success(MapMediaToDto(newMedia));
    }

    public async Task<Result> RemoveMediaAsync(Guid workerId, Guid mediaId, CancellationToken ct = default)
    {
        var media = await _db.Set<WorkerMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.WorkerId == workerId, ct);

        if (media == null)
            return Result.Failure("Media not found", "NOT_FOUND");

        _db.Set<WorkerMedia>().Remove(media);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<WorkerMediaDto>> SetPrimaryMediaAsync(Guid workerId, Guid mediaId, CancellationToken ct = default)
    {
        var media = await _db.Set<WorkerMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.WorkerId == workerId, ct);

        if (media == null)
            return Result<WorkerMediaDto>.Failure("Media not found", "NOT_FOUND");

        // Unset existing primary of same type
        var existingPrimary = await _db.Set<WorkerMedia>()
            .Where(m => m.WorkerId == workerId && m.MediaType == media.MediaType && m.IsPrimary && m.Id != mediaId)
            .ToListAsync(ct);

        foreach (var m in existingPrimary)
            m.IsPrimary = false;

        media.IsPrimary = true;
        await _db.SaveChangesAsync(ct);

        return Result<WorkerMediaDto>.Success(MapMediaToDto(media));
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateCvSerialAsync(CancellationToken ct)
    {
        // Format: TN-YYYY-NNNNN (TenantId prefix + year + sequential)
        var year = DateTime.UtcNow.Year;
        var count = await _db.Set<Entities.Worker>()
            .Where(w => w.CvSerial.StartsWith($"{year}-"))
            .CountAsync(ct);

        return $"{year}-{(count + 1):D5}";
    }

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Entities.Worker, object>>> GetFilterExpressions() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["status"] = x => x.CurrentStatus.ToString().ToLower(),
            ["nationality"] = x => x.Nationality,
            ["jobCategoryId"] = x => x.JobCategoryId,
            ["passportLocation"] = x => x.PassportLocation.ToString().ToLower(),
            ["isAvailableForFlexible"] = x => x.IsAvailableForFlexible,
            ["gender"] = x => x.Gender.ToString().ToLower(),
            ["religion"] = x => x.Religion.ToString().ToLower(),
            ["createdAt"] = x => x.CreatedAt
        };

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Entities.Worker, object>>> GetSortExpressions() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAt"] = x => x.CreatedAt,
            ["fullNameEn"] = x => x.FullNameEn,
            ["nationality"] = x => x.Nationality,
            ["monthlyBaseSalary"] = x => x.MonthlyBaseSalary,
            ["dateOfBirth"] = x => x.DateOfBirth
        };

    #endregion

    #region Mapping

    private static WorkerDto MapToDto(Entities.Worker worker, IncludeSet includes) => new()
    {
        Id = worker.Id,
        CvSerial = worker.CvSerial,
        PassportNumber = worker.PassportNumber,
        EmiratesId = worker.EmiratesId,
        FullNameEn = worker.FullNameEn,
        FullNameAr = worker.FullNameAr,
        Nationality = worker.Nationality,
        DateOfBirth = worker.DateOfBirth,
        Age = worker.Age,
        Gender = worker.Gender.ToString().ToLowerInvariant(),
        Religion = worker.Religion.ToString().ToLowerInvariant(),
        MaritalStatus = worker.MaritalStatus.ToString().ToLowerInvariant(),
        NumberOfChildren = worker.NumberOfChildren,
        Education = worker.Education.ToString().ToLowerInvariant(),
        CurrentStatus = worker.CurrentStatus.ToString().ToLowerInvariant(),
        PassportLocation = worker.PassportLocation.ToString().ToLowerInvariant(),
        IsAvailableForFlexible = worker.IsAvailableForFlexible,
        JobCategory = worker.JobCategory != null
            ? new JobCategoryRefDto
            {
                Id = worker.JobCategory.Id,
                Name = worker.JobCategory.Name.Resolve("en"),
                MoHRECode = worker.JobCategory.MoHRECode
            }
            : null,
        MonthlyBaseSalary = worker.MonthlyBaseSalary,
        YearsOfExperience = worker.YearsOfExperience,
        PhotoUrl = worker.PhotoUrl,
        VideoUrl = worker.VideoUrl,
        // Collections: null if not included, [] if included but empty
        Skills = includes.Has("skills")
            ? worker.Skills?.Select(MapSkillToDto).ToList() ?? []
            : null,
        Languages = includes.Has("languages")
            ? worker.Languages?.Select(MapLanguageToDto).ToList() ?? []
            : null,
        Media = includes.Has("media")
            ? worker.Media?.Select(MapMediaToDto).ToList() ?? []
            : null,
        Notes = worker.Notes,
        CreatedAt = worker.CreatedAt,
        UpdatedAt = worker.UpdatedAt
    };

    private static WorkerSkillDto MapSkillToDto(WorkerSkill skill) => new()
    {
        Id = skill.Id,
        SkillName = skill.SkillName,
        Rating = skill.Rating
    };

    private static WorkerLanguageDto MapLanguageToDto(WorkerLanguage lang) => new()
    {
        Id = lang.Id,
        Language = lang.Language,
        Proficiency = lang.Proficiency.ToString().ToLowerInvariant()
    };

    private static WorkerMediaDto MapMediaToDto(WorkerMedia media) => new()
    {
        Id = media.Id,
        MediaType = media.MediaType.ToString().ToLowerInvariant(),
        FileUrl = media.FileUrl,
        IsPrimary = media.IsPrimary,
        UploadedAt = media.UploadedAt
    };

    private static PassportCustodyDto MapPassportCustodyToDto(WorkerPassportCustody custody) => new()
    {
        Id = custody.Id,
        Location = custody.Location.ToString().ToLowerInvariant(),
        HandedToName = custody.HandedToName,
        HandedToEntityId = custody.HandedToEntityId,
        HandedAt = custody.HandedAt,
        ReceivedAt = custody.ReceivedAt,
        Notes = custody.Notes
    };

    private static WorkerStateHistoryDto MapStateHistoryToDto(WorkerStateHistory history) => new()
    {
        Id = history.Id,
        FromStatus = history.FromStatus.ToString().ToLowerInvariant(),
        ToStatus = history.ToStatus.ToString().ToLowerInvariant(),
        Reason = history.Reason,
        TriggeredByUserId = history.TriggeredByUserId,
        RelatedEntityId = history.RelatedEntityId,
        OccurredAt = history.OccurredAt
    };

    #endregion
}
