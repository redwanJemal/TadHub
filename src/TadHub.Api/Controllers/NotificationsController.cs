using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Contracts;
using Notification.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
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

/// <summary>
/// Notification template management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/notification-templates")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class NotificationTemplatesController : ControllerBase
{
    private readonly INotificationTemplateService _templateService;

    public NotificationTemplatesController(INotificationTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    [HasPermission("notifications.manage")]
    [ProducesResponseType(typeof(IEnumerable<NotificationTemplateListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _templateService.ListAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("notifications.manage")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _templateService.GetByIdAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission("notifications.manage")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken ct)
    {
        var result = await _templateService.CreateAsync(tenantId, request, ct);
        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
        return CreatedAtAction(nameof(GetById), new { tenantId, id = result.Value!.Id }, result.Value);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("notifications.manage")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken ct)
    {
        var result = await _templateService.UpdateAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("notifications.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _templateService.DeleteAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return NoContent();
    }
}

/// <summary>
/// User notification preference endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/notification-preferences")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class NotificationPreferencesController : ControllerBase
{
    private readonly IUserNotificationPreferenceService _preferenceService;
    private readonly ICurrentUser _currentUser;

    public NotificationPreferencesController(
        IUserNotificationPreferenceService preferenceService,
        ICurrentUser currentUser)
    {
        _preferenceService = preferenceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserNotificationPreferenceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _preferenceService.GetPreferencesAsync(tenantId, _currentUser.UserId, ct);
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<UserNotificationPreferenceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences(
        Guid tenantId,
        [FromBody] BulkUpdateUserPreferencesRequest request,
        CancellationToken ct)
    {
        var result = await _preferenceService.BulkUpdateAsync(tenantId, _currentUser.UserId, request, ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
