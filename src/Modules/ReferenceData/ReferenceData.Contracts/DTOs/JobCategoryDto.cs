namespace ReferenceData.Contracts.DTOs;

/// <summary>
/// MoHRE job category DTO.
/// 19 official categories defined by Ministry of Human Resources.
/// </summary>
public record JobCategoryDto
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// MoHRE official code.
    /// </summary>
    public string MoHRECode { get; init; } = string.Empty;

    /// <summary>
    /// Category name in English.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Category name in Arabic.
    /// </summary>
    public string NameAr { get; init; } = string.Empty;

    /// <summary>
    /// Whether this category is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; init; }
}

/// <summary>
/// Lightweight job category reference for dropdowns.
/// </summary>
public record JobCategoryRefDto
{
    public Guid Id { get; init; }
    public string MoHRECode { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
}
