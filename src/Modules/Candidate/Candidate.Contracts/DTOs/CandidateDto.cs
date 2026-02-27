namespace Candidate.Contracts.DTOs;

/// <summary>
/// Full candidate response DTO.
/// </summary>
public sealed record CandidateDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }

    // Personal
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PassportNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }

    // Sourcing
    public string SourceType { get; init; } = string.Empty;
    public Guid? TenantSupplierId { get; init; }
    public CandidateSupplierDto? Supplier { get; init; }

    // Pipeline
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }

    // Professional Profile
    public string? Religion { get; init; }
    public string? MaritalStatus { get; init; }
    public string? EducationLevel { get; init; }
    public Guid? JobCategoryId { get; init; }
    public JobCategoryInfoDto? JobCategory { get; init; }
    public int? ExperienceYears { get; init; }

    // Media
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? PassportDocumentUrl { get; init; }

    // Financial
    public decimal? ProcurementCost { get; init; }
    public decimal? MonthlySalary { get; init; }

    // Document tracking
    public DateOnly? PassportExpiry { get; init; }
    public string? MedicalStatus { get; init; }
    public string? VisaStatus { get; init; }

    // Operational
    public DateOnly? ExpectedArrivalDate { get; init; }
    public DateOnly? ActualArrivalDate { get; init; }
    public string? Notes { get; init; }
    public string? ExternalReference { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Collections
    public List<CandidateSkillDto> Skills { get; init; } = new();
    public List<CandidateLanguageDto> Languages { get; init; } = new();

    /// <summary>
    /// Status history. Included when requested via ?include=statusHistory.
    /// </summary>
    public List<CandidateStatusHistoryDto>? StatusHistory { get; init; }
}
