namespace SaasKit.Infrastructure.Tenancy;

/// <summary>
/// Implementation of IPortalContext that stores portal info per-request.
/// </summary>
public class PortalContext : IPortalContext
{
    /// <inheritdoc />
    public Guid? PortalId { get; set; }

    /// <inheritdoc />
    public bool IsPortalContext => PortalId.HasValue;

    /// <inheritdoc />
    public string? Subdomain { get; set; }

    /// <inheritdoc />
    public string? Domain { get; set; }
}
