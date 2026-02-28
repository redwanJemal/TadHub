using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/supplier-payments")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class SupplierPaymentsController : ControllerBase
{
    private readonly ISupplierPaymentService _supplierPaymentService;

    public SupplierPaymentsController(ISupplierPaymentService supplierPaymentService)
    {
        _supplierPaymentService = supplierPaymentService;
    }

    [HttpGet]
    [HasPermission("supplier_payments.view")]
    [ProducesResponseType(typeof(PagedList<SupplierPaymentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.ListAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("supplier_payments.view")]
    [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.GetByIdAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission("supplier_payments.create")]
    [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateSupplierPaymentRequest request,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("supplier_payments.edit")]
    [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateSupplierPaymentRequest request,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission("supplier_payments.manage_status")]
    [ProducesResponseType(typeof(SupplierPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionSupplierPaymentStatusRequest request,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("supplier_payments.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _supplierPaymentService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region Error Helpers

    private IActionResult MapResultError<T>(Result<T> result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapResultError(Result result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapError(string error, string? errorCode)
    {
        var path = HttpContext.Request.Path.Value;
        var (status, apiError) = errorCode switch
        {
            "NOT_FOUND" => (404, ApiError.NotFound(error, path)),
            "CONFLICT" => (409, ApiError.Conflict(error, path)),
            "FORBIDDEN" => (403, ApiError.Forbidden(error)),
            _ => (400, ApiError.BadRequest(error, path))
        };
        return new ObjectResult(apiError) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    #endregion
}
