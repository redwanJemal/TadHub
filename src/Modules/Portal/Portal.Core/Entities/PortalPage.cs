using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a content page within a portal.
/// </summary>
public class PortalPage : TenantScopedEntity
{
    /// <summary>
    /// The portal this page belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// Page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL slug (e.g., "about-us").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Page content (HTML/Markdown).
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Content format: html, markdown.
    /// </summary>
    public string ContentFormat { get; set; } = "html";

    /// <summary>
    /// Whether the page is published.
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// When the page was published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Display order in navigation.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether to show in navigation.
    /// </summary>
    public bool ShowInNavigation { get; set; } = true;

    /// <summary>
    /// Page type: landing, content, legal, custom.
    /// </summary>
    public string PageType { get; set; } = "content";

    /// <summary>
    /// SEO title override.
    /// </summary>
    public string? SeoTitle { get; set; }

    /// <summary>
    /// SEO description override.
    /// </summary>
    public string? SeoDescription { get; set; }

    /// <summary>
    /// Featured image URL.
    /// </summary>
    public string? FeaturedImageUrl { get; set; }
}
