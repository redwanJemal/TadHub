using System.ComponentModel.DataAnnotations;

namespace Candidate.Contracts.DTOs;

/// <summary>
/// Request to create a new candidate.
/// </summary>
public sealed record CreateCandidateRequest
{
    /// <summary>
    /// Full name in English. Required.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FullNameEn { get; init; } = string.Empty;

    /// <summary>
    /// Full name in Arabic. Optional.
    /// </summary>
    [MaxLength(255)]
    public string? FullNameAr { get; init; }

    /// <summary>
    /// ISO alpha-2 nationality code. Required.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Nationality { get; init; } = string.Empty;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; init; }

    /// <summary>
    /// Gender (Male, Female).
    /// </summary>
    [MaxLength(20)]
    public string? Gender { get; init; }

    /// <summary>
    /// Passport number.
    /// </summary>
    [MaxLength(50)]
    public string? PassportNumber { get; init; }

    /// <summary>
    /// Phone number.
    /// </summary>
    [Phone]
    [MaxLength(50)]
    public string? Phone { get; init; }

    /// <summary>
    /// Email address.
    /// </summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    /// <summary>
    /// Sourcing channel: Supplier or Local. Required.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SourceType { get; init; } = string.Empty;

    /// <summary>
    /// FK to TenantSupplier. Required when SourceType is Supplier, must be null for Local.
    /// </summary>
    public Guid? TenantSupplierId { get; init; }

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

    /// <summary>
    /// Skills to associate with the candidate.
    /// </summary>
    public List<CandidateSkillRequest>? Skills { get; init; }

    /// <summary>
    /// Languages to associate with the candidate.
    /// </summary>
    public List<CandidateLanguageRequest>? Languages { get; init; }

    /// <summary>
    /// TenantFile ID for a photo uploaded before form submission (deferred upload).
    /// </summary>
    public Guid? PhotoFileId { get; init; }

    /// <summary>
    /// TenantFile ID for a passport document uploaded before form submission (deferred upload).
    /// </summary>
    public Guid? PassportFileId { get; init; }

    /// <summary>
    /// Passport expiry date.
    /// </summary>
    public DateOnly? PassportExpiry { get; init; }

    /// <summary>
    /// Medical clearance status.
    /// </summary>
    [MaxLength(100)]
    public string? MedicalStatus { get; init; }

    /// <summary>
    /// Visa status.
    /// </summary>
    [MaxLength(100)]
    public string? VisaStatus { get; init; }

    /// <summary>
    /// Expected arrival date.
    /// </summary>
    public DateOnly? ExpectedArrivalDate { get; init; }

    /// <summary>
    /// Internal notes.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; init; }

    /// <summary>
    /// External reference number (e.g., supplier tracking ID).
    /// </summary>
    [MaxLength(100)]
    public string? ExternalReference { get; init; }
}
