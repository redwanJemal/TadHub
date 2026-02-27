namespace Worker.Contracts.DTOs;

/// <summary>
/// Full worker response DTO.
/// </summary>
public sealed record WorkerDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid CandidateId { get; init; }
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
    public Guid? JobCategoryId { get; init; }
    public JobCategoryInfoDto? JobCategory { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }

    // Media
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? PassportDocumentUrl { get; init; }

    // Source
    public string SourceType { get; init; } = string.Empty;
    public Guid? TenantSupplierId { get; init; }
    public WorkerSupplierDto? Supplier { get; init; }

    // Status
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? TerminatedAt { get; init; }
    public string? TerminationReason { get; init; }
    public string? Notes { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Collections
    public List<WorkerSkillDto> Skills { get; init; } = new();
    public List<WorkerLanguageDto> Languages { get; init; } = new();
    public List<WorkerStatusHistoryDto>? StatusHistory { get; init; }
}
