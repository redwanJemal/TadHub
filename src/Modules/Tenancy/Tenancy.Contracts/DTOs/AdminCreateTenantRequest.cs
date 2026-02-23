using System.ComponentModel.DataAnnotations;

namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Request for a platform admin to create a new tenant with its owner user.
/// </summary>
public sealed record AdminCreateTenantRequest
{
    // ── Tenant fields ──

    /// <summary>
    /// Tenant display name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional custom slug (auto-generated from name if not provided).
    /// </summary>
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase alphanumeric with hyphens")]
    public string? Slug { get; init; }

    /// <summary>
    /// Optional logo URL.
    /// </summary>
    [Url]
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Optional description.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>
    /// Optional website URL.
    /// </summary>
    [Url]
    public string? Website { get; init; }

    // ── Owner user fields ──

    /// <summary>
    /// Email address for the tenant owner account.
    /// </summary>
    [Required]
    [EmailAddress]
    public string OwnerEmail { get; init; } = string.Empty;

    /// <summary>
    /// Password for the tenant owner account.
    /// </summary>
    [Required]
    [MinLength(8)]
    public string OwnerPassword { get; init; } = string.Empty;

    /// <summary>
    /// Owner's first name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string OwnerFirstName { get; init; } = string.Empty;

    /// <summary>
    /// Owner's last name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string OwnerLastName { get; init; } = string.Empty;
}
