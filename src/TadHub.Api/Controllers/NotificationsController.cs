using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Contracts;
using Notification.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Api.Controllers;

/// <summary>
/// User notification management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/notifications")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationModuleService _notificationService;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(
        INotificationModuleService notificationService,
        ICurrentUser currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets notifications for the current user.
    /// Supports: filter[isRead]=false, filter[type]=info, filter[type]=warning, sort=-createdAt
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _notificationService.GetNotificationsAsync(
            tenantId,
            _currentUser.UserId,
            qp,
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific notification.
    /// </summary>
    [HttpGet("{notificationId:guid}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct)
    {
        var result = await _notificationService.GetByIdAsync(tenantId, notificationId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        // Verify the notification belongs to the current user
        if (result.Value!.UserId != _currentUser.UserId)
            return NotFound(new { error = "Notification not found" });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the unread notification count for the current user.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(Guid tenantId, CancellationToken ct)
    {
        var count = await _notificationService.GetUnreadCountAsync(
            tenantId,
            _currentUser.UserId,
            ct);

        return Ok(new UnreadCountDto { Count = count });
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    [HttpPost("{notificationId:guid}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct)
    {
        // First verify ownership
        var getResult = await _notificationService.GetByIdAsync(tenantId, notificationId, ct);
        if (!getResult.IsSuccess)
            return NotFound(new { error = getResult.Error });

        if (getResult.Value!.UserId != _currentUser.UserId)
            return NotFound(new { error = "Notification not found" });

        var result = await _notificationService.MarkAsReadAsync(tenantId, notificationId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Marks all notifications for the current user as read.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(Guid tenantId, CancellationToken ct)
    {
        var result = await _notificationService.MarkAllAsReadAsync(
            tenantId,
            _currentUser.UserId,
            ct);

        return Ok(new { markedAsRead = result.Value });
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    [HttpDelete("{notificationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(
        Guid tenantId,
        Guid notificationId,
        CancellationToken ct)
    {
        // First verify ownership
        var getResult = await _notificationService.GetByIdAsync(tenantId, notificationId, ct);
        if (!getResult.IsSuccess)
            return NotFound(new { error = getResult.Error });

        if (getResult.Value!.UserId != _currentUser.UserId)
            return NotFound(new { error = "Notification not found" });

        var result = await _notificationService.DeleteAsync(tenantId, notificationId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

/// <summary>
/// Internal notification endpoints for system/service use.
/// Used by other modules to create notifications.
/// </summary>
[ApiController]
[Route("api/v1/internal/notifications")]
[Authorize]
public class InternalNotificationsController : ControllerBase
{
    private readonly INotificationModuleService _notificationService;

    public InternalNotificationsController(INotificationModuleService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Creates a notification for a user (internal use).
    /// Requires appropriate permissions.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification(
        [FromBody] CreateNotificationWithTenantRequest request,
        CancellationToken ct)
    {
        var createRequest = new CreateNotificationRequest
        {
            UserId = request.UserId,
            Title = request.Title,
            Body = request.Body,
            Type = request.Type,
            Link = request.Link
        };

        var result = await _notificationService.CreateAsync(request.TenantId, createRequest, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(NotificationsController.GetNotification),
            "Notifications",
            new { tenantId = request.TenantId, notificationId = result.Value!.Id },
            result.Value);
    }
}

/// <summary>
/// Request to create a notification with tenant context.
/// </summary>
public record CreateNotificationWithTenantRequest
{
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Type { get; init; } = "info";
    public string? Link { get; init; }
}
