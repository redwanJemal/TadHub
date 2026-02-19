using Microsoft.AspNetCore.Http;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Auth;

/// <summary>
/// Implementation of ICurrentUser that extracts user information from JWT claims.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private IReadOnlyList<string>? _roles;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid UserId => _httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;

    /// <inheritdoc />
    public string Email => _httpContextAccessor.HttpContext?.User.GetEmail() ?? string.Empty;

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            if (_roles is not null)
                return _roles;

            _roles = _httpContextAccessor.HttpContext?.User.GetRoles() ?? Array.Empty<string>();
            return _roles;
        }
    }

    /// <summary>
    /// Gets the Keycloak ID (sub claim) as string.
    /// </summary>
    public string KeycloakId => _httpContextAccessor.HttpContext?.User.GetKeycloakId() ?? string.Empty;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName => _httpContextAccessor.HttpContext?.User.GetFirstName() ?? string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName => _httpContextAccessor.HttpContext?.User.GetLastName() ?? string.Empty;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => _httpContextAccessor.HttpContext?.User.GetFullName() ?? string.Empty;

    /// <inheritdoc />
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(HasRole);
    }
}
