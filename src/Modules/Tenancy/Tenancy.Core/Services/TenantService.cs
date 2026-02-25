using System.Linq.Expressions;
using System.Security.Cryptography;
using Identity.Contracts;
using Identity.Contracts.DTOs;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Extensions;
using TadHub.SharedKernel.Interfaces;
using TadHub.Infrastructure.Keycloak;
using TadHub.Infrastructure.Keycloak.Models;
using TadHub.SharedKernel.Models;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;
using Authorization.Core.Entities;
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
    private readonly IIdentityService _identityService;
    private readonly IKeycloakAdminClient _keycloakAdmin;
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
    private static readonly Dictionary<string, Expression<Func<TenantMembership, object>>> MemberFilters = new()
    {
        ["isOwner"] = x => x.IsOwner,
        ["status"] = x => x.Status,
        ["joinedAt"] = x => x.JoinedAt
    };

    /// <summary>
    /// Fields available for sorting members.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<TenantMembership, object>>> MemberSortable = new()
    {
        ["joinedAt"] = x => x.JoinedAt,
        ["createdAt"] = x => x.CreatedAt
    };

    public TenantService(
        AppDbContext db,
        IPublishEndpoint publisher,
        ICurrentUser currentUser,
        IIdentityService identityService,
        IKeycloakAdminClient keycloakAdmin,
        IClock clock,
        ILogger<TenantService> logger)
    {
        _db = db;
        _publisher = publisher;
        _currentUser = currentUser;
        _identityService = identityService;
        _keycloakAdmin = keycloakAdmin;
        _clock = clock;
        _logger = logger;
    }

    #region Tenant Operations

    public async Task<Result<TenantDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with ID {id} not found");

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<Result<TenantDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug.ToLower(), ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with slug '{slug}' not found");

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<PagedList<TenantDto>> ListUserTenantsAsync(Guid userId, QueryParameters qp, CancellationToken ct = default)
    {
        // Get tenant IDs for this user first, then query tenants
        var tenantIds = _db.Set<TenantMembership>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.TenantId);

        var query = _db.Set<Tenant>()
            .AsNoTracking()
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
            .ApplyFilters(qp.Filters, TenantFilters)
            .ApplySort(qp.GetSortFields(), TenantSortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var internalUserId = _currentUser.UserId;

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
            LogoUrl = request.LogoUrl,
            Description = request.Description,
            Website = request.Website
        };

        _db.Set<Tenant>().Add(tenant);

        // Create owner membership for the current user
        var owner = new TenantMembership
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = internalUserId,
            IsOwner = true,
            Status = MembershipStatus.Active,
            JoinedAt = _clock.UtcNow
        };

        _db.Set<TenantMembership>().Add(owner);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created tenant {TenantId} ({Slug}) by user {UserId}",
            tenant.Id, tenant.Slug, internalUserId);

        // Publish domain event — triggers SeedDefaultRolesAsync via TenantCreatedConsumer
        await _publisher.Publish(new TenantCreatedEvent(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            internalUserId,
            _clock.UtcNow), ct);

        return Result<TenantDto>.Success(MapToDto(tenant));
    }

    public async Task<Result<TenantDto>> AdminCreateAsync(AdminCreateTenantRequest request, CancellationToken ct = default)
    {
        // 1. Check email uniqueness — local DB
        var existingLocal = await _identityService.GetByEmailAsync(request.OwnerEmail, ct);
        if (existingLocal.IsSuccess)
            return Result<TenantDto>.Conflict($"A user with email '{request.OwnerEmail}' already exists");

        // 1b. Check email uniqueness — Keycloak
        var existingKc = await _keycloakAdmin.GetUserByEmailAsync(request.OwnerEmail, ct);
        if (existingKc is not null)
            return Result<TenantDto>.Conflict($"A user with email '{request.OwnerEmail}' already exists in the identity provider");

        // 2. Validate slug uniqueness
        var slug = request.Slug?.ToLower() ?? request.Name.ToSlug();
        var slugExists = await _db.Set<Tenant>().AnyAsync(x => x.Slug == slug, ct);
        if (slugExists)
            return Result<TenantDto>.Conflict($"Tenant with slug '{slug}' already exists");

        // 3. Create Keycloak user
        string keycloakUserId;
        try
        {
            var kcUser = new KeycloakUserRepresentation
            {
                Username = request.OwnerEmail.ToLower(),
                Email = request.OwnerEmail.ToLower(),
                FirstName = request.OwnerFirstName,
                LastName = request.OwnerLastName,
                Enabled = true,
                EmailVerified = true,
                Credentials = new List<KeycloakCredentialRepresentation>
                {
                    new()
                    {
                        Type = "password",
                        Value = request.OwnerPassword,
                        Temporary = false
                    }
                }
            };

            keycloakUserId = await _keycloakAdmin.CreateUserAsync(kcUser, ct);
            _logger.LogInformation("Created Keycloak user {KeycloakUserId} for {Email}", keycloakUserId, request.OwnerEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Keycloak user for {Email}", request.OwnerEmail);
            return Result<TenantDto>.Failure("Failed to create user in identity provider", "KEYCLOAK_ERROR");
        }

        try
        {
            // 4. Create local UserProfile
            var profileResult = await _identityService.CreateAsync(new CreateUserProfileRequest
            {
                KeycloakId = keycloakUserId,
                Email = request.OwnerEmail.ToLower(),
                FirstName = request.OwnerFirstName,
                LastName = request.OwnerLastName
            }, ct);

            if (!profileResult.IsSuccess)
            {
                await RollbackKeycloakUserAsync(keycloakUserId);
                return Result<TenantDto>.Failure(
                    profileResult.Error ?? "Failed to create user profile",
                    "IDENTITY_ERROR");
            }

            var internalUserId = profileResult.Value!.Id;

            // 5. Create Tenant + owner TenantMembership
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = slug,
                Status = TenantStatus.Active,
                LogoUrl = request.LogoUrl,
                Description = request.Description,
                Website = request.Website
            };

            _db.Set<Tenant>().Add(tenant);

            var owner = new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = internalUserId,
                IsOwner = true,
                Status = MembershipStatus.Active,
                JoinedAt = _clock.UtcNow
            };

            _db.Set<TenantMembership>().Add(owner);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Admin-created tenant {TenantId} ({Slug}) with owner {UserId} (Keycloak: {KeycloakId})",
                tenant.Id, tenant.Slug, internalUserId, keycloakUserId);

            // 6. Publish domain event — triggers role seeding
            await _publisher.Publish(new TenantCreatedEvent(
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                internalUserId,
                _clock.UtcNow), ct);

            return Result<TenantDto>.Success(MapToDto(tenant));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant/profile for {Email}; rolling back Keycloak user {KeycloakUserId}",
                request.OwnerEmail, keycloakUserId);
            await RollbackKeycloakUserAsync(keycloakUserId);
            throw;
        }
    }

    public async Task<Result<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (tenant is null)
            return Result<TenantDto>.NotFound($"Tenant with ID {id} not found");

        // Apply updates
        if (request.Name is not null)
            tenant.Name = request.Name;
        if (request.NameAr is not null)
            tenant.NameAr = request.NameAr;
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

    public async Task<PagedList<TenantMemberDto>> GetMembersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<TenantMembership>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.User)
            .ApplyFilters(qp.Filters, MemberFilters)
            .ApplySort(qp.GetSortFields(), MemberSortable);

        var pagedMembers = await query.ToPagedListAsync(qp, ct);

        // Load roles for all members in the page in one query
        // IgnoreQueryFilters: bypass global tenant filter since we're explicitly filtering by tenantId
        var memberUserIds = pagedMembers.Items.Select(m => m.UserId).ToList();
        var userRoles = await _db.Set<UserRole>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(ur => ur.TenantId == tenantId && memberUserIds.Contains(ur.UserId))
            .Include(ur => ur.Role)
            .ToListAsync(ct);

        var rolesLookup = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ur => new MemberRoleInfo { Id = ur.RoleId, Name = ur.Role.Name }).ToList());

        return pagedMembers.Map(m => MapMemberToDto(m, rolesLookup));
    }

    public async Task<Result<TenantMemberDto>> GetMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantMembership>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (member is null)
            return Result<TenantMemberDto>.NotFound("Member not found");

        var roles = await _db.Set<UserRole>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(ur => ur.TenantId == tenantId && ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => new MemberRoleInfo { Id = ur.RoleId, Name = ur.Role.Name })
            .ToListAsync(ct);

        var rolesLookup = new Dictionary<Guid, List<MemberRoleInfo>> { [userId] = roles };
        return Result<TenantMemberDto>.Success(MapMemberToDto(member, rolesLookup));
    }

    public async Task<Result<TenantMemberDto>> AddMemberAsync(Guid tenantId, Guid userId, bool isOwner = false, CancellationToken ct = default)
    {
        // Check if already a member
        var exists = await _db.Set<TenantMembership>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (exists)
            return Result<TenantMemberDto>.Conflict("User is already a member of this tenant");

        var member = new TenantMembership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            IsOwner = isOwner,
            Status = MembershipStatus.Active,
            JoinedAt = _clock.UtcNow
        };

        _db.Set<TenantMembership>().Add(member);
        await _db.SaveChangesAsync(ct);

        // Reload with user
        var result = await GetMemberAsync(tenantId, userId, ct);
        return result;
    }

    public async Task<Result<bool>> RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.Set<TenantMembership>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

        if (member is null)
            return Result<bool>.NotFound("Member not found");

        // Prevent removing the last owner
        if (member.IsOwner)
        {
            var ownerCount = await _db.Set<TenantMembership>()
                .CountAsync(x => x.TenantId == tenantId && x.IsOwner, ct);

            if (ownerCount <= 1)
                return Result<bool>.ValidationError("Cannot remove the last owner");
        }

        _db.Set<TenantMembership>().Remove(member);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Removed user {UserId} from tenant {TenantId}",
            userId, tenantId);

        return Result<bool>.Success(true);
    }

    public async Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<TenantMembership>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
    }

    public async Task<bool> IsOwnerAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<TenantMembership>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == userId && x.IsOwner, ct);
    }

    #endregion

    #region Invitation Operations

    public async Task<Result<TenantInvitationDto>> InviteMemberAsync(Guid tenantId, InviteMemberRequest request, CancellationToken ct = default)
    {
        var internalUserId = _currentUser.UserId;

        // Check if already a member
        var existingMember = await _db.Set<TenantMembership>()
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
            DefaultRoleId = request.RoleId,
            Token = GenerateInvitationToken(),
            ExpiresAt = _clock.UtcNow.AddDays(7),
            InvitedByUserId = internalUserId
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

    public async Task<Result<TenantMemberDto>> AcceptInvitationAsync(string token, CancellationToken ct = default)
    {
        var invitation = await _db.Set<TenantUserInvitation>()
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Token == token, ct);

        if (invitation is null)
            return Result<TenantMemberDto>.NotFound("Invitation not found");

        if (invitation.IsExpired)
            return Result<TenantMemberDto>.ValidationError("Invitation has expired");

        if (invitation.IsAccepted)
            return Result<TenantMemberDto>.ValidationError("Invitation has already been accepted");

        // Mark invitation as accepted
        invitation.AcceptedAt = _clock.UtcNow;

        var internalUserId = _currentUser.UserId;

        // Add user as member (not owner)
        var result = await AddMemberAsync(invitation.TenantId, internalUserId, isOwner: false, ct);

        if (!result.IsSuccess)
            return result;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} accepted invitation to tenant {TenantId}",
            internalUserId, invitation.TenantId);

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

    private async Task RollbackKeycloakUserAsync(string keycloakUserId)
    {
        try
        {
            await _keycloakAdmin.DeleteUserAsync(keycloakUserId);
            _logger.LogInformation("Rolled back Keycloak user {KeycloakUserId}", keycloakUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback Keycloak user {KeycloakUserId}. Manual cleanup required.", keycloakUserId);
        }
    }

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
        NameAr = tenant.NameAr,
        Slug = tenant.Slug,
        Status = tenant.Status,
        LogoUrl = tenant.LogoUrl,
        Description = tenant.Description,
        Website = tenant.Website,
        CreatedAt = tenant.CreatedAt,
        UpdatedAt = tenant.UpdatedAt
    };

    private static TenantMemberDto MapMemberToDto(
        TenantMembership member,
        Dictionary<Guid, List<MemberRoleInfo>>? rolesLookup = null) => new()
    {
        Id = member.Id,
        TenantId = member.TenantId,
        UserId = member.UserId,
        Email = member.User.Email,
        FirstName = member.User.FirstName,
        LastName = member.User.LastName,
        AvatarUrl = member.User.AvatarUrl,
        IsOwner = member.IsOwner,
        Status = member.Status,
        Roles = rolesLookup?.GetValueOrDefault(member.UserId)?.AsReadOnly()
            ?? (IReadOnlyList<MemberRoleInfo>)[],
        JoinedAt = member.JoinedAt
    };

    private static TenantInvitationDto MapInvitationToDto(TenantUserInvitation invitation) => new()
    {
        Id = invitation.Id,
        TenantId = invitation.TenantId,
        TenantName = invitation.Tenant.Name,
        Email = invitation.Email,
        DefaultRoleId = invitation.DefaultRoleId,
        Token = invitation.Token,
        ExpiresAt = invitation.ExpiresAt,
        AcceptedAt = invitation.AcceptedAt,
        InvitedByUserId = invitation.InvitedByUserId,
        InvitedByName = $"{invitation.InvitedBy.FirstName} {invitation.InvitedBy.LastName}".Trim(),
        CreatedAt = invitation.CreatedAt
    };

    #endregion
}
