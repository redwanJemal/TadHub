namespace Authorization.Contracts.DTOs;

/// <summary>
/// Role data transfer object.
/// </summary>
public sealed record RoleDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsSystem { get; init; }
    public bool IsCustom { get; init; }
    public Guid? TemplateId { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<PermissionDto> Permissions { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Request to create a new role.
/// </summary>
public sealed record CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<Guid> PermissionIds { get; init; } = [];
}

/// <summary>
/// Request to update a role.
/// </summary>
public sealed record UpdateRoleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<Guid>? PermissionIds { get; init; }
}
