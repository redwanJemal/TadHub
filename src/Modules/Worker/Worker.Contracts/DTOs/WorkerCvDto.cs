namespace Worker.Contracts.DTOs;

/// <summary>
/// Aggregated CV view for a worker.
/// </summary>
public sealed record WorkerCvDto
{
    public Guid Id { get; init; }
    public string WorkerCode { get; init; } = string.Empty;

    // Personal
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PassportNumber { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }

    // Professional
    public string? Religion { get; init; }
    public string? MaritalStatus { get; init; }
    public string? EducationLevel { get; init; }
    public string? JobCategoryName { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }

    // Media
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? PassportDocumentUrl { get; init; }

    // Collections
    public List<WorkerSkillDto> Skills { get; init; } = new();
    public List<WorkerLanguageDto> Languages { get; init; } = new();
}
