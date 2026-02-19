namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Tenant member data transfer object.
/// </summary>
public sealed record TenantUserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; init; }
    public TenantRole Role { get; init; }
    public DateTimeOffset JoinedAt { get; init; }
}

/// <summary>
/// Tenant user role enumeration.
/// </summary>
public enum TenantRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
