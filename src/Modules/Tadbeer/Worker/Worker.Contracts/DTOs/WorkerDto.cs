using TadHub.SharedKernel.Api;

namespace Worker.Contracts.DTOs;

/// <summary>
/// Minimal reference DTO for Worker.
/// Used in nested objects when include=worker is NOT specified.
/// </summary>
public record WorkerRefDto : IRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string CvSerial { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
}

/// <summary>
/// Full Worker DTO with all CV details.
/// Used when include=worker IS specified or on detail endpoints.
/// </summary>
public record WorkerDto : IRefDto
{
    public Guid Id { get; init; }
    
    #region Identity
    
    /// <summary>
    /// CV serial number (agency-assigned identifier).
    /// </summary>
    public string CvSerial { get; init; } = string.Empty;
    
    /// <summary>
    /// Passport number.
    /// </summary>
    public string PassportNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// UAE Emirates ID (if issued).
    /// </summary>
    public string? EmiratesId { get; init; }
    
    /// <summary>
    /// Full name in English.
    /// </summary>
    public string FullNameEn { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name in Arabic.
    /// </summary>
    public string FullNameAr { get; init; } = string.Empty;
    
    #endregion
    
    #region Personal Details
    
    /// <summary>
    /// Nationality (e.g., "Philippines", "Indonesia", "Ethiopia").
    /// </summary>
    public string Nationality { get; init; } = string.Empty;
    
    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; init; }
    
    /// <summary>
    /// Age (calculated).
    /// </summary>
    public int Age { get; init; }
    
    /// <summary>
    /// Gender.
    /// </summary>
    public string Gender { get; init; } = string.Empty;
    
    /// <summary>
    /// Religion.
    /// </summary>
    public string Religion { get; init; } = string.Empty;
    
    /// <summary>
    /// Marital status.
    /// </summary>
    public string MaritalStatus { get; init; } = string.Empty;
    
    /// <summary>
    /// Number of children.
    /// </summary>
    public int? NumberOfChildren { get; init; }
    
    /// <summary>
    /// Highest education level.
    /// </summary>
    public string Education { get; init; } = string.Empty;
    
    #endregion
    
    #region Status & Location
    
    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public string CurrentStatus { get; init; } = string.Empty;
    
    /// <summary>
    /// Where the passport is currently held.
    /// </summary>
    public string PassportLocation { get; init; } = string.Empty;
    
    /// <summary>
    /// Whether worker is available for flexible bookings.
    /// </summary>
    public bool IsAvailableForFlexible { get; init; }
    
    #endregion
    
    #region Job & Pricing
    
    /// <summary>
    /// Job category (e.g., Housemaid, Nanny, Driver).
    /// RefDto when not included.
    /// </summary>
    public JobCategoryRefDto? JobCategory { get; init; }
    
    /// <summary>
    /// Monthly base salary in AED.
    /// </summary>
    public decimal MonthlyBaseSalary { get; init; }
    
    /// <summary>
    /// Years of experience.
    /// </summary>
    public int? YearsOfExperience { get; init; }
    
    #endregion
    
    #region Media
    
    /// <summary>
    /// Primary photo URL.
    /// </summary>
    public string? PhotoUrl { get; init; }
    
    /// <summary>
    /// Video introduction URL.
    /// </summary>
    public string? VideoUrl { get; init; }
    
    #endregion
    
    #region Included Collections (null = not included, [] = included but empty)
    
    /// <summary>
    /// Worker skills with ratings.
    /// </summary>
    public List<WorkerSkillDto>? Skills { get; init; }
    
    /// <summary>
    /// Languages spoken.
    /// </summary>
    public List<WorkerLanguageDto>? Languages { get; init; }
    
    /// <summary>
    /// All media files.
    /// </summary>
    public List<WorkerMediaDto>? Media { get; init; }
    
    #endregion
    
    #region Metadata
    
    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; init; }
    
    /// <summary>
    /// If from shared pool, the source tenant ID.
    /// </summary>
    public Guid? SharedFromTenantId { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    
    #endregion
}

/// <summary>
/// Worker skill with rating.
/// </summary>
public record WorkerSkillDto
{
    public Guid Id { get; init; }
    public string SkillName { get; init; } = string.Empty;
    
    /// <summary>
    /// Rating 0-100.
    /// </summary>
    public int Rating { get; init; }
}

/// <summary>
/// Worker language proficiency.
/// </summary>
public record WorkerLanguageDto
{
    public Guid Id { get; init; }
    public string Language { get; init; } = string.Empty;
    public string Proficiency { get; init; } = string.Empty;
}

/// <summary>
/// Worker media file.
/// </summary>
public record WorkerMediaDto
{
    public Guid Id { get; init; }
    public string MediaType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}

/// <summary>
/// Job category reference.
/// </summary>
public record JobCategoryRefDto : IRefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string MoHRECode { get; init; } = string.Empty;
}

/// <summary>
/// Full job category DTO.
/// </summary>
public record JobCategoryDto : IRefDto
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Localized name (en/ar).
    /// </summary>
    public Dictionary<string, string> Name { get; init; } = new();
    
    /// <summary>
    /// MoHRE official code.
    /// </summary>
    public string MoHRECode { get; init; } = string.Empty;
    
    /// <summary>
    /// Whether this category is active.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Passport custody record.
/// </summary>
public record PassportCustodyDto
{
    public Guid Id { get; init; }
    public string Location { get; init; } = string.Empty;
    public string? HandedToName { get; init; }
    public Guid? HandedToEntityId { get; init; }
    public DateTimeOffset? HandedAt { get; init; }
    public DateTimeOffset? ReceivedAt { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Worker state history entry.
/// </summary>
public record WorkerStateHistoryDto
{
    public Guid Id { get; init; }
    public string FromStatus { get; init; } = string.Empty;
    public string ToStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public Guid? TriggeredByUserId { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

/// <summary>
/// Nationality-based pricing.
/// </summary>
public record NationalityPricingDto
{
    public Guid Id { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public DateTimeOffset EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
}
