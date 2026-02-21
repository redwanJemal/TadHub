namespace ReferenceData.Contracts.DTOs;

/// <summary>
/// Country reference data DTO.
/// ISO 3166-1 compliant with bilingual support.
/// </summary>
public record CountryDto
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ISO 3166-1 alpha-2 code (e.g., "AE", "US").
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 code (e.g., "ARE", "USA").
    /// </summary>
    public string Alpha3Code { get; init; } = string.Empty;

    /// <summary>
    /// Country name in English.
    /// </summary>
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Country name in Arabic.
    /// </summary>
    public string NameAr { get; init; } = string.Empty;

    /// <summary>
    /// Nationality adjective in English (e.g., "Emirati", "American").
    /// </summary>
    public string NationalityEn { get; init; } = string.Empty;

    /// <summary>
    /// Nationality adjective in Arabic (e.g., "إماراتي", "أمريكي").
    /// </summary>
    public string NationalityAr { get; init; } = string.Empty;

    /// <summary>
    /// Phone dialing code (e.g., "+971", "+1").
    /// </summary>
    public string DialingCode { get; init; } = string.Empty;

    /// <summary>
    /// Whether this country is active for selection.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Display order (lower = higher priority).
    /// Common Tadbeer nationalities are prioritized.
    /// </summary>
    public int DisplayOrder { get; init; }
}

/// <summary>
/// Lightweight country reference for dropdowns.
/// </summary>
public record CountryRefDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
}
