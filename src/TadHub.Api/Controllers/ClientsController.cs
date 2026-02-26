using Client.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/clients")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("clients.view")]
    public async Task<IActionResult> List(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _clientService.ListAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("clients.view")]
    public async Task<IActionResult> GetById(Guid tenantId, Guid id, CancellationToken ct)
    {
        var result = await _clientService.GetByIdAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission("clients.create")]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.CreateAsync(tenantId, request, ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "CONFLICT" => Conflict(new { error = result.Error }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }
        return CreatedAtAction(nameof(GetById), new { tenantId, id = result.Value!.Id }, result.Value);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("clients.edit")]
    public async Task<IActionResult> Update(Guid tenantId, Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct)
    {
        var result = await _clientService.UpdateAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("clients.delete")]
    public async Task<IActionResult> Delete(Guid tenantId, Guid id, CancellationToken ct)
    {
        var result = await _clientService.DeleteAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return NoContent();
    }
}
