namespace SaasKit.SharedKernel.Interfaces;

/// <summary>
/// Interface for checking user permissions.
/// Implemented by the Authorization module, used by Infrastructure for policy checks.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks if a user has a specific permission in a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="permission">The permission name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the user has the permission.</returns>
    Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken ct = default);
}
