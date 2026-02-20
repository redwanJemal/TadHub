using System.Linq.Expressions;
using Identity.Contracts;
using Identity.Contracts.DTOs;
using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Identity.Core.Services;

/// <summary>
/// Service for managing platform admin users.
/// </summary>
public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _db;
    private readonly IIdentityService _identityService;
    private readonly IClock _clock;
    private readonly ILogger<AdminUserService> _logger;

    /// <summary>
    /// Fields available for sorting in list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<AdminUser, object>>> SortableFields = new()
    {
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["email"] = x => x.User.Email,
        ["firstName"] = x => x.User.FirstName,
        ["lastName"] = x => x.User.LastName
    };

    public AdminUserService(
        AppDbContext db,
        IIdentityService identityService,
        IClock clock,
        ILogger<AdminUserService> logger)
    {
        _db = db;
        _identityService = identityService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<AdminUserDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<AdminUser>()
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.User.Email.ToLower().Contains(searchLower) ||
                x.User.FirstName.ToLower().Contains(searchLower) ||
                x.User.LastName.ToLower().Contains(searchLower));
        }

        // Apply filters
        var isSuperAdminFilter = qp.Filters.FirstOrDefault(f => f.Name == "isSuperAdmin");
        if (isSuperAdminFilter != null && isSuperAdminFilter.Values.Count > 0)
        {
            if (bool.TryParse(isSuperAdminFilter.Values[0], out var isSuperAdmin))
            {
                query = query.Where(x => x.IsSuperAdmin == isSuperAdmin);
            }
        }

        // Apply sorting
        var sortFields = qp.GetSortFields();
        if (sortFields.Count > 0)
        {
            var firstSort = sortFields.First();
            if (SortableFields.TryGetValue(firstSort.Name, out var sortExpr))
            {
                query = firstSort.Descending
                    ? query.OrderByDescending(sortExpr)
                    : query.OrderBy(sortExpr);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((qp.Page - 1) * qp.PageSize)
            .Take(qp.PageSize)
            .Select(x => MapToDto(x))
            .ToListAsync(ct);

        return new PagedList<AdminUserDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    public async Task<Result<AdminUserDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var adminUser = await _db.Set<AdminUser>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (adminUser is null)
            return Result<AdminUserDto>.NotFound($"Admin user with ID {id} not found");

        return Result<AdminUserDto>.Success(MapToDto(adminUser));
    }

    public async Task<Result<AdminUserDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var adminUser = await _db.Set<AdminUser>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (adminUser is null)
            return Result<AdminUserDto>.NotFound($"Admin user with user ID {userId} not found");

        return Result<AdminUserDto>.Success(MapToDto(adminUser));
    }

    public async Task<Result<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken ct = default)
    {
        // Find user profile by email
        var userResult = await _identityService.GetByEmailAsync(request.Email, ct);
        
        UserProfile? userProfile;
        
        if (!userResult.IsSuccess)
        {
            // User doesn't exist - for now, return an error
            // In the future, we could create the user in Keycloak here
            return Result<AdminUserDto>.NotFound(
                $"User with email {request.Email} not found. The user must log in at least once before being made an admin.");
        }
        
        // Get the user profile entity
        userProfile = await _db.Set<UserProfile>()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower(), ct);

        if (userProfile is null)
            return Result<AdminUserDto>.NotFound($"User profile not found");

        // Check if already an admin
        var existingAdmin = await _db.Set<AdminUser>()
            .AnyAsync(x => x.UserId == userProfile.Id, ct);

        if (existingAdmin)
            return Result<AdminUserDto>.Conflict($"User {request.Email} is already a platform admin");

        // Create admin user record
        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            UserId = userProfile.Id,
            IsSuperAdmin = request.IsSuperAdmin,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow
        };

        _db.Set<AdminUser>().Add(adminUser);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created admin user {AdminId} for user {UserId} ({Email}), IsSuperAdmin: {IsSuperAdmin}",
            adminUser.Id, userProfile.Id, request.Email, request.IsSuperAdmin);

        // Load the user for the DTO
        adminUser.User = userProfile;

        return Result<AdminUserDto>.Success(MapToDto(adminUser));
    }

    public async Task<Result<AdminUserDto>> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken ct = default)
    {
        var adminUser = await _db.Set<AdminUser>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (adminUser is null)
            return Result<AdminUserDto>.NotFound($"Admin user with ID {id} not found");

        // Apply updates
        if (request.IsSuperAdmin.HasValue)
            adminUser.IsSuperAdmin = request.IsSuperAdmin.Value;

        adminUser.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated admin user {AdminId}, IsSuperAdmin: {IsSuperAdmin}",
            adminUser.Id, adminUser.IsSuperAdmin);

        return Result<AdminUserDto>.Success(MapToDto(adminUser));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var adminUser = await _db.Set<AdminUser>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (adminUser is null)
            return Result.NotFound($"Admin user with ID {id} not found");

        // Prevent removing the last super admin
        if (adminUser.IsSuperAdmin)
        {
            var superAdminCount = await _db.Set<AdminUser>()
                .CountAsync(x => x.IsSuperAdmin, ct);

            if (superAdminCount <= 1)
                return Result.ValidationError("Cannot remove the last super admin");
        }

        _db.Set<AdminUser>().Remove(adminUser);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Removed admin status from user {UserId} ({Email})",
            adminUser.UserId, adminUser.User.Email);

        return Result.Success();
    }

    public async Task<bool> IsAdminAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<AdminUser>()
            .AnyAsync(x => x.UserId == userId, ct);
    }

    public async Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<AdminUser>()
            .AnyAsync(x => x.UserId == userId && x.IsSuperAdmin, ct);
    }

    private static AdminUserDto MapToDto(AdminUser adminUser) => new()
    {
        Id = adminUser.Id,
        UserId = adminUser.UserId,
        User = new UserProfileDto
        {
            Id = adminUser.User.Id,
            KeycloakId = adminUser.User.KeycloakId,
            Email = adminUser.User.Email,
            FirstName = adminUser.User.FirstName,
            LastName = adminUser.User.LastName,
            AvatarUrl = adminUser.User.AvatarUrl,
            Phone = adminUser.User.Phone,
            Locale = adminUser.User.Locale,
            DefaultTenantId = adminUser.User.DefaultTenantId,
            IsActive = adminUser.User.IsActive,
            LastLoginAt = adminUser.User.LastLoginAt,
            CreatedAt = adminUser.User.CreatedAt,
            UpdatedAt = adminUser.User.UpdatedAt
        },
        IsSuperAdmin = adminUser.IsSuperAdmin,
        CreatedAt = adminUser.CreatedAt,
        UpdatedAt = adminUser.UpdatedAt
    };
}
