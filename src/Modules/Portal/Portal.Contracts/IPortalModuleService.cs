using Portal.Contracts.DTOs;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace Portal.Contracts;

/// <summary>
/// Service for managing portals.
/// </summary>
public interface IPortalService
{
    /// <summary>
    /// Lists portals for a tenant with filtering.
    /// Supports filter[isActive]=true
    /// </summary>
    Task<PagedList<PortalDto>> GetPortalsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a portal by ID.
    /// </summary>
    Task<Result<PortalDto>> GetPortalByIdAsync(Guid tenantId, Guid portalId, CancellationToken ct = default);

    /// <summary>
    /// Gets a portal by subdomain.
    /// </summary>
    Task<Result<PortalDto>> GetPortalBySubdomainAsync(string subdomain, CancellationToken ct = default);

    /// <summary>
    /// Gets a portal by custom domain.
    /// </summary>
    Task<Result<PortalDto>> GetPortalByDomainAsync(string domain, CancellationToken ct = default);

    /// <summary>
    /// Creates a new portal.
    /// </summary>
    Task<Result<PortalDto>> CreatePortalAsync(Guid tenantId, CreatePortalRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a portal.
    /// </summary>
    Task<Result<PortalDto>> UpdatePortalAsync(Guid tenantId, Guid portalId, UpdatePortalRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a portal.
    /// </summary>
    Task<Result<bool>> DeletePortalAsync(Guid tenantId, Guid portalId, CancellationToken ct = default);

    /// <summary>
    /// Adds a custom domain to a portal.
    /// </summary>
    Task<Result<PortalDomainDto>> AddDomainAsync(Guid tenantId, Guid portalId, string domain, CancellationToken ct = default);

    /// <summary>
    /// Verifies a custom domain.
    /// </summary>
    Task<Result<PortalDomainDto>> VerifyDomainAsync(Guid tenantId, Guid portalId, Guid domainId, CancellationToken ct = default);

    /// <summary>
    /// Removes a custom domain.
    /// </summary>
    Task<Result<bool>> RemoveDomainAsync(Guid tenantId, Guid portalId, Guid domainId, CancellationToken ct = default);
}

/// <summary>
/// Service for managing portal users.
/// </summary>
public interface IPortalUserService
{
    /// <summary>
    /// Lists users for a portal with filtering.
    /// Supports filter[email][contains]=..., filter[isActive]=true
    /// </summary>
    Task<PagedList<PortalUserDto>> GetUsersAsync(Guid portalId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a portal user by ID.
    /// </summary>
    Task<Result<PortalUserDto>> GetUserByIdAsync(Guid portalId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a portal user by email.
    /// </summary>
    Task<Result<PortalUserDto>> GetUserByEmailAsync(Guid portalId, string email, CancellationToken ct = default);

    /// <summary>
    /// Creates a portal user (admin).
    /// </summary>
    Task<Result<PortalUserDto>> CreateUserAsync(Guid portalId, CreatePortalUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a portal user.
    /// </summary>
    Task<Result<PortalUserDto>> UpdateUserAsync(Guid portalId, Guid userId, UpdatePortalUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a portal user.
    /// </summary>
    Task<Result<bool>> DeleteUserAsync(Guid portalId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Registers a new portal user (public registration).
    /// </summary>
    Task<Result<PortalUserDto>> RegisterAsync(Guid portalId, PortalUserRegistrationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticates a portal user.
    /// </summary>
    Task<Result<PortalUserLoginResponse>> LoginAsync(Guid portalId, PortalUserLoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verifies a portal user's email.
    /// </summary>
    Task<Result<PortalUserDto>> VerifyEmailAsync(string token, CancellationToken ct = default);
}
