namespace SaasKit.SharedKernel.Interfaces;

/// <summary>
/// Provides access to the current tenant context.
/// Populated by tenant resolution middleware from subdomain, header, or JWT claim.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The unique identifier of the current tenant.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// The URL-friendly slug of the current tenant.
    /// </summary>
    string TenantSlug { get; }

    /// <summary>
    /// Indicates whether a tenant has been successfully resolved for this request.
    /// </summary>
    bool IsResolved { get; }
}
