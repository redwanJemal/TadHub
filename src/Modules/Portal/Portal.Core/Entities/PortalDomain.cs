using SaasKit.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a custom domain mapping for a portal.
/// </summary>
public class PortalDomain : TenantScopedEntity
{
    /// <summary>
    /// The portal this domain belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// The custom domain (e.g., "portal.acme.com").
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the primary domain.
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Whether the domain is verified.
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Verification token for DNS TXT record.
    /// </summary>
    public string? VerificationToken { get; set; }

    /// <summary>
    /// When the domain was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// SSL certificate status: pending, issued, failed.
    /// </summary>
    public string SslStatus { get; set; } = "pending";

    /// <summary>
    /// SSL certificate expiration date.
    /// </summary>
    public DateTimeOffset? SslExpiresAt { get; set; }
}
