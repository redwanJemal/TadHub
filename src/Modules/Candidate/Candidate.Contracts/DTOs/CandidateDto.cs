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

    // Pipeline
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }

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

    /// <summary>
    /// Status history. Included when requested via ?include=statusHistory.
    /// </summary>
    public List<CandidateStatusHistoryDto>? StatusHistory { get; init; }
}
