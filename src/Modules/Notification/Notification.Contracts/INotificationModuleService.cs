using Notification.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Notification.Contracts;

/// <summary>
/// Service for managing user notifications.
/// </summary>
public interface INotificationModuleService
{
    /// <summary>
    /// Gets notifications for a user with optional filtering.
    /// Supports filter[isRead], filter[type], sort=-createdAt
    /// </summary>
    Task<PagedList<NotificationDto>> GetNotificationsAsync(
        Guid tenantId,
        Guid userId,
        QueryParameters qp,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    Task<Result<NotificationDto>> GetByIdAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a new notification and pushes via SSE.
    /// </summary>
    Task<Result<NotificationDto>> CreateAsync(
        Guid tenantId,
        CreateNotificationRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task<Result<NotificationDto>> MarkAsReadAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    Task<Result<int>> MarkAllAsReadAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the unread notification count for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    Task<Result<bool>> DeleteAsync(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all notifications across tenants with optional filtering.
    /// Supports filter[tenantId], filter[type], filter[userId], sort=-createdAt
    /// </summary>
    Task<PagedList<NotificationDto>> GetAllNotificationsAsync(
        QueryParameters qp,
        CancellationToken ct = default);
}
