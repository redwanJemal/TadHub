namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to update a platform admin user.
/// </summary>
public sealed record UpdateAdminUserRequest
{
    /// <summary>
    /// Whether this admin should have super admin privileges.
    /// </summary>
    public bool? IsSuperAdmin { get; init; }
}
