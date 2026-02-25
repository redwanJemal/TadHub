using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Sse;

namespace TadHub.Api.Endpoints;

/// <summary>
/// SSE endpoint for real-time event streaming.
/// </summary>
public static class SseEndpoint
{
    private const int KeepaliveIntervalSeconds = 30;

    /// <summary>
    /// Maps the SSE endpoint.
    /// </summary>
    public static IEndpointRouteBuilder MapSseEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/events/stream", HandleSseConnection)
            .WithName("EventStream")
            .WithTags("Events")
            .RequireAuthorization()
            .Produces(200, contentType: "text/event-stream")
            .Produces(401);

        return endpoints;
    }

    [Authorize]
    private static async Task HandleSseConnection(
        HttpContext context,
        ISseConnectionManager connectionManager,
        CancellationToken cancellationToken)
    {
        // Extract user info from claims / middleware-resolved ID
        var userId = GetUserId(context);
        var tenantId = GetTenantId(context.User);

        if (userId == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Set SSE headers
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Connection = "keep-alive";
        context.Response.Headers["X-Accel-Buffering"] = "no"; // Disable nginx buffering

        // Create and register connection
        var connection = new SseConnection(context.Response, userId, tenantId);
        connectionManager.AddConnection(connection);

        try
        {
            // Send initial 'connected' event
            await connection.WriteEventAsync("connected", new
            {
                connectionId = connection.ConnectionId,
                userId = userId.ToString(),
                tenantId = tenantId.ToString(),
                connectedAt = connection.ConnectedAt
            }, cancellationToken: cancellationToken);

            // Keep connection alive with periodic heartbeats
            using var keepaliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(KeepaliveIntervalSeconds));
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await keepaliveTimer.WaitForNextTickAsync(cancellationToken);
                    await connection.WriteCommentAsync("keepalive", cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            // Cleanup on disconnect
            connectionManager.RemoveConnection(connection.ConnectionId);
            await connection.DisposeAsync();
        }
    }

    private static Guid GetUserId(HttpContext context)
    {
        // Use the internal user ID resolved by UserIdentityResolutionMiddleware
        if (context.Items.TryGetValue(UserIdentityResolutionMiddleware.InternalUserIdKey, out var resolved)
            && resolved is Guid internalId)
        {
            return internalId;
        }

        // Fallback to JWT sub claim
        var subClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst("sub");

        if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            return userId;

        return Guid.Empty;
    }

    private static Guid GetTenantId(ClaimsPrincipal user)
    {
        var tenantClaim = user.FindFirst("tenant_id") 
            ?? user.FindFirst("tenantId");
        
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            return tenantId;

        return Guid.Empty;
    }
}
