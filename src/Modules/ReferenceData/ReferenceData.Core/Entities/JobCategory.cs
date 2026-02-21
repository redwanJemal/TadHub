using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Localization;

namespace ReferenceData.Core.Entities;

/// <summary>
/// MoHRE job category entity.
/// Global entity (not tenant-scoped).
/// 19 official categories defined by Ministry of Human Resources and Emiratisation.
/// </summary>
public class JobCategory : BaseEntity
{
    /// <summary>
    /// MoHRE official code (e.g., "DMW", "HSK", "COK").
    /// </summary>
    public string MoHRECode { get; set; } = string.Empty;

    /// <summary>
    /// Localized category name (en/ar).
    /// </summary>
    public LocalizedString Name { get; set; } = new();

    /// <summary>
    /// Whether this category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for UI.
    /// Common categories have lower values.
    /// </summary>
    public int DisplayOrder { get; set; }
}
