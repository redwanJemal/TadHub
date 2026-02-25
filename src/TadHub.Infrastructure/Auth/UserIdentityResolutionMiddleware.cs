using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace TadHub.Infrastructure.Auth;

/// <summary>
/// Middleware that resolves the Keycloak sub claim to the internal user_profiles.Id once per request.
/// Stores the result in HttpContext.Items["InternalUserId"] for consumption by CurrentUser and other services.
/// </summary>
public class UserIdentityResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserIdentityResolutionMiddleware> _logger;

    public const string InternalUserIdKey = "InternalUserId";

    public UserIdentityResolutionMiddleware(RequestDelegate next, ILogger<UserIdentityResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var keycloakId = context.User.GetKeycloakId();

            if (!string.IsNullOrEmpty(keycloakId))
            {
                var internalId = await db.Database
                    .SqlQuery<Guid?>($"SELECT id FROM user_profiles WHERE keycloak_id = {keycloakId} LIMIT 1")
                    .FirstOrDefaultAsync(context.RequestAborted);

                if (internalId.HasValue)
                {
                    context.Items[InternalUserIdKey] = internalId.Value;
                    _logger.LogDebug(
                        "Resolved Keycloak ID {KeycloakId} to internal user ID {InternalId}",
                        keycloakId, internalId.Value);
                }
                else
                {
                    _logger.LogDebug(
                        "No user profile found for Keycloak ID {KeycloakId} (JIT provisioning pending)",
                        keycloakId);
                }
            }
        }

        await _next(context);
    }
}
