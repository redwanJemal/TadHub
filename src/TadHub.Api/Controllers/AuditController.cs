using Audit.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/audit")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService) => _auditService = auditService;

    [HttpGet("events")]
    [HasPermission("analytics.view")]
    public async Task<IActionResult> GetEvents(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        qp.PageSize = Math.Min(qp.PageSize, 200); // Max 200 for audit
        return Ok(await _auditService.GetEventsAsync(tenantId, qp, ct));
    }

    [HttpGet("logs")]
    [HasPermission("analytics.view")]
    public async Task<IActionResult> GetLogs(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        qp.PageSize = Math.Min(qp.PageSize, 200);
        return Ok(await _auditService.GetLogsAsync(tenantId, qp, ct));
    }
}

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/webhooks")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService) => _webhookService = webhookService;

    [HttpGet]
    [HasPermission("settings.view")]
    public async Task<IActionResult> GetWebhooks(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
        => Ok(await _webhookService.GetWebhooksAsync(tenantId, qp, ct));

    [HttpPost]
    [HasPermission("settings.manage")]
    public async Task<IActionResult> CreateWebhook(Guid tenantId, [FromBody] CreateWebhookRequest request, CancellationToken ct)
    {
        var result = await _webhookService.CreateWebhookAsync(tenantId, request, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Created($"/api/v1/tenants/{tenantId}/webhooks/{result.Value!.Id}", result.Value);
    }

    [HttpDelete("{webhookId:guid}")]
    [HasPermission("settings.manage")]
    public async Task<IActionResult> DeleteWebhook(Guid tenantId, Guid webhookId, CancellationToken ct)
    {
        var result = await _webhookService.DeleteWebhookAsync(tenantId, webhookId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }
}
