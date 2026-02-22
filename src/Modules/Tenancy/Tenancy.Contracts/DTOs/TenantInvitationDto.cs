namespace Tenancy.Contracts.DTOs;

/// <summary>
/// Tenant invitation data transfer object.
/// </summary>
public sealed record TenantInvitationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid? DefaultRoleId { get; init; }
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? AcceptedAt { get; init; }
    public Guid InvitedByUserId { get; init; }
    public string InvitedByName { get; init; } = string.Empty;
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsAccepted => AcceptedAt.HasValue;
    public DateTimeOffset CreatedAt { get; init; }
}
