using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SaasKit.Infrastructure.Tenancy;

/// <summary>
/// Middleware that resolves the current portal from subdomain or custom domain.
/// Should run after TenantResolutionMiddleware.
/// </summary>
public class PortalResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PortalResolutionMiddleware> _logger;

    // Known non-portal subdomains
    private static readonly HashSet<string> ReservedSubdomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "www", "api", "app", "admin", "portal", "mail", "smtp", "ftp",
        "cdn", "static", "assets", "images", "files", "docs", "help",
        "support", "status", "blog", "news"
    };

    public PortalResolutionMiddleware(RequestDelegate next, ILogger<PortalResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, PortalContext portalContext)
    {
        var host = context.Request.Host.Host;

        // Try to resolve portal from host
        var (subdomain, isCustomDomain) = ParseHost(host);

        if (!string.IsNullOrEmpty(subdomain) && !ReservedSubdomains.Contains(subdomain))
        {
            portalContext.Subdomain = subdomain;
            _logger.LogDebug("Resolved portal subdomain: {Subdomain}", subdomain);
        }
        else if (isCustomDomain)
        {
            portalContext.Domain = host;
            _logger.LogDebug("Resolved custom domain: {Domain}", host);
        }

        // Note: Actual portal ID resolution would need to query the database
        // This is typically done in a filter or service that has access to IPortalService
        // The middleware just extracts the subdomain/domain for later lookup

        await _next(context);
    }

    private static (string? Subdomain, bool IsCustomDomain) ParseHost(string host)
    {
        // Remove port if present
        var hostWithoutPort = host.Split(':')[0];

        // Check if it's a known base domain (e.g., portal.example.com)
        // In production, this would be configured
        var baseDomains = new[] { "portal.example.com", "localhost" };

        foreach (var baseDomain in baseDomains)
        {
            if (hostWithoutPort.EndsWith($".{baseDomain}", StringComparison.OrdinalIgnoreCase))
            {
                var subdomain = hostWithoutPort[..^(baseDomain.Length + 1)];
                // Handle nested subdomains (take the first part)
                var firstPart = subdomain.Split('.')[0];
                return (firstPart, false);
            }

            if (hostWithoutPort.Equals(baseDomain, StringComparison.OrdinalIgnoreCase))
            {
                return (null, false);
            }
        }

        // Not a known base domain - treat as custom domain
        return (null, true);
    }
}
