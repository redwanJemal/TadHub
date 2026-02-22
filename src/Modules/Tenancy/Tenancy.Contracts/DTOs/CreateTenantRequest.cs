using System.ComponentModel.DataAnnotations;

namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Request to create a new tenant.
/// </summary>
public sealed record CreateTenantRequest
{
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
}
