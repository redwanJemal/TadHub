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
/// Service for managing platform staff members.
/// </summary>
public class PlatformStaffService : IPlatformStaffService
{
    private readonly AppDbContext _db;
    private readonly IIdentityService _identityService;
    private readonly IClock _clock;
    private readonly ILogger<PlatformStaffService> _logger;

    /// <summary>
    /// Fields available for sorting in list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<PlatformStaff, object>>> SortableFields = new()
    {
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["email"] = x => x.User.Email,
        ["firstName"] = x => x.User.FirstName,
        ["lastName"] = x => x.User.LastName,
        ["role"] = x => x.Role
    };

    public PlatformStaffService(
        AppDbContext db,
        IIdentityService identityService,
        IClock clock,
        ILogger<PlatformStaffService> logger)
    {
        _db = db;
        _identityService = identityService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<PlatformStaffDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<PlatformStaff>()
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
        var roleFilter = qp.Filters.FirstOrDefault(f => f.Name == "role");
        if (roleFilter != null && roleFilter.Values.Count > 0)
        {
            var roleValue = roleFilter.Values[0];
            query = query.Where(x => x.Role == roleValue);
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

        return new PagedList<PlatformStaffDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    public async Task<Result<PlatformStaffDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var staff = await _db.Set<PlatformStaff>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (staff is null)
            return Result<PlatformStaffDto>.NotFound($"Platform staff with ID {id} not found");

        return Result<PlatformStaffDto>.Success(MapToDto(staff));
    }

    public async Task<Result<PlatformStaffDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var staff = await _db.Set<PlatformStaff>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (staff is null)
            return Result<PlatformStaffDto>.NotFound($"Platform staff with user ID {userId} not found");

        return Result<PlatformStaffDto>.Success(MapToDto(staff));
    }

    public async Task<Result<PlatformStaffDto>> CreateAsync(CreatePlatformStaffRequest request, CancellationToken ct = default)
    {
        // Find user profile by email
        var userResult = await _identityService.GetByEmailAsync(request.Email, ct);

        UserProfile? userProfile;

        if (!userResult.IsSuccess)
        {
            // User doesn't exist - for now, return an error
            // In the future, we could create the user in Keycloak here
            return Result<PlatformStaffDto>.NotFound(
                $"User with email {request.Email} not found. The user must log in at least once before being made platform staff.");
        }

        // Get the user profile entity
        userProfile = await _db.Set<UserProfile>()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email.ToLower(), ct);

        if (userProfile is null)
            return Result<PlatformStaffDto>.NotFound("User profile not found");

        // Check if already platform staff
        var existingStaff = await _db.Set<PlatformStaff>()
            .AnyAsync(x => x.UserId == userProfile.Id, ct);

        if (existingStaff)
            return Result<PlatformStaffDto>.Conflict($"User {request.Email} is already platform staff");

        // Create platform staff record
        var staff = new PlatformStaff
        {
            Id = Guid.NewGuid(),
            UserId = userProfile.Id,
            Role = request.Role,
            Department = request.Department,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow
        };

        _db.Set<PlatformStaff>().Add(staff);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created platform staff {StaffId} for user {UserId} ({Email}), Role: {Role}, Department: {Department}",
            staff.Id, userProfile.Id, request.Email, request.Role, request.Department);

        // Load the user for the DTO
        staff.User = userProfile;

        return Result<PlatformStaffDto>.Success(MapToDto(staff));
    }

    public async Task<Result<PlatformStaffDto>> UpdateAsync(Guid id, UpdatePlatformStaffRequest request, CancellationToken ct = default)
    {
        var staff = await _db.Set<PlatformStaff>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (staff is null)
            return Result<PlatformStaffDto>.NotFound($"Platform staff with ID {id} not found");

        // Apply updates
        if (request.Role is not null)
            staff.Role = request.Role;

        if (request.Department is not null)
            staff.Department = request.Department;

        staff.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated platform staff {StaffId}, Role: {Role}, Department: {Department}",
            staff.Id, staff.Role, staff.Department);

        return Result<PlatformStaffDto>.Success(MapToDto(staff));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var staff = await _db.Set<PlatformStaff>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (staff is null)
            return Result.NotFound($"Platform staff with ID {id} not found");

        // Prevent removing the last super-admin
        if (staff.Role == "super-admin")
        {
            var superAdminCount = await _db.Set<PlatformStaff>()
                .CountAsync(x => x.Role == "super-admin", ct);

            if (superAdminCount <= 1)
                return Result.ValidationError("Cannot remove the last super-admin");
        }

        _db.Set<PlatformStaff>().Remove(staff);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Removed platform staff status from user {UserId} ({Email})",
            staff.UserId, staff.User.Email);

        return Result.Success();
    }

    public async Task<bool> IsStaffAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<PlatformStaff>()
            .AnyAsync(x => x.UserId == userId, ct);
    }

    public async Task<bool> HasRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        return await _db.Set<PlatformStaff>()
            .AnyAsync(x => x.UserId == userId && x.Role == role, ct);
    }

    private static PlatformStaffDto MapToDto(PlatformStaff staff) => new()
    {
        Id = staff.Id,
        UserId = staff.UserId,
        User = new UserProfileDto
        {
            Id = staff.User.Id,
            KeycloakId = staff.User.KeycloakId,
            Email = staff.User.Email,
            FirstName = staff.User.FirstName,
            LastName = staff.User.LastName,
            AvatarUrl = staff.User.AvatarUrl,
            Phone = staff.User.Phone,
            Locale = staff.User.Locale,
            DefaultTenantId = staff.User.DefaultTenantId,
            IsActive = staff.User.IsActive,
            LastLoginAt = staff.User.LastLoginAt,
            CreatedAt = staff.User.CreatedAt,
            UpdatedAt = staff.User.UpdatedAt
        },
        Role = staff.Role,
        Department = staff.Department,
        CreatedAt = staff.CreatedAt,
        UpdatedAt = staff.UpdatedAt
    };
}
