namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to update a platform staff member.
/// </summary>
public sealed record UpdatePlatformStaffRequest
{
    /// <summary>
    /// Platform role: "super-admin", "admin", "finance", "sales", "support"
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Optional department/notes for organizational clarity.
    /// </summary>
    public string? Department { get; init; }
}
