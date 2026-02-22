namespace Identity.Contracts.DTOs;

/// <summary>
/// Platform staff data transfer object.
/// </summary>
public sealed record PlatformStaffDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public UserProfileDto User { get; init; } = null!;
    public string Role { get; init; } = "admin";
    public string? Department { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
