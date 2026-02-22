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
    /// Optional RBAC role ID to assign when invitation is accepted.
    /// Defaults to "Member" role if not specified.
    /// </summary>
    public Guid? RoleId { get; init; }
}
