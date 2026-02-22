namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Tenant data transfer object.
/// </summary>
public sealed record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string Slug { get; init; } = string.Empty;
    public TenantStatus Status { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
    public string? Website { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Tenant status enumeration.
/// </summary>
public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Deleted = 2
}
