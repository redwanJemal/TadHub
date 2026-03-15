using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Worker.Core.Consumers;

/// <summary>
/// Projection record for raw SQL reads from candidates table (Candidate module).
/// </summary>
file sealed record CandidateSnapshot
{
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PassportNumber { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Religion { get; init; }
    public string? MaritalStatus { get; init; }
    public string? EducationLevel { get; init; }
    public Guid? JobCategoryId { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? PassportDocumentUrl { get; init; }
    public Guid? TenantSupplierId { get; init; }
}

/// <summary>
/// Projection record for raw SQL reads from candidate_skills table.
/// </summary>
file sealed record CandidateSkillSnapshot
{
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Projection record for raw SQL reads from candidate_languages table.
/// </summary>
file sealed record CandidateLanguageSnapshot
{
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Creates a Worker record when a Placement reaches Arrived status.
/// This means the candidate has physically arrived in the UAE.
/// </summary>
public class PlacementArrivedConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<PlacementArrivedConsumer> _logger;

    public PlacementArrivedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<PlacementArrivedConsumer> logger)
    {
        _db = db;
        _clock = clock;
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

        // Fetch candidate data via raw SQL (cross-module read from Candidate module)
        var data = await _db.Database.SqlQueryRaw<CandidateSnapshot>(
            @"SELECT full_name_en AS ""FullNameEn"", full_name_ar AS ""FullNameAr"",
                     nationality AS ""Nationality"", date_of_birth AS ""DateOfBirth"",
                     gender AS ""Gender"", passport_number AS ""PassportNumber"",
                     passport_expiry AS ""PassportExpiry"", phone AS ""Phone"", email AS ""Email"",
                     religion AS ""Religion"", marital_status AS ""MaritalStatus"",
                     education_level AS ""EducationLevel"", job_category_id AS ""JobCategoryId"",
                     experience_years AS ""ExperienceYears"", monthly_salary AS ""MonthlySalary"",
                     photo_url AS ""PhotoUrl"", video_url AS ""VideoUrl"",
                     passport_document_url AS ""PassportDocumentUrl"",
                     tenant_supplier_id AS ""TenantSupplierId""
              FROM candidates
              WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
            message.CandidateId, message.TenantId).FirstOrDefaultAsync(ct);

        if (data is null)
        {
            _logger.LogError("Could not fetch candidate {CandidateId} for worker creation", message.CandidateId);
            return;
        }

        // Fetch skills via raw SQL
        var skills = await _db.Database.SqlQueryRaw<CandidateSkillSnapshot>(
            @"SELECT skill_name AS ""SkillName"", proficiency_level AS ""ProficiencyLevel""
              FROM candidate_skills
              WHERE candidate_id = {0} AND tenant_id = {1} AND is_deleted = false",
            message.CandidateId, message.TenantId).ToListAsync(ct);

        // Fetch languages via raw SQL
        var languages = await _db.Database.SqlQueryRaw<CandidateLanguageSnapshot>(
            @"SELECT language AS ""Language"", proficiency_level AS ""ProficiencyLevel""
              FROM candidate_languages
              WHERE candidate_id = {0} AND tenant_id = {1} AND is_deleted = false",
            message.CandidateId, message.TenantId).ToListAsync(ct);

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
        foreach (var skill in skills)
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

        // Copy languages from candidate
        foreach (var lang in languages)
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
