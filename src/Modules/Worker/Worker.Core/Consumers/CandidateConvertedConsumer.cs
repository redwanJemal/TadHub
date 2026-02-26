using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Worker.Core.Consumers;

/// <summary>
/// Consumes CandidateConvertedEvent to auto-create a Worker record.
/// </summary>
public class CandidateConvertedConsumer : IConsumer<CandidateConvertedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<CandidateConvertedConsumer> _logger;

    public CandidateConvertedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<CandidateConvertedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CandidateConvertedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Received CandidateConvertedEvent for candidate {CandidateId} in tenant {TenantId}",
            message.CandidateId, message.TenantId);

        // Idempotency: skip if worker already exists for this candidate+tenant
        var exists = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == message.TenantId && x.CandidateId == message.CandidateId, ct);

        if (exists)
        {
            _logger.LogWarning(
                "Worker already exists for candidate {CandidateId} in tenant {TenantId}, skipping",
                message.CandidateId, message.TenantId);
            return;
        }

        // Generate worker code: WRK-000001
        var maxCode = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == message.TenantId)
            .OrderByDescending(x => x.WorkerCode)
            .Select(x => x.WorkerCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (maxCode is not null && maxCode.StartsWith("WRK-") && int.TryParse(maxCode[4..], out var current))
        {
            nextNumber = current + 1;
        }

        var workerCode = $"WRK-{nextNumber:D6}";

        var now = _clock.UtcNow;
        var data = message.CandidateData;

        // Parse source type
        Enum.TryParse<WorkerSourceType>(data.SourceType, ignoreCase: true, out var sourceType);

        var worker = new Entities.Worker
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            CandidateId = message.CandidateId,
            WorkerCode = workerCode,
            Status = WorkerStatus.Active,
            StatusChangedAt = now,
            ActivatedAt = now,

            // Personal snapshot
            FullNameEn = data.FullNameEn,
            FullNameAr = data.FullNameAr,
            Nationality = data.Nationality,
            DateOfBirth = data.DateOfBirth,
            Gender = data.Gender,
            PassportNumber = data.PassportNumber,
            PassportExpiry = data.PassportExpiry,
            Phone = data.Phone,
            Email = data.Email,

            // Professional
            Religion = data.Religion,
            MaritalStatus = data.MaritalStatus,
            EducationLevel = data.EducationLevel,
            JobCategoryId = data.JobCategoryId,
            ExperienceYears = data.ExperienceYears,
            MonthlySalary = data.MonthlySalary,

            // Media
            PhotoUrl = data.PhotoUrl,
            VideoUrl = data.VideoUrl,
            PassportDocumentUrl = data.PassportDocumentUrl,

            // Source
            SourceType = sourceType,
            TenantSupplierId = data.TenantSupplierId,
        };

        // Copy skills
        foreach (var skill in data.Skills)
        {
            worker.Skills.Add(new WorkerSkill
            {
                Id = Guid.NewGuid(),
                TenantId = message.TenantId,
                WorkerId = worker.Id,
                SkillName = skill.SkillName,
                ProficiencyLevel = skill.ProficiencyLevel,
            });
        }

        // Copy languages
        foreach (var lang in data.Languages)
        {
            worker.Languages.Add(new WorkerLanguage
            {
                Id = Guid.NewGuid(),
                TenantId = message.TenantId,
                WorkerId = worker.Id,
                Language = lang.Language,
                ProficiencyLevel = lang.ProficiencyLevel,
            });
        }

        // Initial status history entry
        var history = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            WorkerId = worker.Id,
            FromStatus = null,
            ToStatus = WorkerStatus.Active,
            ChangedAt = now,
            ChangedBy = null, // System action
            Notes = "Auto-created from converted candidate",
        };

        _db.Set<Entities.Worker>().Add(worker);
        _db.Set<WorkerStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created worker {WorkerId} ({WorkerCode}) from candidate {CandidateId}",
            worker.Id, workerCode, message.CandidateId);
    }
}
