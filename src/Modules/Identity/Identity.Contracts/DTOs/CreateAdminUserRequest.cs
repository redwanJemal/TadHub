namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to create a new platform staff member.
/// </summary>
public sealed record CreatePlatformStaffRequest
{
    /// <summary>
    /// Email of the user to make platform staff.
    /// If the user doesn't exist, they'll be looked up in Keycloak.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Platform role: "super-admin", "admin", "finance", "sales", "support"
    /// </summary>
    public string Role { get; init; } = "admin";

    /// <summary>
    /// Optional department/notes for organizational clarity.
    /// </summary>
    public string? Department { get; init; }
}
