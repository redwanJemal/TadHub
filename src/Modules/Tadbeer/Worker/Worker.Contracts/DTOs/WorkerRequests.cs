namespace Worker.Contracts.DTOs;

/// <summary>
/// Request to create a new worker.
/// </summary>
public record CreateWorkerRequest
{
    #region Identity
    
    /// <summary>
    /// Passport number (required, unique per tenant).
    /// </summary>
    public string PassportNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// UAE Emirates ID (optional, assigned after visa).
    /// </summary>
    public string? EmiratesId { get; init; }
    
    /// <summary>
    /// CV serial number (agency identifier).
    /// </summary>
    public string? CvSerial { get; init; }
    
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
    /// Nationality (required).
    /// </summary>
    public string Nationality { get; init; } = string.Empty;
    
    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly DateOfBirth { get; init; }
    
    /// <summary>
    /// Gender: Female, Male.
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
    /// Education level.
    /// </summary>
    public string Education { get; init; } = string.Empty;
    
    /// <summary>
    /// Years of experience.
    /// </summary>
    public int? YearsOfExperience { get; init; }
    
    #endregion
    
    #region Job & Pricing
    
    /// <summary>
    /// Job category ID.
    /// </summary>
    public Guid JobCategoryId { get; init; }
    
    /// <summary>
    /// Monthly base salary in AED.
    /// </summary>
    public decimal MonthlyBaseSalary { get; init; }
    
    /// <summary>
    /// Available for flexible (hourly) bookings.
    /// </summary>
    public bool IsAvailableForFlexible { get; init; }
    
    #endregion
    
    #region Media URLs
    
    /// <summary>
    /// Primary photo URL.
    /// </summary>
    public string? PhotoUrl { get; init; }
    
    /// <summary>
    /// Video introduction URL.
    /// </summary>
    public string? VideoUrl { get; init; }
    
    #endregion
    
    /// <summary>
    /// Initial skills to add.
    /// </summary>
    public List<CreateWorkerSkillRequest>? Skills { get; init; }
    
    /// <summary>
    /// Languages spoken.
    /// </summary>
    public List<CreateWorkerLanguageRequest>? Languages { get; init; }
    
    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request to add a skill to a worker.
/// </summary>
public record CreateWorkerSkillRequest
{
    /// <summary>
    /// Skill name: Cooking, Cleaning, Childcare, etc.
    /// </summary>
    public string SkillName { get; init; } = string.Empty;
    
    /// <summary>
    /// Rating 0-100.
    /// </summary>
    public int Rating { get; init; }
}

/// <summary>
/// Request to add a language to a worker.
/// </summary>
public record CreateWorkerLanguageRequest
{
    /// <summary>
    /// Language (e.g., "English", "Arabic", "Tagalog").
    /// </summary>
    public string Language { get; init; } = string.Empty;
    
    /// <summary>
    /// Proficiency: Poor, Fair, Fluent.
    /// </summary>
    public string Proficiency { get; init; } = string.Empty;
}

/// <summary>
/// Request to update a worker's CV details.
/// </summary>
public record UpdateWorkerRequest
{
    public string? FullNameEn { get; init; }
    public string? FullNameAr { get; init; }
    public string? EmiratesId { get; init; }
    public string? Religion { get; init; }
    public string? MaritalStatus { get; init; }
    public int? NumberOfChildren { get; init; }
    public string? Education { get; init; }
    public int? YearsOfExperience { get; init; }
    public Guid? JobCategoryId { get; init; }
    public decimal? MonthlyBaseSalary { get; init; }
    public bool? IsAvailableForFlexible { get; init; }
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to transition a worker to a new state.
/// </summary>
public record WorkerStateTransitionRequest
{
    /// <summary>
    /// Target state to transition to.
    /// </summary>
    public string TargetState { get; init; } = string.Empty;
    
    /// <summary>
    /// Reason for the transition.
    /// </summary>
    public string? Reason { get; init; }
    
    /// <summary>
    /// Related entity ID (e.g., contractId for Hired, medicalReportId for MedicallyUnfit).
    /// </summary>
    public Guid? RelatedEntityId { get; init; }
}

/// <summary>
/// Request to transfer passport custody.
/// </summary>
public record TransferPassportRequest
{
    /// <summary>
    /// New passport location.
    /// </summary>
    public string Location { get; init; } = string.Empty;
    
    /// <summary>
    /// Name of person/entity receiving the passport.
    /// </summary>
    public string? HandedToName { get; init; }
    
    /// <summary>
    /// Entity ID (e.g., clientId, immigrationFileId).
    /// </summary>
    public Guid? HandedToEntityId { get; init; }
    
    /// <summary>
    /// Notes about the transfer.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request to add media to a worker.
/// </summary>
public record AddWorkerMediaRequest
{
    /// <summary>
    /// Media type: Photo, Video, Document.
    /// </summary>
    public string MediaType { get; init; } = string.Empty;
    
    /// <summary>
    /// File URL.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Whether this should be the primary photo/video.
    /// </summary>
    public bool IsPrimary { get; init; }
}

/// <summary>
/// Request to create/update nationality pricing.
/// </summary>
public record CreateNationalityPricingRequest
{
    /// <summary>
    /// Nationality (e.g., "Philippines", "Indonesia").
    /// </summary>
    public string Nationality { get; init; } = string.Empty;
    
    /// <summary>
    /// Contract type: Traditional, Temporary, Flexible.
    /// </summary>
    public string ContractType { get; init; } = string.Empty;
    
    /// <summary>
    /// Price amount in AED.
    /// </summary>
    public decimal Amount { get; init; }
    
    /// <summary>
    /// When this pricing becomes effective.
    /// </summary>
    public DateTimeOffset EffectiveFrom { get; init; }
    
    /// <summary>
    /// When this pricing expires (null = no expiry).
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; init; }
}
