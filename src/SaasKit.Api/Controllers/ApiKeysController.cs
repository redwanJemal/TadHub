using ApiManagement.Contracts;
using ApiManagement.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Api;

namespace SaasKit.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/api-keys")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    [HasPermission("api.view")]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeys(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _apiKeyService.GetApiKeysAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("{apiKeyId:guid}")]
    [HasPermission("api.view")]
    [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKey(Guid tenantId, Guid apiKeyId, CancellationToken ct)
    {
        var result = await _apiKeyService.GetApiKeyByIdAsync(tenantId, apiKeyId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission("api.manage")]
    [ProducesResponseType(typeof(ApiKeyWithSecretDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApiKey(Guid tenantId, [FromBody] CreateApiKeyRequest request, CancellationToken ct)
    {
        var result = await _apiKeyService.CreateApiKeyAsync(tenantId, request, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetApiKey), new { tenantId, apiKeyId = result.Value!.Id }, result.Value);
    }

    [HttpDelete("{apiKeyId:guid}")]
    [HasPermission("api.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeApiKey(Guid tenantId, Guid apiKeyId, CancellationToken ct)
    {
        var result = await _apiKeyService.RevokeApiKeyAsync(tenantId, apiKeyId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("{apiKeyId:guid}/logs")]
    [HasPermission("api.view")]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeyLogs(Guid tenantId, Guid apiKeyId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _apiKeyService.GetApiKeyLogsAsync(tenantId, apiKeyId, qp, ct);
        return Ok(result);
    }
}
