using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Contracts;
using Notification.Contracts.DTOs;
using Notification.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Models;

namespace Notification.Core.Services;

public sealed class UserNotificationPreferenceService : IUserNotificationPreferenceService
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserNotificationPreferenceService> _logger;

    public UserNotificationPreferenceService(AppDbContext db, ILogger<UserNotificationPreferenceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserNotificationPreferenceDto>> GetPreferencesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var prefs = await _db.Set<UserNotificationPreference>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .OrderBy(x => x.EventType)
            .Select(x => new UserNotificationPreferenceDto
            {
                Id = x.Id,
                UserId = x.UserId,
                EventType = x.EventType,
                Muted = x.Muted,
                Channels = x.Channels
            })
            .ToListAsync(ct);

        return prefs.AsReadOnly();
    }

    public async Task<Result<IReadOnlyList<UserNotificationPreferenceDto>>> BulkUpdateAsync(
        Guid tenantId, Guid userId, BulkUpdateUserPreferencesRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Set<UserNotificationPreference>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .ToListAsync(ct);

        foreach (var pref in request.Preferences)
        {
            var existing_pref = existing.FirstOrDefault(x => x.EventType == pref.EventType);
            if (existing_pref is not null)
            {
                existing_pref.Muted = pref.Muted;
                existing_pref.Channels = pref.Channels;
            }
            else
            {
                _db.Set<UserNotificationPreference>().Add(new UserNotificationPreference
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = userId,
                    EventType = pref.EventType,
                    Muted = pref.Muted,
                    Channels = pref.Channels
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated {Count} notification preferences for user {UserId} in tenant {TenantId}",
            request.Preferences.Count, userId, tenantId);

        return Result<IReadOnlyList<UserNotificationPreferenceDto>>.Success(
            await GetPreferencesAsync(tenantId, userId, ct));
    }

    public async Task<bool> IsEventMutedForUserAsync(
        Guid tenantId, Guid userId, string eventType, CancellationToken ct = default)
    {
        var pref = await _db.Set<UserNotificationPreference>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId
                && x.EventType == eventType && !x.IsDeleted, ct);

        return pref?.Muted ?? false;
    }
}
