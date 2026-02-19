namespace Identity.Contracts.DTOs;

/// <summary>
/// User profile data transfer object.
/// </summary>
public sealed record UserProfileDto
{
    public Guid Id { get; init; }
    public string KeycloakId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public string Locale { get; init; } = "en";
    public Guid? DefaultTenantId { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
