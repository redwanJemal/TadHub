using System.ComponentModel.DataAnnotations;

namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Request to update an existing tenant.
/// </summary>
public sealed record UpdateTenantRequest
{
    /// <summary>
    /// Tenant display name.
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; init; }

    /// <summary>
    /// Arabic name for bilingual support.
    /// </summary>
    [MaxLength(255)]
    public string? NameAr { get; init; }

    /// <summary>
    /// Logo URL.
    /// </summary>
    [Url]
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>
    /// Website URL.
    /// </summary>
    [Url]
    public string? Website { get; init; }
}
