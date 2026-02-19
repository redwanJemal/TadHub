using System.Linq.Expressions;
using Identity.Contracts;
using Identity.Contracts.DTOs;
using Identity.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Identity.Core.Services;

/// <summary>
/// Service for managing user profiles.
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IClock _clock;
    private readonly ILogger<IdentityService> _logger;

    /// <summary>
    /// Fields available for filtering in list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<UserProfile, object>>> FilterableFields = new()
    {
        ["email"] = x => x.Email,
        ["firstName"] = x => x.FirstName,
        ["lastName"] = x => x.LastName,
        ["isActive"] = x => x.IsActive,
        ["locale"] = x => x.Locale,
        ["createdAt"] = x => x.CreatedAt
    };

    /// <summary>
    /// Fields available for sorting in list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<UserProfile, object>>> SortableFields = new()
    {
        ["email"] = x => x.Email,
        ["firstName"] = x => x.FirstName,
        ["lastName"] = x => x.LastName,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["lastLoginAt"] = x => x.LastLoginAt!
    };

    public IdentityService(
        AppDbContext db,
        IPublishEndpoint publisher,
        IClock clock,
        ILogger<IdentityService> logger)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
            return Result<UserProfileDto>.NotFound($"User with ID {id} not found");

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserProfileDto>> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.KeycloakId == keycloakId, ct);

        if (user is null)
            return Result<UserProfileDto>.NotFound($"User with Keycloak ID {keycloakId} not found");

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);

        if (user is null)
            return Result<UserProfileDto>.NotFound($"User with email {email} not found");

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserProfileDto>> CreateAsync(CreateUserProfileRequest request, CancellationToken ct = default)
    {
        // Check for duplicate KeycloakId
        var existsByKeycloak = await _db.Set<UserProfile>()
            .AnyAsync(x => x.KeycloakId == request.KeycloakId, ct);
        if (existsByKeycloak)
            return Result<UserProfileDto>.Conflict($"User with Keycloak ID {request.KeycloakId} already exists");

        // Check for duplicate email
        var existsByEmail = await _db.Set<UserProfile>()
            .AnyAsync(x => x.Email.ToLower() == request.Email.ToLower(), ct);
        if (existsByEmail)
            return Result<UserProfileDto>.Conflict($"User with email {request.Email} already exists");

        var user = new UserProfile
        {
            Id = Guid.NewGuid(),
            KeycloakId = request.KeycloakId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AvatarUrl = request.AvatarUrl,
            Phone = request.Phone,
            Locale = request.Locale,
            DefaultTenantId = request.DefaultTenantId,
            IsActive = true
        };

        _db.Set<UserProfile>().Add(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created user profile {UserId} for {Email}", user.Id, user.Email);

        // Publish domain event
        await _publisher.Publish(new UserCreatedEvent(
            user.Id,
            user.KeycloakId,
            user.Email,
            user.FirstName,
            user.LastName,
            _clock.UtcNow), ct);

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserProfileDto>> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>().FindAsync([id], ct);
        if (user is null)
            return Result<UserProfileDto>.NotFound($"User with ID {id} not found");

        // Apply updates (only non-null values)
        if (request.FirstName is not null)
            user.FirstName = request.FirstName;
        if (request.LastName is not null)
            user.LastName = request.LastName;
        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl;
        if (request.Phone is not null)
            user.Phone = request.Phone;
        if (request.Locale is not null)
            user.Locale = request.Locale;
        if (request.DefaultTenantId.HasValue)
            user.DefaultTenantId = request.DefaultTenantId;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated user profile {UserId}", user.Id);

        // Publish domain event
        await _publisher.Publish(new UserUpdatedEvent(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            _clock.UtcNow), ct);

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>().FindAsync([id], ct);
        if (user is null)
            return Result<bool>.NotFound($"User with ID {id} not found");

        if (!user.IsActive)
            return Result<bool>.Success(true); // Already deactivated

        user.IsActive = false;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deactivated user profile {UserId}", user.Id);

        await _publisher.Publish(new UserDeactivatedEvent(user.Id, _clock.UtcNow), ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>().FindAsync([id], ct);
        if (user is null)
            return Result<bool>.NotFound($"User with ID {id} not found");

        if (user.IsActive)
            return Result<bool>.Success(true); // Already active

        user.IsActive = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Reactivated user profile {UserId}", user.Id);

        return Result<bool>.Success(true);
    }

    public async Task<PagedList<UserProfileDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<UserProfile>()
            .AsNoTracking()
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.Email.ToLower().Contains(searchLower) ||
                x.FirstName.ToLower().Contains(searchLower) ||
                x.LastName.ToLower().Contains(searchLower));
        }

        var pagedList = await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);

        return pagedList;
    }

    public async Task RecordLoginAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<UserProfile>().FindAsync([id], ct);
        if (user is null)
            return;

        user.LastLoginAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Recorded login for user {UserId}", user.Id);
    }

    public async Task<Result<UserProfileDto>> GetOrCreateFromKeycloakAsync(
        string keycloakId,
        string email,
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        // Try to find existing user
        var existingUser = await _db.Set<UserProfile>()
            .FirstOrDefaultAsync(x => x.KeycloakId == keycloakId, ct);

        if (existingUser is not null)
        {
            // Update last login
            existingUser.LastLoginAt = _clock.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Result<UserProfileDto>.Success(MapToDto(existingUser));
        }

        // Create new user (JIT provisioning)
        var request = new CreateUserProfileRequest
        {
            KeycloakId = keycloakId,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        return await CreateAsync(request, ct);
    }

    private static UserProfileDto MapToDto(UserProfile user) => new()
    {
        Id = user.Id,
        KeycloakId = user.KeycloakId,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        AvatarUrl = user.AvatarUrl,
        Phone = user.Phone,
        Locale = user.Locale,
        DefaultTenantId = user.DefaultTenantId,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
