using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Contracts;
using Portal.Contracts.DTOs;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Portal.Core.Services;

/// <summary>
/// Service for managing portals.
/// </summary>
public class PortalService : IPortalService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PortalService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Portal, object>>> PortalFilters = new()
    {
        ["isActive"] = x => x.IsActive,
        ["name"] = x => x.Name,
        ["subdomain"] = x => x.Subdomain
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Portal, object>>> PortalSortable = new()
    {
        ["name"] = x => x.Name,
        ["createdAt"] = x => x.CreatedAt,
        ["subdomain"] = x => x.Subdomain
    };

    public PortalService(AppDbContext db, ILogger<PortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedList<PortalDto>> GetPortalsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Portal>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, PortalFilters)
            .ApplySort(qp.GetSortFields(), PortalSortable);

        var pagedPortals = await query.ToPagedListAsync(qp, ct);

        var portalIds = pagedPortals.Items.Select(p => p.Id).ToList();

        // Get user counts
        var userCounts = await _db.Set<Entities.PortalUser>()
            .Where(u => portalIds.Contains(u.PortalId))
            .GroupBy(u => u.PortalId)
            .Select(g => new { PortalId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PortalId, x => x.Count, ct);

        // Get page counts
        var pageCounts = await _db.Set<Entities.PortalPage>()
            .Where(p => portalIds.Contains(p.PortalId))
            .GroupBy(p => p.PortalId)
            .Select(g => new { PortalId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PortalId, x => x.Count, ct);

        return new PagedList<PortalDto>(
            pagedPortals.Items.Select(p => MapToDto(p,
                userCounts.GetValueOrDefault(p.Id, 0),
                pageCounts.GetValueOrDefault(p.Id, 0))).ToList(),
            pagedPortals.TotalCount,
            pagedPortals.Page,
            pagedPortals.PageSize);
    }

    public async Task<Result<PortalDto>> GetPortalByIdAsync(Guid tenantId, Guid portalId, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == portalId && x.TenantId == tenantId, ct);

        if (portal is null)
            return Result<PortalDto>.NotFound("Portal not found");

        var userCount = await _db.Set<Entities.PortalUser>().CountAsync(u => u.PortalId == portalId, ct);
        var pageCount = await _db.Set<Entities.PortalPage>().CountAsync(p => p.PortalId == portalId, ct);

        return Result<PortalDto>.Success(MapToDto(portal, userCount, pageCount));
    }

    public async Task<Result<PortalDto>> GetPortalBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .AsNoTracking()
            .IgnoreQueryFilters() // Subdomain lookup is cross-tenant
            .FirstOrDefaultAsync(x => x.Subdomain == subdomain.ToLowerInvariant(), ct);

        if (portal is null)
            return Result<PortalDto>.NotFound("Portal not found");

        return Result<PortalDto>.Success(MapToDto(portal, 0, 0));
    }

    public async Task<Result<PortalDto>> GetPortalByDomainAsync(string domain, CancellationToken ct = default)
    {
        var portalDomain = await _db.Set<Entities.PortalDomain>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(d => d.Portal)
            .FirstOrDefaultAsync(x => x.Domain == domain.ToLowerInvariant() && x.IsVerified, ct);

        if (portalDomain?.Portal is null)
            return Result<PortalDto>.NotFound("Portal not found");

        return Result<PortalDto>.Success(MapToDto(portalDomain.Portal, 0, 0));
    }

    public async Task<Result<PortalDto>> CreatePortalAsync(Guid tenantId, CreatePortalRequest request, CancellationToken ct = default)
    {
        var subdomain = request.Subdomain.ToLowerInvariant().Trim();

        // Validate subdomain format
        if (!IsValidSubdomain(subdomain))
            return Result<PortalDto>.ValidationError("Invalid subdomain format. Use lowercase letters, numbers, and hyphens only.");

        // Check subdomain uniqueness (globally)
        var exists = await _db.Set<Entities.Portal>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Subdomain == subdomain, ct);

        if (exists)
            return Result<PortalDto>.Conflict($"Subdomain '{subdomain}' is already taken");

        var portal = new Entities.Portal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Subdomain = subdomain,
            Description = request.Description,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AllowPublicRegistration = request.AllowPublicRegistration,
            IsActive = true
        };

        _db.Set<Entities.Portal>().Add(portal);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created portal {PortalId} '{PortalName}' with subdomain '{Subdomain}' for tenant {TenantId}",
            portal.Id, portal.Name, portal.Subdomain, tenantId);

        return Result<PortalDto>.Success(MapToDto(portal, 0, 0));
    }

    public async Task<Result<PortalDto>> UpdatePortalAsync(Guid tenantId, Guid portalId, UpdatePortalRequest request, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .FirstOrDefaultAsync(x => x.Id == portalId && x.TenantId == tenantId, ct);

        if (portal is null)
            return Result<PortalDto>.NotFound("Portal not found");

        if (request.Name is not null) portal.Name = request.Name;
        if (request.Description is not null) portal.Description = request.Description;
        if (request.IsActive.HasValue) portal.IsActive = request.IsActive.Value;
        if (request.PrimaryColor is not null) portal.PrimaryColor = request.PrimaryColor;
        if (request.SecondaryColor is not null) portal.SecondaryColor = request.SecondaryColor;
        if (request.LogoUrl is not null) portal.LogoUrl = request.LogoUrl;
        if (request.FaviconUrl is not null) portal.FaviconUrl = request.FaviconUrl;
        if (request.SeoTitle is not null) portal.SeoTitle = request.SeoTitle;
        if (request.SeoDescription is not null) portal.SeoDescription = request.SeoDescription;
        if (request.AllowPublicRegistration.HasValue) portal.AllowPublicRegistration = request.AllowPublicRegistration.Value;
        if (request.RequireEmailVerification.HasValue) portal.RequireEmailVerification = request.RequireEmailVerification.Value;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated portal {PortalId} for tenant {TenantId}", portalId, tenantId);

        var userCount = await _db.Set<Entities.PortalUser>().CountAsync(u => u.PortalId == portalId, ct);
        var pageCount = await _db.Set<Entities.PortalPage>().CountAsync(p => p.PortalId == portalId, ct);

        return Result<PortalDto>.Success(MapToDto(portal, userCount, pageCount));
    }

    public async Task<Result<bool>> DeletePortalAsync(Guid tenantId, Guid portalId, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .FirstOrDefaultAsync(x => x.Id == portalId && x.TenantId == tenantId, ct);

        if (portal is null)
            return Result<bool>.NotFound("Portal not found");

        _db.Set<Entities.Portal>().Remove(portal);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted portal {PortalId} for tenant {TenantId}", portalId, tenantId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<PortalDomainDto>> AddDomainAsync(Guid tenantId, Guid portalId, string domain, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .FirstOrDefaultAsync(x => x.Id == portalId && x.TenantId == tenantId, ct);

        if (portal is null)
            return Result<PortalDomainDto>.NotFound("Portal not found");

        domain = domain.ToLowerInvariant().Trim();

        // Check domain uniqueness
        var exists = await _db.Set<Entities.PortalDomain>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Domain == domain, ct);

        if (exists)
            return Result<PortalDomainDto>.Conflict($"Domain '{domain}' is already in use");

        var portalDomain = new Entities.PortalDomain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PortalId = portalId,
            Domain = domain,
            VerificationToken = GenerateVerificationToken(),
            IsVerified = false,
            IsPrimary = false
        };

        _db.Set<Entities.PortalDomain>().Add(portalDomain);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Added domain '{Domain}' to portal {PortalId}", domain, portalId);

        return Result<PortalDomainDto>.Success(MapDomainToDto(portalDomain));
    }

    public async Task<Result<PortalDomainDto>> VerifyDomainAsync(Guid tenantId, Guid portalId, Guid domainId, CancellationToken ct = default)
    {
        var domain = await _db.Set<Entities.PortalDomain>()
            .FirstOrDefaultAsync(x => x.Id == domainId && x.PortalId == portalId && x.TenantId == tenantId, ct);

        if (domain is null)
            return Result<PortalDomainDto>.NotFound("Domain not found");

        // TODO: Actually verify DNS record
        // For now, just mark as verified
        domain.IsVerified = true;
        domain.VerifiedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Verified domain '{Domain}' for portal {PortalId}", domain.Domain, portalId);

        return Result<PortalDomainDto>.Success(MapDomainToDto(domain));
    }

    public async Task<Result<bool>> RemoveDomainAsync(Guid tenantId, Guid portalId, Guid domainId, CancellationToken ct = default)
    {
        var domain = await _db.Set<Entities.PortalDomain>()
            .FirstOrDefaultAsync(x => x.Id == domainId && x.PortalId == portalId && x.TenantId == tenantId, ct);

        if (domain is null)
            return Result<bool>.NotFound("Domain not found");

        _db.Set<Entities.PortalDomain>().Remove(domain);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Removed domain '{Domain}' from portal {PortalId}", domain.Domain, portalId);

        return Result<bool>.Success(true);
    }

    private static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain) || subdomain.Length < 3 || subdomain.Length > 63)
            return false;

        // Must start and end with alphanumeric, can contain hyphens
        return System.Text.RegularExpressions.Regex.IsMatch(subdomain, @"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$");
    }

    private static string GenerateVerificationToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static PortalDto MapToDto(Entities.Portal p, int userCount, int pageCount) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        Name = p.Name,
        Subdomain = p.Subdomain,
        Description = p.Description,
        IsActive = p.IsActive,
        PrimaryColor = p.PrimaryColor,
        SecondaryColor = p.SecondaryColor,
        LogoUrl = p.LogoUrl,
        FaviconUrl = p.FaviconUrl,
        SeoTitle = p.SeoTitle,
        SeoDescription = p.SeoDescription,
        AllowPublicRegistration = p.AllowPublicRegistration,
        RequireEmailVerification = p.RequireEmailVerification,
        EnableSso = p.EnableSso,
        UserCount = userCount,
        PageCount = pageCount,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static PortalDomainDto MapDomainToDto(Entities.PortalDomain d) => new()
    {
        Id = d.Id,
        PortalId = d.PortalId,
        Domain = d.Domain,
        IsPrimary = d.IsPrimary,
        IsVerified = d.IsVerified,
        VerificationToken = d.VerificationToken,
        VerifiedAt = d.VerifiedAt,
        SslStatus = d.SslStatus
    };
}
