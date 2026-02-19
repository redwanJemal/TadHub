using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a B2B2C portal created by a tenant.
/// Each tenant can create multiple portals with custom branding and subdomains.
/// </summary>
public class Portal : TenantScopedEntity
{
    /// <summary>
    /// Portal name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique subdomain (e.g., "acme" for acme.portal.example.com).
    /// </summary>
    public string Subdomain { get; set; } = string.Empty;

    /// <summary>
    /// Portal description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the portal is active and accessible.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Branding

    /// <summary>
    /// Primary brand color (hex).
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Secondary brand color (hex).
    /// </summary>
    public string? SecondaryColor { get; set; }

    /// <summary>
    /// Logo URL.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Favicon URL.
    /// </summary>
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// Custom CSS.
    /// </summary>
    public string? CustomCss { get; set; }

    #endregion

    #region SEO

    /// <summary>
    /// SEO title.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO description.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// SEO keywords.
    /// </summary>
    public string? SeoKeywords { get; set; }

    /// <summary>
    /// Open Graph image URL.
    /// </summary>
    public string? OgImageUrl { get; set; }

    #endregion

    #region Auth Configuration

    /// <summary>
    /// Whether public registration is allowed.
    /// </summary>
    public bool AllowPublicRegistration { get; set; } = true;

    /// <summary>
    /// Whether email verification is required.
    /// </summary>
    public bool RequireEmailVerification { get; set; } = true;

    /// <summary>
    /// Whether SSO is enabled for this portal.
    /// </summary>
    public bool EnableSso { get; set; } = false;

    /// <summary>
    /// SSO provider configuration (JSON).
    /// </summary>
    public string? SsoConfig { get; set; }

    #endregion

    #region Stripe Connect

    /// <summary>
    /// Stripe Connect Account ID for portal monetization.
    /// </summary>
    public string? StripeAccountId { get; set; }

    /// <summary>
    /// Whether Stripe Connect onboarding is complete.
    /// </summary>
    public bool StripeOnboardingComplete { get; set; } = false;

    #endregion

    /// <summary>
    /// Custom domains for this portal.
    /// </summary>
    public ICollection<PortalDomain> Domains { get; set; } = new List<PortalDomain>();

    /// <summary>
    /// Portal users.
    /// </summary>
    public ICollection<PortalUser> Users { get; set; } = new List<PortalUser>();

    /// <summary>
    /// Portal pages.
    /// </summary>
    public ICollection<PortalPage> Pages { get; set; } = new List<PortalPage>();

    /// <summary>
    /// Portal settings.
    /// </summary>
    public PortalSettings? Settings { get; set; }
}
