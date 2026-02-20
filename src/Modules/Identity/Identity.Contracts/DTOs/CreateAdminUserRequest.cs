namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to create a new platform admin user.
/// </summary>
public sealed record CreateAdminUserRequest
{
    /// <summary>
    /// Email of the user to make an admin.
    /// If the user doesn't exist, they'll be looked up in Keycloak.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Whether this admin should have super admin privileges.
    /// </summary>
    public bool IsSuperAdmin { get; init; }
}
