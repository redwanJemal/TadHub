namespace Authorization.Contracts.DTOs;

/// <summary>
/// Permission data transfer object.
/// </summary>
public sealed record PermissionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Module { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
