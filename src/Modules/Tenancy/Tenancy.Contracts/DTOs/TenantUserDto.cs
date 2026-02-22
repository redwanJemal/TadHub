namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Membership status for tenant members.
/// </summary>
public enum MembershipStatus
{
    Active = 0,
    Suspended = 1,
    Invited = 2
}

/// <summary>
/// Tenant member data transfer object.
/// </summary>
public sealed record TenantMemberDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; init; }
    public bool IsOwner { get; init; }
    public MembershipStatus Status { get; init; }
    public DateTimeOffset JoinedAt { get; init; }
}
