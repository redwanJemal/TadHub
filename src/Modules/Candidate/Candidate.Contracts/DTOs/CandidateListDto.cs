namespace Candidate.Contracts.DTOs;

/// <summary>
/// Lightweight candidate DTO for list views.
/// </summary>
public sealed record CandidateListDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public Guid? TenantSupplierId { get; init; }
    public CandidateSupplierDto? Supplier { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Gender { get; init; }
    public string? ExternalReference { get; init; }
    public Guid? JobCategoryId { get; init; }
    public string? JobCategoryName { get; init; }
    public string? PhotoUrl { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
