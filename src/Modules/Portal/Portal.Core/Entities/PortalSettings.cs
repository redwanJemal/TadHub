using SaasKit.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Portal-specific settings and configuration.
/// </summary>
public class PortalSettings : TenantScopedEntity
{
    /// <summary>
    /// The portal these settings belong to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    #region General Settings

    /// <summary>
    /// Default language code (e.g., "en", "de").
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Timezone for the portal.
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Date format preference.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    #endregion

    #region Email Settings

    /// <summary>
    /// Support email address.
    /// </summary>
    public string? SupportEmail { get; set; }

    /// <summary>
    /// From email address for notifications.
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// From name for notifications.
    /// </summary>
    public string? FromName { get; set; }

    #endregion

    #region Social Links

    /// <summary>
    /// Twitter/X URL.
    /// </summary>
    public string? TwitterUrl { get; set; }

    /// <summary>
    /// LinkedIn URL.
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Facebook URL.
    /// </summary>
    public string? FacebookUrl { get; set; }

    /// <summary>
    /// Instagram URL.
    /// </summary>
    public string? InstagramUrl { get; set; }

    #endregion

    #region Legal

    /// <summary>
    /// Terms of service URL.
    /// </summary>
    public string? TermsUrl { get; set; }

    /// <summary>
    /// Privacy policy URL.
    /// </summary>
    public string? PrivacyUrl { get; set; }

    #endregion

    #region Analytics

    /// <summary>
    /// Google Analytics tracking ID.
    /// </summary>
    public string? GoogleAnalyticsId { get; set; }

    /// <summary>
    /// Custom tracking scripts (JSON array).
    /// </summary>
    public string? CustomTrackingScripts { get; set; }

    #endregion

    /// <summary>
    /// Additional settings (JSON).
    /// </summary>
    public string? AdditionalSettings { get; set; }
}
