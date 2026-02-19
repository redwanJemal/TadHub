using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a theme/template for a portal.
/// </summary>
public class PortalTheme : TenantScopedEntity
{
    /// <summary>
    /// The portal this theme belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// Theme name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the active theme.
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Base theme template: default, minimal, modern, corporate.
    /// </summary>
    public string BaseTemplate { get; set; } = "default";

    /// <summary>
    /// Theme variables/overrides (JSON).
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// Custom CSS overrides.
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Custom JavaScript.
    /// </summary>
    public string? CustomJs { get; set; }

    /// <summary>
    /// Header HTML template.
    /// </summary>
    public string? HeaderTemplate { get; set; }

    /// <summary>
    /// Footer HTML template.
    /// </summary>
    public string? FooterTemplate { get; set; }
}
