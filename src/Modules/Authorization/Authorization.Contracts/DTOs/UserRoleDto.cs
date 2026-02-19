namespace Authorization.Contracts.DTOs;

/// <summary>
/// User role assignment data transfer object.
/// </summary>
public sealed record UserRoleDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; init; }
    public Guid? AssignedByUserId { get; init; }
}

/// <summary>
/// Request to assign a role to a user.
/// </summary>
public sealed record AssignRoleRequest
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
}

/// <summary>
/// Collection of user permissions for authorization checks.
/// </summary>
public sealed record UserPermissionsDto
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = [];
    public IReadOnlyList<string> Roles { get; init; } = [];
}
