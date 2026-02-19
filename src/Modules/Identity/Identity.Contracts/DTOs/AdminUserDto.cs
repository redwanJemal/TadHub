namespace Identity.Contracts.DTOs;

/// <summary>
/// Admin user data transfer object.
/// </summary>
public sealed record AdminUserDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public UserProfileDto User { get; init; } = null!;
    public bool IsSuperAdmin { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
