using Microsoft.AspNetCore.Authorization;

namespace SaasKit.Infrastructure.Auth;

/// <summary>
/// Authorization attribute that requires specific permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates a new HasPermission attribute with the specified permission.
    /// </summary>
    /// <param name="permission">The required permission name (e.g., "tenancy.manage").</param>
    public HasPermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}")
    {
        Permission = permission;
    }

    /// <summary>
    /// The required permission name.
    /// </summary>
    public string Permission { get; }
}

/// <summary>
/// Authorization requirement for a specific permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
