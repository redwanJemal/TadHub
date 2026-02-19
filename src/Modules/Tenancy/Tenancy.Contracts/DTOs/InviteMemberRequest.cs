using System.ComponentModel.DataAnnotations;

namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Request to invite a member to a tenant.
/// </summary>
public sealed record InviteMemberRequest
{
    /// <summary>
    /// Email address to send the invitation to.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Role to assign to the invited user.
    /// </summary>
    [Required]
    public TenantRole Role { get; init; } = TenantRole.Member;
}

/// <summary>
/// Request to update a member's role.
/// </summary>
public sealed record UpdateMemberRoleRequest
{
    /// <summary>
    /// New role for the member.
    /// </summary>
    [Required]
    public TenantRole Role { get; init; }
}
