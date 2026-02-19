using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasKit.Infrastructure.Auth;
using SaasKit.Infrastructure.Persistence;

namespace SaasKit.Infrastructure.Tenancy;

/// <summary>
/// Middleware that resolves the current tenant from the request.
/// Resolution order: subdomain → X-Tenant-Id header → JWT claim → query param
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, AppDbContext db)
    {
        // Try to resolve tenant from subdomain first
        var slug = TryGetSlugFromSubdomain(context.Request.Host.Host);
        
        if (!string.IsNullOrEmpty(slug))
        {
            // Query tenant by slug using raw SQL to avoid circular dependency
            var tenantInfo = await db.Database
                .SqlQuery<TenantSlugInfo>($"SELECT id, slug FROM tenants WHERE slug = {slug} LIMIT 1")
                .FirstOrDefaultAsync(context.RequestAborted);
                
            if (tenantInfo is not null)
            {
                tenantContext.SetTenant(tenantInfo.Id, tenantInfo.Slug);
                _logger.LogDebug("Resolved tenant {TenantId} from subdomain {Slug}", tenantInfo.Id, slug);
            }
        }

        // If not resolved, TenantContext will try other methods (header, JWT, query)
        // when accessed

        await _next(context);
    }

    /// <summary>
    /// Extracts tenant slug from subdomain.
    /// Example: "acme.app.example.com" → "acme"
    /// </summary>
    private static string? TryGetSlugFromSubdomain(string host)
    {
        if (string.IsNullOrEmpty(host))
            return null;

        // Skip localhost and IPs
        if (host.StartsWith("localhost") || char.IsDigit(host[0]))
            return null;

        var parts = host.Split('.');
        
        // Need at least 3 parts for subdomain (e.g., acme.app.com)
        if (parts.Length < 3)
            return null;

        // Skip common non-tenant subdomains
        var subdomain = parts[0].ToLower();
        var skipSubdomains = new[] { "www", "api", "app", "admin", "staging", "dev", "test" };
        
        if (skipSubdomains.Contains(subdomain))
            return null;

        return subdomain;
    }

    /// <summary>
    /// Minimal DTO for tenant slug lookup.
    /// </summary>
    private sealed record TenantSlugInfo(Guid Id, string Slug);
}
