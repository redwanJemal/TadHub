using System.ComponentModel.DataAnnotations;

namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Request to create a new user and add them as a tenant member directly (with password).
/// </summary>
public sealed record CreateMemberRequest
{
    /// <summary>
    /// Email address for the new user.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Password for the new user account.
    /// </summary>
    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Optional role ID to assign to the new member.
    /// </summary>
    public Guid? RoleId { get; init; }
}
