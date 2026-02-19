namespace Worker.Contracts.DTOs;

/// <summary>
/// Complex search criteria for worker matching.
/// Used by IWorkerSearchService for ranked results.
/// </summary>
public record WorkerSearchCriteria
{
    #region Basic Filters
    
    /// <summary>
    /// Filter by nationalities (array, OR logic).
    /// </summary>
    public List<string>? Nationalities { get; init; }
    
    /// <summary>
    /// Filter by job category IDs (array, OR logic).
    /// </summary>
    public List<Guid>? JobCategoryIds { get; init; }
    
    /// <summary>
    /// Filter by statuses (array, OR logic).
    /// Default: ReadyForMarket only.
    /// </summary>
    public List<string>? Statuses { get; init; }
    
    /// <summary>
    /// Filter by religions (array, OR logic).
    /// </summary>
    public List<string>? Religions { get; init; }
    
    /// <summary>
    /// Filter by gender.
    /// </summary>
    public string? Gender { get; init; }
    
    #endregion
    
    #region Range Filters
    
    /// <summary>
    /// Minimum age.
    /// </summary>
    public int? AgeMin { get; init; }
    
    /// <summary>
    /// Maximum age.
    /// </summary>
    public int? AgeMax { get; init; }
    
    /// <summary>
    /// Minimum monthly salary in AED.
    /// </summary>
    public decimal? SalaryMin { get; init; }
    
    /// <summary>
    /// Maximum monthly salary in AED.
    /// </summary>
    public decimal? SalaryMax { get; init; }
    
    /// <summary>
    /// Minimum years of experience.
    /// </summary>
    public int? ExperienceMin { get; init; }
    
    #endregion
    
    #region Skill Requirements
    
    /// <summary>
    /// Required skills with minimum ratings.
    /// </summary>
    public List<SkillRequirement>? Skills { get; init; }
    
    #endregion
    
    #region Language Requirements
    
    /// <summary>
    /// Required languages with minimum proficiency.
    /// </summary>
    public List<LanguageRequirement>? Languages { get; init; }
    
    #endregion
    
    #region Availability
    
    /// <summary>
    /// Filter to workers available for flexible bookings.
    /// </summary>
    public bool? AvailableForFlexible { get; init; }
    
    /// <summary>
    /// Include workers from shared pool agreements.
    /// Default: true.
    /// </summary>
    public bool IncludeSharedPool { get; init; } = true;
    
    #endregion
    
    #region Text Search
    
    /// <summary>
    /// Free text search (name, CV serial, notes).
    /// </summary>
    public string? SearchText { get; init; }
    
    #endregion
    
    #region Pagination & Sorting
    
    /// <summary>
    /// Page number (1-indexed).
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Sort by: relevance (default), salary, age, experience, createdAt.
    /// </summary>
    public string? SortBy { get; init; }
    
    /// <summary>
    /// Sort direction: asc, desc.
    /// </summary>
    public string SortDirection { get; init; } = "desc";
    
    #endregion
}

/// <summary>
/// Skill requirement with minimum rating.
/// </summary>
public record SkillRequirement
{
    /// <summary>
    /// Skill name: Cooking, Cleaning, Childcare, etc.
    /// </summary>
    public string SkillName { get; init; } = string.Empty;
    
    /// <summary>
    /// Minimum rating (0-100). Default: 0 (any rating).
    /// </summary>
    public int MinRating { get; init; }
}

/// <summary>
/// Language requirement with minimum proficiency.
/// </summary>
public record LanguageRequirement
{
    /// <summary>
    /// Language name.
    /// </summary>
    public string Language { get; init; } = string.Empty;
    
    /// <summary>
    /// Minimum proficiency: Poor, Fair, Fluent.
    /// </summary>
    public string MinProficiency { get; init; } = "Poor";
}

/// <summary>
/// Search result with relevance score.
/// </summary>
public record WorkerSearchResult
{
    /// <summary>
    /// Worker data.
    /// </summary>
    public WorkerDto Worker { get; init; } = null!;
    
    /// <summary>
    /// Relevance score (0-100).
    /// </summary>
    public decimal RelevanceScore { get; init; }
    
    /// <summary>
    /// Score breakdown by category.
    /// </summary>
    public ScoreBreakdown? Breakdown { get; init; }
}

/// <summary>
/// Score breakdown for transparency.
/// </summary>
public record ScoreBreakdown
{
    /// <summary>
    /// Nationality match score (0-20).
    /// </summary>
    public decimal NationalityScore { get; init; }
    
    /// <summary>
    /// Skill match score (0-30).
    /// </summary>
    public decimal SkillScore { get; init; }
    
    /// <summary>
    /// Language match score (0-20).
    /// </summary>
    public decimal LanguageScore { get; init; }
    
    /// <summary>
    /// Experience score (0-15).
    /// </summary>
    public decimal ExperienceScore { get; init; }
    
    /// <summary>
    /// Availability score (0-15).
    /// </summary>
    public decimal AvailabilityScore { get; init; }
}
