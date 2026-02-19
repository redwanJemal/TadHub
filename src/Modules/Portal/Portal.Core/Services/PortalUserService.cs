using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Contracts;
using Portal.Contracts.DTOs;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Portal.Core.Services;

/// <summary>
/// Service for managing portal users.
/// </summary>
public class PortalUserService : IPortalUserService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<PortalUserService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.PortalUser, object>>> UserFilters = new()
    {
        ["isActive"] = x => x.IsActive,
        ["email"] = x => x.Email,
        ["emailVerified"] = x => x.EmailVerified
    };

    private static readonly Dictionary<string, Expression<Func<Entities.PortalUser, object>>> UserSortable = new()
    {
        ["email"] = x => x.Email,
        ["createdAt"] = x => x.CreatedAt,
        ["lastLoginAt"] = x => x.LastLoginAt!
    };

    public PortalUserService(AppDbContext db, IClock clock, ILogger<PortalUserService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<PortalUserDto>> GetUsersAsync(Guid portalId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.PortalUser>()
            .AsNoTracking()
            .Where(x => x.PortalId == portalId)
            .ApplyFilters(qp.Filters, UserFilters)
            .ApplySort(qp.GetSortFields(), UserSortable);

        return await query
            .Select(u => MapToDto(u))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<PortalUserDto>> GetUserByIdAsync(Guid portalId, Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Set<Entities.PortalUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.PortalId == portalId, ct);

        if (user is null)
            return Result<PortalUserDto>.NotFound("User not found");

        return Result<PortalUserDto>.Success(MapToDto(user));
    }

    public async Task<Result<PortalUserDto>> GetUserByEmailAsync(Guid portalId, string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToUpperInvariant();

        var user = await _db.Set<Entities.PortalUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortalId == portalId && x.NormalizedEmail == normalizedEmail, ct);

        if (user is null)
            return Result<PortalUserDto>.NotFound("User not found");

        return Result<PortalUserDto>.Success(MapToDto(user));
    }

    public async Task<Result<PortalUserDto>> CreateUserAsync(Guid portalId, CreatePortalUserRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        // Check for existing user
        var exists = await _db.Set<Entities.PortalUser>()
            .AnyAsync(x => x.PortalId == portalId && x.NormalizedEmail == normalizedEmail, ct);

        if (exists)
            return Result<PortalUserDto>.Conflict($"User with email '{request.Email}' already exists");

        // Get portal to get tenant ID
        var portal = await _db.Set<Entities.Portal>()
            .FirstOrDefaultAsync(x => x.Id == portalId, ct);

        if (portal is null)
            return Result<PortalUserDto>.NotFound("Portal not found");

        var user = new Entities.PortalUser
        {
            Id = Guid.NewGuid(),
            TenantId = portal.TenantId,
            PortalId = portalId,
            Email = request.Email.ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = request.Password is not null ? HashPassword(request.Password) : null,
            EmailVerified = !portal.RequireEmailVerification,
            IsActive = true
        };

        _db.Set<Entities.PortalUser>().Add(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created portal user {UserId} '{Email}' in portal {PortalId}",
            user.Id, user.Email, portalId);

        // TODO: Send invitation email if request.SendInvitation

        return Result<PortalUserDto>.Success(MapToDto(user));
    }

    public async Task<Result<PortalUserDto>> UpdateUserAsync(Guid portalId, Guid userId, UpdatePortalUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Set<Entities.PortalUser>()
            .FirstOrDefaultAsync(x => x.Id == userId && x.PortalId == portalId, ct);

        if (user is null)
            return Result<PortalUserDto>.NotFound("User not found");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated portal user {UserId} in portal {PortalId}", userId, portalId);

        return Result<PortalUserDto>.Success(MapToDto(user));
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid portalId, Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Set<Entities.PortalUser>()
            .FirstOrDefaultAsync(x => x.Id == userId && x.PortalId == portalId, ct);

        if (user is null)
            return Result<bool>.NotFound("User not found");

        _db.Set<Entities.PortalUser>().Remove(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted portal user {UserId} from portal {PortalId}", userId, portalId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<PortalUserDto>> RegisterAsync(Guid portalId, PortalUserRegistrationRequest request, CancellationToken ct = default)
    {
        var portal = await _db.Set<Entities.Portal>()
            .FirstOrDefaultAsync(x => x.Id == portalId && x.IsActive, ct);

        if (portal is null)
            return Result<PortalUserDto>.NotFound("Portal not found");

        if (!portal.AllowPublicRegistration)
            return Result<PortalUserDto>.ValidationError("Public registration is not allowed for this portal");

        var normalizedEmail = request.Email.ToUpperInvariant();

        // Check for existing user
        var exists = await _db.Set<Entities.PortalUser>()
            .AnyAsync(x => x.PortalId == portalId && x.NormalizedEmail == normalizedEmail, ct);

        if (exists)
            return Result<PortalUserDto>.Conflict("An account with this email already exists");

        var user = new Entities.PortalUser
        {
            Id = Guid.NewGuid(),
            TenantId = portal.TenantId,
            PortalId = portalId,
            Email = request.Email.ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password),
            EmailVerified = !portal.RequireEmailVerification,
            IsActive = true
        };

        _db.Set<Entities.PortalUser>().Add(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("New portal user registered: {UserId} '{Email}' in portal {PortalId}",
            user.Id, user.Email, portalId);

        // TODO: Send verification email if RequireEmailVerification

        return Result<PortalUserDto>.Success(MapToDto(user));
    }

    public async Task<Result<PortalUserLoginResponse>> LoginAsync(Guid portalId, PortalUserLoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = await _db.Set<Entities.PortalUser>()
            .FirstOrDefaultAsync(x => x.PortalId == portalId && x.NormalizedEmail == normalizedEmail, ct);

        if (user is null || user.PasswordHash is null)
            return Result<PortalUserLoginResponse>.ValidationError("Invalid email or password");

        if (!user.IsActive)
            return Result<PortalUserLoginResponse>.ValidationError("Account is disabled");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Result<PortalUserLoginResponse>.ValidationError("Invalid email or password");

        // Update login stats
        user.LastLoginAt = _clock.UtcNow;
        user.LoginCount++;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Portal user {UserId} logged in to portal {PortalId}", user.Id, portalId);

        // TODO: Generate real JWT tokens
        return Result<PortalUserLoginResponse>.Success(new PortalUserLoginResponse
        {
            AccessToken = $"portal_access_token_{user.Id}",
            RefreshToken = $"portal_refresh_token_{user.Id}",
            ExpiresIn = 3600,
            User = MapToDto(user)
        });
    }

    public async Task<Result<PortalUserDto>> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var registration = await _db.Set<Entities.PortalUserRegistration>()
            .IgnoreQueryFilters()
            .Include(r => r.PortalUser)
            .FirstOrDefaultAsync(x => x.VerificationToken == token && x.Status == "pending", ct);

        if (registration is null)
            return Result<PortalUserDto>.NotFound("Invalid or expired verification token");

        if (registration.ExpiresAt < _clock.UtcNow)
        {
            registration.Status = "expired";
            await _db.SaveChangesAsync(ct);
            return Result<PortalUserDto>.ValidationError("Verification token has expired");
        }

        registration.Status = "verified";
        registration.VerifiedAt = _clock.UtcNow;

        if (registration.PortalUser is not null)
        {
            registration.PortalUser.EmailVerified = true;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Email verified for portal user registration {RegistrationId}", registration.Id);

        return registration.PortalUser is not null
            ? Result<PortalUserDto>.Success(MapToDto(registration.PortalUser))
            : Result<PortalUserDto>.NotFound("User not found");
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        string expectedHash = parts[1];

        string actualHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));

        return actualHash == expectedHash;
    }

    private static PortalUserDto MapToDto(Entities.PortalUser u) => new()
    {
        Id = u.Id,
        PortalId = u.PortalId,
        Email = u.Email,
        EmailVerified = u.EmailVerified,
        FirstName = u.FirstName,
        LastName = u.LastName,
        DisplayName = u.DisplayName,
        AvatarUrl = u.AvatarUrl,
        PhoneNumber = u.PhoneNumber,
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        LoginCount = u.LoginCount,
        HasSso = u.SsoSubject is not null,
        CreatedAt = u.CreatedAt
    };
}
