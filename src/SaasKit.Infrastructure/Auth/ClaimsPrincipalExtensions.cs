using System.Security.Claims;

namespace SaasKit.Infrastructure.Auth;

/// <summary>
/// Extension methods for extracting information from ClaimsPrincipal.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Standard claim types used by Keycloak and other OIDC providers.
    /// </summary>
    private static class ClaimTypes
    {
        public const string Sub = "sub";
        public const string Email = "email";
        public const string PreferredUsername = "preferred_username";
        public const string GivenName = "given_name";
        public const string FamilyName = "family_name";
        public const string Name = "name";
        public const string RealmAccess = "realm_access";
        public const string ResourceAccess = "resource_access";
    }

    /// <summary>
    /// Gets the user ID (sub claim) as a Guid.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subClaim = principal.FindFirst(ClaimTypes.Sub)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(subClaim))
            return Guid.Empty;

        // Try to parse as Guid (Keycloak uses UUID format)
        if (Guid.TryParse(subClaim, out var guid))
            return guid;

        // If not a valid Guid, generate deterministic Guid from string
        return GenerateDeterministicGuid(subClaim);
    }

    /// <summary>
    /// Gets the Keycloak ID (sub claim) as string.
    /// </summary>
    public static string GetKeycloakId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Sub)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? string.Empty;
    }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public static string GetFirstName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.GivenName)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value
            ?? string.Empty;
    }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public static string GetLastName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.FamilyName)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
            ?? string.Empty;
    }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public static string GetFullName(this ClaimsPrincipal principal)
    {
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(name))
            return name;

        var firstName = principal.GetFirstName();
        var lastName = principal.GetLastName();
        return $"{firstName} {lastName}".Trim();
    }

    /// <summary>
    /// Gets the user's roles from JWT claims.
    /// Handles both Keycloak realm_access format and standard role claims.
    /// </summary>
    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
    {
        var roles = new List<string>();

        // Standard role claims
        foreach (var claim in principal.FindAll(System.Security.Claims.ClaimTypes.Role))
        {
            roles.Add(claim.Value);
        }

        // Also check "role" claim (some providers use singular)
        foreach (var claim in principal.FindAll("role"))
        {
            if (!roles.Contains(claim.Value))
                roles.Add(claim.Value);
        }

        // Keycloak puts roles in realm_access.roles (as JSON)
        var realmAccess = principal.FindFirst(ClaimTypes.RealmAccess)?.Value;
        if (!string.IsNullOrEmpty(realmAccess))
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(realmAccess);
                if (doc.RootElement.TryGetProperty("roles", out var rolesArray))
                {
                    foreach (var role in rolesArray.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue) && !roles.Contains(roleValue))
                            roles.Add(roleValue);
                    }
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return roles.AsReadOnly();
    }

    /// <summary>
    /// Generates a deterministic Guid from a string.
    /// Used when the sub claim is not a valid Guid format.
    /// </summary>
    private static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
