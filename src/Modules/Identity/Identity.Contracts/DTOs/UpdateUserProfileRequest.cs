using System.ComponentModel.DataAnnotations;

namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to update an existing user profile.
/// All fields are optional - only non-null values are updated.
/// </summary>
public sealed record UpdateUserProfileRequest
{
    /// <summary>
    /// User's first name.
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; init; }

    /// <summary>
    /// User's last name.
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; init; }

    /// <summary>
    /// Avatar URL.
    /// </summary>
    [Url]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Phone number.
    /// </summary>
    [Phone]
    public string? Phone { get; init; }

    /// <summary>
    /// User's preferred locale.
    /// </summary>
    [MaxLength(10)]
    public string? Locale { get; init; }

    /// <summary>
    /// Default tenant ID.
    /// </summary>
    public Guid? DefaultTenantId { get; init; }
}
