using System.ComponentModel.DataAnnotations;

namespace Identity.Contracts.DTOs;

/// <summary>
/// Request to create a new user profile.
/// </summary>
public sealed record CreateUserProfileRequest
{
    /// <summary>
    /// Keycloak user ID (sub claim).
    /// </summary>
    [Required]
    public string KeycloakId { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

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
    /// Optional avatar URL.
    /// </summary>
    [Url]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Optional phone number.
    /// </summary>
    [Phone]
    public string? Phone { get; init; }

    /// <summary>
    /// User's preferred locale (default: "en").
    /// </summary>
    [MaxLength(10)]
    public string Locale { get; init; } = "en";

    /// <summary>
    /// Optional default tenant ID.
    /// </summary>
    public Guid? DefaultTenantId { get; init; }
}
