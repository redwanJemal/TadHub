using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Contracts;
using Notification.Contracts.DTOs;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Persistence;
using SaasKit.Infrastructure.Sse;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;
using SaasKit.SharedKernel.Models;

namespace Notification.Core.Services;

/// <summary>
/// Service for managing user notifications with SSE push support.
/// </summary>
public class NotificationModuleService : INotificationModuleService
{
    private readonly AppDbContext _db;
    private readonly ISseNotifier _sseNotifier;
    private readonly IClock _clock;
    private readonly ILogger<NotificationModuleService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Notification, object>>> NotificationFilters = new()
    {
        ["isRead"] = x => x.IsRead,
        ["type"] = x => x.Type,
        ["userId"] = x => x.UserId
    };

    public NotificationModuleService(
        AppDbContext db,
        ISseNotifier sseNotifier,
        IClock clock,
        ILogger<NotificationModuleService> logger)
    {
        _db = db;
        _sseNotifier = sseNotifier;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<NotificationDto>> GetNotificationsAsync(
        Guid tenantId,
        Guid userId,
        QueryParameters qp,
        CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Notification>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ApplyFilters(qp.Filters, NotificationFilters)
            .ApplySort(qp.GetSortFields(), GetSortExpressions());

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<NotificationDto>> GetByIdAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default)
    {
        var notification = await _db.Set<Entities.Notification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.TenantId == tenantId, ct);

        if (notification is null)
            return Result<NotificationDto>.NotFound("Notification not found");

        return Result<NotificationDto>.Success(MapToDto(notification));
    }

    public async Task<Result<NotificationDto>> CreateAsync(
        Guid tenantId,
        CreateNotificationRequest request,
        CancellationToken ct = default)
    {
        // Validate type
        var validTypes = new[] { "info", "warning", "success", "error" };
        if (!validTypes.Contains(request.Type.ToLowerInvariant()))
            return Result<NotificationDto>.ValidationError($"Invalid type. Must be one of: {string.Join(", ", validTypes)}");

        var notification = new Entities.Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = request.UserId,
            Title = request.Title,
            Body = request.Body,
            Type = request.Type.ToLowerInvariant(),
            Link = request.Link,
            IsRead = false
        };

        _db.Set<Entities.Notification>().Add(notification);
        await _db.SaveChangesAsync(ct);

        var dto = MapToDto(notification);

        // Push via SSE
        try
        {
            await _sseNotifier.SendToUserAsync(
                request.UserId,
                "notification.new",
                dto,
                ct);

            _logger.LogDebug("Pushed notification {NotificationId} to user {UserId} via SSE",
                notification.Id, request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push notification {NotificationId} via SSE", notification.Id);
            // Don't fail the create operation if SSE push fails
        }

        _logger.LogInformation("Created notification {NotificationId} for user {UserId} in tenant {TenantId}",
            notification.Id, request.UserId, tenantId);

        return Result<NotificationDto>.Success(dto);
    }

    public async Task<Result<NotificationDto>> MarkAsReadAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default)
    {
        var notification = await _db.Set<Entities.Notification>()
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.TenantId == tenantId, ct);

        if (notification is null)
            return Result<NotificationDto>.NotFound("Notification not found");

        if (notification.IsRead)
            return Result<NotificationDto>.Success(MapToDto(notification));

        notification.IsRead = true;
        notification.ReadAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Marked notification {NotificationId} as read", notificationId);

        return Result<NotificationDto>.Success(MapToDto(notification));
    }

    public async Task<Result<int>> MarkAllAsReadAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken ct = default)
    {
        var now = _clock.UtcNow;

        var count = await _db.Set<Entities.Notification>()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now)
                .SetProperty(x => x.UpdatedAt, now), ct);

        _logger.LogInformation("Marked {Count} notifications as read for user {UserId} in tenant {TenantId}",
            count, userId, tenantId);

        return Result<int>.Success(count);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await _db.Set<Entities.Notification>()
            .CountAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead, ct);
    }

    public async Task<Result<bool>> DeleteAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default)
    {
        var notification = await _db.Set<Entities.Notification>()
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.TenantId == tenantId, ct);

        if (notification is null)
            return Result<bool>.NotFound("Notification not found");

        _db.Set<Entities.Notification>().Remove(notification);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted notification {NotificationId} in tenant {TenantId}",
            notificationId, tenantId);

        return Result<bool>.Success(true);
    }

    private static NotificationDto MapToDto(Entities.Notification n) => new()
    {
        Id = n.Id,
        TenantId = n.TenantId,
        UserId = n.UserId,
        Title = n.Title,
        Body = n.Body,
        Type = n.Type,
        Link = n.Link,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };

    private static Dictionary<string, Expression<Func<Entities.Notification, object>>> GetSortExpressions() => new()
    {
        ["createdAt"] = x => x.CreatedAt,
        ["isRead"] = x => x.IsRead,
        ["type"] = x => x.Type
    };
}
