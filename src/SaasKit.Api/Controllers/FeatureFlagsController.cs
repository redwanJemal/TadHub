using FeatureFlags.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Api.Controllers;

[ApiController]
[Route("api/v1/feature-flags")]
[Authorize]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _flagService;
    private readonly ITenantContext _tenantContext;

    public FeatureFlagsController(IFeatureFlagService flagService, ITenantContext tenantContext)
    {
        _flagService = flagService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlags([FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _flagService.GetFlagsAsync(qp, ct);
        return Ok(result);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetFlag(string name, CancellationToken ct)
    {
        var result = await _flagService.GetFlagByNameAsync(name, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFlag([FromBody] CreateFeatureFlagRequest request, CancellationToken ct)
    {
        var result = await _flagService.CreateFlagAsync(request, ct);
        if (!result.IsSuccess) return result.ErrorCode == "CONFLICT" ? Conflict(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetFlag), new { name = result.Value!.Name }, result.Value);
    }

    [HttpPut("{flagId:guid}")]
    public async Task<IActionResult> UpdateFlag(Guid flagId, [FromBody] UpdateFeatureFlagRequest request, CancellationToken ct)
    {
        var result = await _flagService.UpdateFlagAsync(flagId, request, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{flagId:guid}")]
    public async Task<IActionResult> DeleteFlag(Guid flagId, CancellationToken ct)
    {
        var result = await _flagService.DeleteFlagAsync(flagId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("{name}/evaluate")]
    public async Task<IActionResult> EvaluateFlag(string name, [FromQuery] string? planSlug, CancellationToken ct)
    {
        var result = await _flagService.EvaluateFlagAsync(name, _tenantContext.TenantId, planSlug, ct);
        return Ok(result);
    }
}
