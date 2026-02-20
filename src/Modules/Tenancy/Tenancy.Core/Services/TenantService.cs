using System.Linq.Expressions;
using System.Security.Cryptography;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Extensions;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Services;

/// <summary>
/// Service for managing tenants and memberships.
/// </summary>
public class TenantService : ITenantService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<TenantService> _logger;

    /// <summary>
    /// Fields available for filtering tenants.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<Tenant, object>>> TenantFilters = new()
    {
        ["status"] = x => x.Status,
        ["name"] = x => x.Name,
        ["slug"] = x => x.Slug,
        ["createdAt"] = x => x.CreatedAt
    };

    /// <summary>
    /// Fields available for sorting tenants.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<Tenant, object>>> TenantSortable = new()
    {
        ["name"] = x => x.Name,
        ["slug"] = x => x.Slug,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt
    };

    /// <summary>
    /// Fields available for filtering members.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<TenantUser, object>>> MemberFilters = new()
    {
        ["role"] = x => x.Role,
        ["joinedAt"] = x => x.JoinedAt
    };

    /// <summary>
    /// Fields available for sorting members.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<TenantUser, object>>> MemberSortable = new()
    {
        ["role"] = x => x.Role,
        ["joinedAt"] = x => x.JoinedAt,
        ["createdAt"] = x => x.CreatedAt
    };

    public TenantService(
        AppDbContext db,
        IPublishEndpoint publisher,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<TenantService> logger)
    {
        _db = db;
        _publisher = publisher;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    #region Tenant Operations

    public async Task<Result<TenantDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .AsNoTracking()
            .Include(x => x.TenantType)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with ID {id} not found");

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<Result<TenantDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .AsNoTracking()
            .Include(x => x.TenantType)
            .FirstOrDefaultAsync(x => x.Slug == slug.ToLower(), ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with slug '{slug}' not found");

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<PagedList<TenantDto>> ListUserTenantsAsync(Guid userId, QueryParameters qp, CancellationToken ct = default)
    {
        // Get tenant IDs for this user first, then query tenants with Include
        var tenantIds = _db.Set<TenantUser>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.TenantId);

        var query = _db.Set<Tenant>()
            .AsNoTracking()
            .Include(x => x.TenantType)
            .Where(x => tenantIds.Contains(x.Id))
            .ApplyFilters(qp.Filters, TenantFilters)
            .ApplySort(qp.GetSortFields(), TenantSortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<TenantDto>> ListAllTenantsAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Tenant>()
            .AsNoTracking()
            .Include(x => x.TenantType)
            .ApplyFilters(qp.Filters, TenantFilters)
            .ApplySort(qp.GetSortFields(), TenantSortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        // Generate slug from name if not provided
        var slug = request.Slug?.ToLower() ?? request.Name.ToSlug();

        // Check for duplicate slug
        var slugExists = await _db.Set<Tenant>()
            .AnyAsync(x => x.Slug == slug, ct);

        if (slugExists)
            return Result<TenantDto>.Conflict($"Tenant with slug '{slug}' already exists");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            Status = TenantStatus.Active,
            TenantTypeId = request.TenantTypeId,
            LogoUrl = request.LogoUrl,
            Description = request.Description,
            Website = request.Website
        };

        _db.Set<Tenant>().Add(tenant);

        // Create owner membership for the current user
        var owner = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = _currentUser.UserId,
            Role = TenantRole.Owner,
            JoinedAt = _clock.UtcNow
        };

        _db.Set<TenantUser>().Add(owner);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created tenant {TenantId} ({Slug}) by user {UserId}",
            tenant.Id, tenant.Slug, _currentUser.UserId);

        // Publish domain event
        await _publisher.Publish(new TenantCreatedEvent(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            _currentUser.UserId,
            _clock.UtcNow), ct);

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<Result<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .Include(x => x.TenantType)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with ID {id} not found");

        // Apply updates
        if (request.Name is not null)
            tenant.Name = request.Name;
        if (request.TenantTypeId.HasValue)
            tenant.TenantTypeId = request.TenantTypeId;
        if (request.LogoUrl is not null)
            tenant.LogoUrl = request.LogoUrl;
        if (request.Description is not null)
            tenant.Description = request.Description;
        if (request.Website is not null)
            tenant.Website = request.Website;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated tenant {TenantId}", tenant.Id);

        await _publisher.Publish(new TenantUpdatedEvent(tenant.Id, tenant.Name, _clock.UtcNow), ct);

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<Result<bool>> SuspendAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>().FindAsync([id], ct);
        if (tenant is null)
            return Result<bool>.NotFound($"Tenant with ID {id} not found");

        if (tenant.Status == TenantStatus.Suspended)
            return Result<bool>.Success(true);

        tenant.Status = TenantStatus.Suspended;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Suspended tenant {TenantId}", tenant.Id);

        await _publisher.Publish(new TenantSuspendedEvent(tenant.Id, _clock.UtcNow), ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>().FindAsync([id], ct);
        if (tenant is null)
            return Result<bool>.NotFound($"Tenant with ID {id} not found");

        if (tenant.Status == TenantStatus.Active)
            return Result<bool>.Success(true);

        tenant.Status = TenantStatus.Active;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Reactivated tenant {TenantId}", tenant.Id);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>().FindAsync([id], ct);
        if (tenant is null)
            return Result<bool>.NotFound($"Tenant with ID {id} not found");

        tenant.Status = TenantStatus.Deleted;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted tenant {TenantId}", tenant.Id);

        await _publisher.Publish(new TenantDeletedEvent(tenant.Id, _clock.UtcNow), ct);

        return Result<bool>.Success(true);
    }

    #endregion

    #region Member Operations

    public async Task<PagedList<TenantUserDto>> GetMembersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<TenantUser>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.User)
            .ApplyFilters(qp.Filters, MemberFilters)
            .ApplySort(qp.GetSortFields(), MemberSortable);

        return await query
            .Select(x => MapMemberToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<TenantUserDto>> GetMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantUser>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (member is null)
            return Result<TenantUserDto>.NotFound("Member not found");

        return Result<TenantUserDto>.Success(MapMemberToDto(member));
    }

    public async Task<Result<TenantUserDto>> AddMemberAsync(Guid tenantId, Guid userId, TenantRole role, CancellationToken ct = default)
    {
        // Check if already a member
        var exists = await _db.Set<TenantUser>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (exists)
            return Result<TenantUserDto>.Conflict("User is already a member of this tenant");

        var member = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            JoinedAt = _clock.UtcNow
        };

        _db.Set<TenantUser>().Add(member);
        await _db.SaveChangesAsync(ct);

        // Reload with user
        var result = await GetMemberAsync(tenantId, userId, ct);
        return result;
    }

    public async Task<Result<TenantUserDto>> UpdateMemberRoleAsync(Guid tenantId, Guid userId, TenantRole role, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantUser>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (member is null)
            return Result<TenantUserDto>.NotFound("Member not found");

        // Prevent demoting the last owner
        if (member.Role == TenantRole.Owner && role != TenantRole.Owner)
        {
            var ownerCount = await _db.Set<TenantUser>()
                .CountAsync(x => x.TenantId == tenantId && x.Role == TenantRole.Owner, ct);

            if (ownerCount <= 1)
                return Result<TenantUserDto>.ValidationError("Cannot demote the last owner");
        }

        member.Role = role;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Updated role for user {UserId} in tenant {TenantId} to {Role}",
            userId, tenantId, role);

        return Result<TenantUserDto>.Success(MapMemberToDto(member));
    }

    public async Task<Result<bool>> RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantUser>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (member is null)
            return Result<bool>.NotFound("Member not found");

        // Prevent removing the last owner
        if (member.Role == TenantRole.Owner)
        {
            var ownerCount = await _db.Set<TenantUser>()
                .CountAsync(x => x.TenantId == tenantId && x.Role == TenantRole.Owner, ct);

            if (ownerCount <= 1)
                return Result<bool>.ValidationError("Cannot remove the last owner");
        }

        _db.Set<TenantUser>().Remove(member);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Removed user {UserId} from tenant {TenantId}",
            userId, tenantId);

        return Result<bool>.Success(true);
    }

    public async Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<TenantUser>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
    }

    public async Task<TenantRole?> GetUserRoleAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        return member?.Role;
    }

    #endregion

    #region Invitation Operations

    public async Task<Result<TenantInvitationDto>> InviteMemberAsync(Guid tenantId, InviteMemberRequest request, CancellationToken ct = default)
    {
        // Check if already a member
        var existingMember = await _db.Set<TenantUser>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.User.Email.ToLower() == request.Email.ToLower(), ct);

        if (existingMember is not null)
            return Result<TenantInvitationDto>.Conflict("User is already a member of this tenant");

        // Check for existing pending invitation
        var existingInvite = await _db.Set<TenantUserInvitation>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId 
                && x.Email.ToLower() == request.Email.ToLower()
                && x.AcceptedAt == null
                && x.ExpiresAt > _clock.UtcNow, ct);

        if (existingInvite is not null)
            return Result<TenantInvitationDto>.Conflict("An invitation is already pending for this email");

        var invitation = new TenantUserInvitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.Email.ToLower(),
            Role = request.Role,
            Token = GenerateInvitationToken(),
            ExpiresAt = _clock.UtcNow.AddDays(7),
            InvitedByUserId = _currentUser.UserId
        };

        _db.Set<TenantUserInvitation>().Add(invitation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created invitation for {Email} to tenant {TenantId}",
            request.Email, tenantId);

        // TODO: Send invitation email

        // Reload with tenant and user info
        var result = await _db.Set<TenantUserInvitation>()
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.InvitedBy)
            .FirstOrDefaultAsync(x => x.Id == invitation.Id, ct);

        return Result<TenantInvitationDto>.Success(MapInvitationToDto(result!));
    }

    public async Task<PagedList<TenantInvitationDto>> GetInvitationsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<TenantUserInvitation>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AcceptedAt == null)
            .Include(x => x.Tenant)
            .Include(x => x.InvitedBy)
            .OrderByDescending(x => x.CreatedAt);

        return await query
            .Select(x => MapInvitationToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<TenantUserDto>> AcceptInvitationAsync(string token, CancellationToken ct = default)
    {
        var invitation = await _db.Set<TenantUserInvitation>()
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Token == token, ct);

        if (invitation is null)
            return Result<TenantUserDto>.NotFound("Invitation not found");

        if (invitation.IsExpired)
            return Result<TenantUserDto>.ValidationError("Invitation has expired");

        if (invitation.IsAccepted)
            return Result<TenantUserDto>.ValidationError("Invitation has already been accepted");

        // Mark invitation as accepted
        invitation.AcceptedAt = _clock.UtcNow;

        // Add user as member
        var result = await AddMemberAsync(invitation.TenantId, _currentUser.UserId, invitation.Role, ct);

        if (!result.IsSuccess)
            return result;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} accepted invitation to tenant {TenantId}",
            _currentUser.UserId, invitation.TenantId);

        return result;
    }

    public async Task<Result<bool>> RevokeInvitationAsync(Guid tenantId, Guid invitationId, CancellationToken ct = default)
    {
        var invitation = await _db.Set<TenantUserInvitation>()
            .FirstOrDefaultAsync(x => x.Id == invitationId && x.TenantId == tenantId, ct);

        if (invitation is null)
            return Result<bool>.NotFound("Invitation not found");

        if (invitation.IsAccepted)
            return Result<bool>.ValidationError("Cannot revoke an accepted invitation");

        _db.Set<TenantUserInvitation>().Remove(invitation);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked invitation {InvitationId}", invitationId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<TenantInvitationDto>> GetInvitationByTokenAsync(string token, CancellationToken ct = default)
    {
        var invitation = await _db.Set<TenantUserInvitation>()
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.InvitedBy)
            .FirstOrDefaultAsync(x => x.Token == token, ct);

        if (invitation is null)
            return Result<TenantInvitationDto>.NotFound("Invitation not found");

        return Result<TenantInvitationDto>.Success(MapInvitationToDto(invitation));
    }

    #endregion

    #region Helpers

    private static string GenerateInvitationToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static TenantDto MapToDto(Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Slug = tenant.Slug,
        Status = tenant.Status,
        TenantTypeId = tenant.TenantTypeId,
        TenantTypeName = tenant.TenantType?.Name,
        LogoUrl = tenant.LogoUrl,
        Description = tenant.Description,
        Website = tenant.Website,
        CreatedAt = tenant.CreatedAt,
        UpdatedAt = tenant.UpdatedAt
    };

    private static TenantUserDto MapMemberToDto(TenantUser member) => new()
    {
        Id = member.Id,
        TenantId = member.TenantId,
        UserId = member.UserId,
        Email = member.User.Email,
        FirstName = member.User.FirstName,
        LastName = member.User.LastName,
        AvatarUrl = member.User.AvatarUrl,
        Role = member.Role,
        JoinedAt = member.JoinedAt
    };

    private static TenantInvitationDto MapInvitationToDto(TenantUserInvitation invitation) => new()
    {
        Id = invitation.Id,
        TenantId = invitation.TenantId,
        TenantName = invitation.Tenant.Name,
        Email = invitation.Email,
        Role = invitation.Role,
        Token = invitation.Token,
        ExpiresAt = invitation.ExpiresAt,
        AcceptedAt = invitation.AcceptedAt,
        InvitedByUserId = invitation.InvitedByUserId,
        InvitedByName = $"{invitation.InvitedBy.FirstName} {invitation.InvitedBy.LastName}".Trim(),
        CreatedAt = invitation.CreatedAt
    };

    #endregion
}
