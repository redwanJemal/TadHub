using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Contracts;
using Notification.Contracts.Channels;
using Notification.Contracts.DTOs;
using Notification.Core.Services;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;
using Tenancy.Contracts;

namespace TadHub.Api.Controllers;

/// <summary>
/// Platform admin notification management endpoints.
/// Allows admins to send notifications to tenants and view notification history.
/// </summary>
[ApiController]
[Route("api/v1/admin/notifications")]
[Authorize(Roles = "platform-admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationModuleService _notificationService;
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;
    private readonly ITenantService _tenantService;

    public AdminNotificationsController(
        INotificationModuleService notificationService,
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver,
        ITenantService tenantService)
    {
        _notificationService = notificationService;
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
        _tenantService = tenantService;
    }

    /// <summary>
    /// Sends a notification to a tenant's members (or specific users).
    /// When userIds is empty/null, sends to all tenant members.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(AdminSendNotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendNotification(
        [FromBody] AdminSendNotificationRequest request,
        CancellationToken ct)
    {
        // Validate tenant exists
        var tenant = await _tenantService.GetByIdAsync(request.TenantId, ct);
        if (!tenant.IsSuccess)
            return NotFound(new { error = "Tenant not found" });

        // Resolve recipients
        IReadOnlyList<RecipientInfo> recipients;

        if (request.UserIds is { Count: > 0 })
        {
            // Send to specific users — resolve their info from tenant members
            var allMembers = await _recipientResolver.GetAllMembersAsync(request.TenantId, ct);
            var userIdSet = request.UserIds.ToHashSet();
            recipients = allMembers.Where(m => userIdSet.Contains(m.UserId)).ToList();

            if (recipients.Count == 0)
                return BadRequest(new { error = "No valid recipients found among the specified user IDs" });
        }
        else
        {
            // Send to all tenant members
            recipients = await _recipientResolver.GetAllMembersAsync(request.TenantId, ct);

            if (recipients.Count == 0)
                return BadRequest(new { error = "Tenant has no active members" });
        }

        // Dispatch notifications
        await _dispatcher.DispatchToManyAsync(
            request.TenantId,
            recipients,
            request.Title,
            request.Body,
            request.Type,
            request.Link,
            "admin.notification",
            ct: ct);

        return Ok(new AdminSendNotificationResponse
        {
            RecipientCount = recipients.Count,
            TenantCount = 1
        });
    }

    /// <summary>
    /// Lists all notifications across tenants with optional filtering.
    /// Supports: filter[tenantId], filter[type], filter[userId], sort=-createdAt
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListNotifications(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _notificationService.GetAllNotificationsAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets notification settings for a tenant.
    /// </summary>
    [HttpGet("tenants/{tenantId:guid}/settings")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantNotificationSettings(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _tenantService.GetSettingsJsonAsync(tenantId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { settings = result.Value });
    }

    /// <summary>
    /// Updates notification settings for a tenant.
    /// </summary>
    [HttpPut("tenants/{tenantId:guid}/settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenantNotificationSettings(
        Guid tenantId,
        [FromBody] UpdateNotificationSettingsRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.UpdateSettingsSectionAsync(
            tenantId,
            "notifications",
            request.SettingsJson,
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new { updated = true });
    }
}

/// <summary>
/// Request to update tenant notification settings.
/// </summary>
public record UpdateNotificationSettingsRequest(string SettingsJson);
