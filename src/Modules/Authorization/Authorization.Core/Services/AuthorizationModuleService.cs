using System.Linq.Expressions;
using System.Text.Json;
using Authorization.Contracts;
using Authorization.Contracts.DTOs;
using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Caching;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Authorization.Core.Services;

/// <summary>
/// Service for managing roles, permissions, and user authorization.
/// </summary>
public class AuthorizationModuleService : IAuthorizationModuleService
{
    private readonly AppDbContext _db;
    private readonly IRedisCacheService _cache;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<AuthorizationModuleService> _logger;

    private const string PermissionsCacheKeyPrefix = "permissions";
    private static readonly TimeSpan PermissionsCacheDuration = TimeSpan.FromMinutes(5);

    private static readonly Dictionary<string, Expression<Func<Permission, object>>> PermissionFilters = new()
    {
        ["name"] = x => x.Name,
        ["module"] = x => x.Module
    };

    private static readonly Dictionary<string, Expression<Func<Role, object>>> RoleFilters = new()
    {
        ["name"] = x => x.Name,
        ["isDefault"] = x => x.IsDefault,
        ["isSystem"] = x => x.IsSystem
    };

    public AuthorizationModuleService(
        AppDbContext db,
        IRedisCacheService cache,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<AuthorizationModuleService> logger)
    {
        _db = db;
        _cache = cache;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    #region Permission Operations

    public async Task<PagedList<PermissionDto>> GetPermissionsAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Permission>()
            .AsNoTracking()
            .ApplyFilters(qp.Filters, PermissionFilters)
            .OrderBy(x => x.Module)
            .ThenBy(x => x.DisplayOrder);

        return await query
            .Select(x => MapPermissionToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var permission = await _db.Set<Permission>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (permission is null)
            return Result<PermissionDto>.NotFound("Permission not found");

        return Result<PermissionDto>.Success(MapPermissionToDto(permission));
    }

    public async Task<Result<PermissionDto>> GetPermissionByNameAsync(string name, CancellationToken ct = default)
    {
        var permission = await _db.Set<Permission>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == name, ct);

        if (permission is null)
            return Result<PermissionDto>.NotFound("Permission not found");

        return Result<PermissionDto>.Success(MapPermissionToDto(permission));
    }

    #endregion

    #region Role Operations

    public async Task<PagedList<RoleDto>> GetRolesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Role>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .ApplyFilters(qp.Filters, RoleFilters)
            .OrderBy(x => x.DisplayOrder);

        var pagedRoles = await query.ToPagedListAsync(qp, ct);

