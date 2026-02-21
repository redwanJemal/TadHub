using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Localization;

namespace ReferenceData.Core.Entities;

/// <summary>
/// Country reference entity.
/// Global entity (not tenant-scoped).
/// ISO 3166-1 compliant with bilingual names and nationality adjectives.
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// ISO 3166-1 alpha-2 code (e.g., "AE", "US", "PH").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 code (e.g., "ARE", "USA", "PHL").
    /// </summary>
    public string Alpha3Code { get; set; } = string.Empty;

    /// <summary>
    /// Localized country name (en/ar).
    /// </summary>
    public LocalizedString Name { get; set; } = new();

    /// <summary>
    /// Localized nationality adjective (e.g., "Emirati/إماراتي").
    /// Used for worker nationality field.
    /// </summary>
    public LocalizedString Nationality { get; set; } = new();

    /// <summary>
    /// Phone dialing code with + prefix (e.g., "+971").
    /// </summary>
    public string DialingCode { get; set; } = string.Empty;

    /// <summary>
    /// Whether this country is active for selection.
    /// Inactive countries are hidden from dropdowns but preserved for historical data.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order (lower = higher priority).
    /// Common Tadbeer nationalities have lower values (0-10).
    /// </summary>
    public int DisplayOrder { get; set; } = 100;

    /// <summary>
    /// Whether this is a common Tadbeer source nationality.
    /// Used for quick filtering in worker registration.
    /// </summary>
    public bool IsCommonNationality { get; set; }
}
