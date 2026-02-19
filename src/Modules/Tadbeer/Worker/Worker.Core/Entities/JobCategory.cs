using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Localization;

namespace Worker.Core.Entities;

/// <summary>
/// Job category (MoHRE-defined).
/// Global entity (not tenant-scoped).
/// 19 official categories defined by MoHRE.
/// </summary>
public class JobCategory : BaseEntity
{
    /// <summary>
    /// Localized name (en/ar).
    /// </summary>
    public LocalizedString Name { get; set; } = new();

    /// <summary>
    /// MoHRE official code.
    /// </summary>
    public string MoHRECode { get; set; } = string.Empty;

    /// <summary>
    /// Whether this category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Workers in this category.
    /// </summary>
    public ICollection<Worker> Workers { get; set; } = new List<Worker>();

    #endregion
}
