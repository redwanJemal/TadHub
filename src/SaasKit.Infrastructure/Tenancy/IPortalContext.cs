namespace SaasKit.Infrastructure.Tenancy;

/// <summary>
/// Provides access to the current portal context.
/// </summary>
public interface IPortalContext
{
    /// <summary>
    /// The current portal ID (null if not in a portal context).
    /// </summary>
    Guid? PortalId { get; }

    /// <summary>
    /// Whether the current request is in a portal context.
    /// </summary>
    bool IsPortalContext { get; }

    /// <summary>
    /// The current portal subdomain.
    /// </summary>
    string? Subdomain { get; }

    /// <summary>
    /// The current portal domain (if using custom domain).
    /// </summary>
    string? Domain { get; }
}