        return new PagedList<RoleDto>(
            pagedRoles.Items.Select(MapRoleToDto).ToList(),
            pagedRoles.TotalCount,
            pagedRoles.Page,
            pagedRoles.PageSize);
    }

    public async Task<Result<RoleDto>> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .AsNoTracking()
            .Include(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId, ct);

        if (role is null)
            return Result<RoleDto>.NotFound("Role not found");

        return Result<RoleDto>.Success(MapRoleToDto(role));
    }

    public async Task<Result<RoleDto>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request, CancellationToken ct = default)
    {
        // Check for duplicate name
        var exists = await _db.Set<Role>()
            .AnyAsync(x => x.TenantId == tenantId && x.Name == request.Name, ct);

        if (exists)
            return Result<RoleDto>.Conflict($"Role '{request.Name}' already exists");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsDefault = false,
            IsSystem = false,
            DisplayOrder = 100
        };

        _db.Set<Role>().Add(role);

        // Add permissions
        if (request.PermissionIds.Count > 0)
        {
            foreach (var permissionId in request.PermissionIds)
            {
                _db.Set<RolePermission>().Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created role {RoleId} '{RoleName}' in tenant {TenantId}",
            role.Id, role.Name, tenantId);

        return await GetRoleByIdAsync(tenantId, role.Id, ct);
    }

    public async Task<Result<RoleDto>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId, ct);

        if (role is null)
            return Result<RoleDto>.NotFound("Role not found");

        if (role.IsSystem)
            return Result<RoleDto>.ValidationError("System roles cannot be modified");

        // Update fields
        if (request.Name is not null)
        {
            // Check for duplicate name
            var exists = await _db.Set<Role>()
                .AnyAsync(x => x.TenantId == tenantId && x.Name == request.Name && x.Id != roleId, ct);
            if (exists)
                return Result<RoleDto>.Conflict($"Role '{request.Name}' already exists");

            role.Name = request.Name;
        }

        if (request.Description is not null)
            role.Description = request.Description;

        // Update permissions
        if (request.PermissionIds is not null)
        {
            // Remove existing permissions
            _db.Set<RolePermission>().RemoveRange(role.Permissions);

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                _db.Set<RolePermission>().Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            // Invalidate cache for all users with this role
            await InvalidateRoleUsersCacheAsync(tenantId, roleId, ct);
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated role {RoleId} in tenant {TenantId}", roleId, tenantId);

        return await GetRoleByIdAsync(tenantId, roleId, ct);
    }

    public async Task<Result<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId, ct);

        if (role is null)
            return Result<bool>.NotFound("Role not found");

        if (role.IsSystem)
            return Result<bool>.ValidationError("System roles cannot be deleted");

        // Check if role is assigned to any users
        var hasUsers = await _db.Set<UserRole>()
            .AnyAsync(x => x.RoleId == roleId, ct);

        if (hasUsers)
            return Result<bool>.ValidationError("Cannot delete role that is assigned to users");

        _db.Set<Role>().Remove(role);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted role {RoleId} from tenant {TenantId}", roleId, tenantId);

        return Result<bool>.Success(true);
    }

    public async Task SeedDefaultRolesAsync(Guid tenantId, Guid ownerUserId, CancellationToken ct = default)
    {
        _logger.LogInformation("Seeding default roles for tenant {TenantId}", tenantId);

        var allPermissions = await _db.Set<Permission>().ToListAsync(ct);

        // Owner role - all permissions
        var ownerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Owner",
            Description = "Full access to all features",
            IsDefault = true,
            IsSystem = true,
            DisplayOrder = 1
        };
        _db.Set<Role>().Add(ownerRole);

        foreach (var permission in allPermissions)
        {
            _db.Set<RolePermission>().Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = ownerRole.Id,
                PermissionId = permission.Id
            });
        }

        // Admin role - most permissions except delete
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Admin",
            Description = "Administrative access",
            IsDefault = true,
            IsSystem = true,
            DisplayOrder = 2
        };
        _db.Set<Role>().Add(adminRole);

        var adminPermissions = allPermissions.Where(p => !p.Name.EndsWith(".delete") && p.Name != "tenancy.delete");
        foreach (var permission in adminPermissions)
        {
            _db.Set<RolePermission>().Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }

        // Member role - view permissions only
        var memberRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Member",
            Description = "Basic access",
            IsDefault = true,
            IsSystem = true,
            DisplayOrder = 3
        };
        _db.Set<Role>().Add(memberRole);

        var memberPermissions = allPermissions.Where(p => p.Name.EndsWith(".view"));
        foreach (var permission in memberPermissions)
        {
            _db.Set<RolePermission>().Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = memberRole.Id,
                PermissionId = permission.Id
            });
        }

        // Assign owner role to the creating user
        _db.Set<UserRole>().Add(new UserRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = ownerUserId,
            RoleId = ownerRole.Id,
            AssignedAt = _clock.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seeded default roles for tenant {TenantId} and assigned Owner role to user {UserId}",
            tenantId, ownerUserId);
    }

    #endregion

    #region User Role Operations

    public async Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var roles = await _db.Set<UserRole>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .Include(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .Select(x => x.Role)
            .ToListAsync(ct);

        return roles.Select(MapRoleToDto).ToList();
    }

    public async Task<Result<UserRoleDto>> AssignRoleAsync(Guid tenantId, AssignRoleRequest request, CancellationToken ct = default)
    {
        // Verify role exists and belongs to tenant
        var role = await _db.Set<Role>()
            .FirstOrDefaultAsync(x => x.Id == request.RoleId && x.TenantId == tenantId, ct);

        if (role is null)
            return Result<UserRoleDto>.NotFound("Role not found");

        // Check if already assigned
        var exists = await _db.Set<UserRole>()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == request.UserId && x.RoleId == request.RoleId, ct);

        if (exists)
            return Result<UserRoleDto>.Conflict("Role is already assigned to this user");

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = request.UserId,
            RoleId = request.RoleId,
            AssignedAt = _clock.UtcNow,
            AssignedByUserId = _currentUser.UserId
        };

        _db.Set<UserRole>().Add(userRole);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await InvalidateUserPermissionsCacheAsync(tenantId, request.UserId, ct);

        _logger.LogInformation(
            "Assigned role {RoleId} to user {UserId} in tenant {TenantId}",
            request.RoleId, request.UserId, tenantId);

        return Result<UserRoleDto>.Success(new UserRoleDto
        {
            Id = userRole.Id,
            TenantId = tenantId,
            UserId = request.UserId,
            RoleId = request.RoleId,
            RoleName = role.Name,
            AssignedAt = userRole.AssignedAt,
            AssignedByUserId = userRole.AssignedByUserId
        });
    }

    public async Task<Result<bool>> RemoveRoleAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var userRole = await _db.Set<UserRole>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId && x.RoleId == roleId, ct);

        if (userRole is null)
            return Result<bool>.NotFound("Role assignment not found");

        _db.Set<UserRole>().Remove(userRole);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await InvalidateUserPermissionsCacheAsync(tenantId, userId, ct);

        _logger.LogInformation(
            "Removed role {RoleId} from user {UserId} in tenant {TenantId}",
            roleId, userId, tenantId);

        return Result<bool>.Success(true);
    }

    #endregion

    #region Authorization Checks

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var cacheKey = $"{PermissionsCacheKeyPrefix}:{tenantId}:{userId}";

        // Try cache first
        var cached = await _cache.GetAsync<UserPermissionsDto>(cacheKey, ct);
        if (cached is not null)
            return cached;

        // Load from database
        var userRoles = await _db.Set<UserRole>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .Include(x => x.Role)
            .ThenInclude(x => x.Permissions)
            .ThenInclude(x => x.Permission)
            .ToListAsync(ct);

        var roles = userRoles.Select(x => x.Role.Name).Distinct().ToList();
        var permissions = userRoles
            .SelectMany(x => x.Role.Permissions)
            .Select(x => x.Permission.Name)
            .Distinct()
            .ToList();

        var result = new UserPermissionsDto
        {
            UserId = userId,
            TenantId = tenantId,
            Permissions = permissions,
            Roles = roles
        };

        // Cache the result
        await _cache.SetAsync(cacheKey, result, PermissionsCacheDuration, ct);

        return result;
    }

    public async Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken ct = default)
    {
        var userPermissions = await GetUserPermissionsAsync(tenantId, userId, ct);
        return userPermissions.Permissions.Contains(permission);
    }

    public async Task<bool> HasAnyPermissionAsync(Guid tenantId, Guid userId, params string[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(tenantId, userId);
        return permissions.Any(p => userPermissions.Permissions.Contains(p));
    }

    public async Task<bool> HasAllPermissionsAsync(Guid tenantId, Guid userId, params string[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(tenantId, userId);
        return permissions.All(p => userPermissions.Permissions.Contains(p));
    }

    public async Task InvalidateUserPermissionsCacheAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var cacheKey = $"{PermissionsCacheKeyPrefix}:{tenantId}:{userId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    private async Task InvalidateRoleUsersCacheAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        var userIds = await _db.Set<UserRole>()
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId)
            .Select(x => x.UserId)
            .ToListAsync(ct);

        foreach (var userId in userIds)
        {
            await InvalidateUserPermissionsCacheAsync(tenantId, userId, ct);
        }
    }

    #endregion

    #region Mappers

    private static PermissionDto MapPermissionToDto(Permission p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Module = p.Module,
        DisplayOrder = p.DisplayOrder
    };

    private static RoleDto MapRoleToDto(Role r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        Name = r.Name,
        Description = r.Description,
        IsDefault = r.IsDefault,
        IsSystem = r.IsSystem,
        DisplayOrder = r.DisplayOrder,
        Permissions = r.Permissions.Select(rp => MapPermissionToDto(rp.Permission)).ToList(),
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    #endregion
}
