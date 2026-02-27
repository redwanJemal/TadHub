using System.ComponentModel.DataAnnotations;

namespace Candidate.Contracts.DTOs;

/// <summary>
/// Partial update request for a candidate. All fields are nullable.
/// </summary>
public sealed record UpdateCandidateRequest
{
    [MaxLength(255)]
    public string? FullNameEn { get; init; }

    [MaxLength(255)]
    public string? FullNameAr { get; init; }

    [MaxLength(10)]
    public string? Nationality { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [MaxLength(20)]
    public string? Gender { get; init; }

    [MaxLength(50)]
    public string? PassportNumber { get; init; }

    [Phone]
    [MaxLength(50)]
    public string? Phone { get; init; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    // Professional Profile

    [MaxLength(50)]
    public string? Religion { get; init; }

    [MaxLength(20)]
    public string? MaritalStatus { get; init; }

    [MaxLength(50)]
    public string? EducationLevel { get; init; }

    public Guid? JobCategoryId { get; init; }

    public int? ExperienceYears { get; init; }

    public decimal? MonthlySalary { get; init; }

    // Media (set by upload endpoints, but can be cleared via update)
    [MaxLength(500)]
    public string? PhotoUrl { get; init; }

    [MaxLength(500)]
    public string? VideoUrl { get; init; }

    [MaxLength(500)]
    public string? PassportDocumentUrl { get; init; }

    public decimal? ProcurementCost { get; init; }

    /// <summary>
    /// Skills to replace (if provided, full-replacement semantics; if null, leave untouched).
    /// </summary>
    public List<CandidateSkillRequest>? Skills { get; init; }

    /// <summary>
    /// Languages to replace (if provided, full-replacement semantics; if null, leave untouched).
    /// </summary>
    public List<CandidateLanguageRequest>? Languages { get; init; }

    public DateOnly? PassportExpiry { get; init; }

    [MaxLength(100)]
    public string? MedicalStatus { get; init; }

    [MaxLength(100)]
    public string? VisaStatus { get; init; }

    public DateOnly? ExpectedArrivalDate { get; init; }

    public DateOnly? ActualArrivalDate { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }

    [MaxLength(100)]
    public string? ExternalReference { get; init; }

    [MaxLength(20)]
    public string? SourceType { get; init; }

    public Guid? TenantSupplierId { get; init; }
}
