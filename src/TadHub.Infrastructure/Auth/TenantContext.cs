using Microsoft.AspNetCore.Http;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Auth;

/// <summary>
/// Implementation of ITenantContext that extracts tenant information from the request.
/// Tenant can be resolved from:
/// 1. HTTP header (X-Tenant-Id) - primary method, set by frontend
/// 2. JWT claim (tenant_id)
/// 3. Subdomain - via TenantResolutionMiddleware calling SetTenant()
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;
    private string? _tenantSlug;
    private bool _resolved;

    // Claim types for tenant information
    private const string TenantIdClaim = "tenant_id";
    private const string TenantSlugClaim = "tenant_slug";

    // HTTP header names
    private const string TenantIdHeader = "X-Tenant-Id";
    private const string TenantSlugHeader = "X-Tenant-Slug";

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid TenantId
    {
        get
        {
            EnsureResolved();
            return _tenantId ?? Guid.Empty;
        }
    }

    /// <inheritdoc />
    public string TenantSlug
    {
        get
        {
            EnsureResolved();
            return _tenantSlug ?? string.Empty;
        }
    }

    /// <inheritdoc />
    public bool IsResolved
    {
        get
        {
            EnsureResolved();
            return _tenantId.HasValue && _tenantId != Guid.Empty;
        }
    }

    private void EnsureResolved()
    {
        if (_resolved)
            return;

        _resolved = true;
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        // 1. Try HTTP header first (highest priority)
        if (httpContext.Request.Headers.TryGetValue(TenantIdHeader, out var headerTenantId))
        {
            if (Guid.TryParse(headerTenantId.FirstOrDefault(), out var tenantGuid))
            {
                _tenantId = tenantGuid;
            }
        }

        if (httpContext.Request.Headers.TryGetValue(TenantSlugHeader, out var headerTenantSlug))
        {
            _tenantSlug = headerTenantSlug.FirstOrDefault();
        }

        // 2. Try JWT claims
        if (!_tenantId.HasValue)
        {
            var claimTenantId = httpContext.User.FindFirst(TenantIdClaim)?.Value;
            if (!string.IsNullOrEmpty(claimTenantId) && Guid.TryParse(claimTenantId, out var claimGuid))
            {
                _tenantId = claimGuid;
            }
        }

        if (string.IsNullOrEmpty(_tenantSlug))
        {
            _tenantSlug = httpContext.User.FindFirst(TenantSlugClaim)?.Value;
        }

        // Tenant context must come from trusted sources only (header or subdomain).
        // Route params and query params are NOT used for tenant resolution.
    }

    /// <summary>
    /// Explicitly sets the tenant context. Used by middleware after resolution.
    /// </summary>
    public void SetTenant(Guid tenantId, string? slug = null)
    {
        _tenantId = tenantId;
        _tenantSlug = slug ?? string.Empty;
        _resolved = true;
    }
}
