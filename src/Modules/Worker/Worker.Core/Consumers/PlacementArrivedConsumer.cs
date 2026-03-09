using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using Candidate.Contracts;

namespace Worker.Core.Consumers;

/// <summary>
/// Creates a Worker record when a Placement reaches Arrived status.
/// This means the candidate has physically arrived in the UAE.
/// </summary>
public class PlacementArrivedConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICandidateService _candidateService;
    private readonly ILogger<PlacementArrivedConsumer> _logger;

    public PlacementArrivedConsumer(
        AppDbContext db,
        IClock clock,
        ICandidateService candidateService,
        ILogger<PlacementArrivedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _candidateService = candidateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementStatusChangedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Only act when placement reaches Arrived
        if (message.ToStatus != "Arrived")
            return;

        _logger.LogInformation(
            "Placement {PlacementId} arrived — creating worker for candidate {CandidateId} in tenant {TenantId}",
            message.PlacementId, message.CandidateId, message.TenantId);

        // Idempotency: skip if worker already exists for this candidate+tenant
        var existingWorker = await _db.Set<Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == message.TenantId && x.CandidateId == message.CandidateId, ct);

        if (existingWorker is not null)
        {
            _logger.LogWarning(
                "Worker already exists for candidate {CandidateId} in tenant {TenantId}, linking to placement",
                message.CandidateId, message.TenantId);

            // Link placement to existing worker via raw SQL (avoids cross-module entity dependency)
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE placements SET worker_id = {existingWorker.Id} WHERE id = {message.PlacementId} AND tenant_id = {message.TenantId}",
                ct);

            return;
        }

        // Fetch candidate data
        var candidateResult = await _candidateService.GetByIdAsync(message.TenantId, message.CandidateId, ct: ct);
        if (!candidateResult.IsSuccess)
        {
            _logger.LogError("Could not fetch candidate {CandidateId} for worker creation", message.CandidateId);
            return;
        }

        var data = candidateResult.Value!;

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

        var worker = new Entities.Worker
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            CandidateId = message.CandidateId,
            WorkerCode = workerCode,
            Status = WorkerStatus.NewArrival,
            Location = WorkerLocation.InCountry,
            StatusChangedAt = now,

            // Personal snapshot from candidate
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
            SourceType = WorkerSourceType.Supplier,
            TenantSupplierId = data.TenantSupplierId,
        };

        // Copy skills from candidate
        if (data.Skills?.Count > 0)
        {
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
        }

        // Copy languages from candidate
        if (data.Languages?.Count > 0)
        {
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
        }

        // Initial status history entry
        var history = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            WorkerId = worker.Id,
            FromStatus = null,
            ToStatus = WorkerStatus.NewArrival,
            ChangedAt = now,
            ChangedBy = null,
            Notes = $"Auto-created from placement arrival (Placement {message.PlacementId})",
        };

        _db.Set<Entities.Worker>().Add(worker);
        _db.Set<WorkerStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        // Link placement to the newly created worker via raw SQL (avoids cross-module entity dependency)
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE placements SET worker_id = {worker.Id} WHERE id = {message.PlacementId} AND tenant_id = {message.TenantId}",
            ct);

        _logger.LogInformation(
            "Created worker {WorkerId} ({WorkerCode}) from placement arrival for candidate {CandidateId}",
            worker.Id, workerCode, message.CandidateId);
    }
}
