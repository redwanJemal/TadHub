using System.ComponentModel.DataAnnotations;

namespace Worker.Contracts.DTOs;

/// <summary>
/// Partial update request for a worker. All fields are nullable — only non-null values are applied.
/// </summary>
public sealed record UpdateWorkerRequest
{
    [MaxLength(255)]
    public string? FullNameEn { get; init; }

    [MaxLength(255)]
    public string? FullNameAr { get; init; }

    [MaxLength(50)]
    public string? Phone { get; init; }

    [MaxLength(255)]
    public string? Email { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }

    // Professional
    [MaxLength(50)]
    public string? Religion { get; init; }

    [MaxLength(20)]
    public string? MaritalStatus { get; init; }

    [MaxLength(50)]
    public string? EducationLevel { get; init; }

    public Guid? JobCategoryId { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }

    // Location & travel
    [MaxLength(20)]
    public string? Location { get; init; }
    public DateTimeOffset? ProcurementPaidAt { get; init; }
    public DateTimeOffset? FlightDate { get; init; }
    public DateTimeOffset? ArrivedAt { get; init; }

    // Skills & Languages (full-replacement semantics)
    public List<WorkerSkillRequest>? Skills { get; init; }
    public List<WorkerLanguageRequest>? Languages { get; init; }
}

public sealed record WorkerSkillRequest
{
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

public sealed record WorkerLanguageRequest
{
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}
