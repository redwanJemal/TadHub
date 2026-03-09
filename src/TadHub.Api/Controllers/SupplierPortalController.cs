using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplierPortal.Contracts;
using SupplierPortal.Contracts.DTOs;
using Candidate.Contracts;
using Candidate.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/supplier-portal")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class SupplierPortalController : ControllerBase
{
    private readonly ISupplierPortalService _portalService;
    private readonly ICandidateService _candidateService;
    private readonly ICurrentUser _currentUser;

    public SupplierPortalController(
        ISupplierPortalService portalService,
        ICandidateService candidateService,
        ICurrentUser currentUser)
    {
        _portalService = portalService;
        _candidateService = candidateService;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(SupplierUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid tenantId, CancellationToken ct)
    {
        var result = await _portalService.GetSupplierUserByUserIdAsync(_currentUser.UserId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("dashboard")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(SupplierDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(Guid tenantId, CancellationToken ct)
    {
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _portalService.GetDashboardAsync(tenantId, supplierResult.Value, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("candidates")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(PagedList<SupplierCandidateListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCandidates(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _portalService.ListCandidatesAsync(tenantId, supplierResult.Value, qp, ct);
        return Ok(result);
    }

    [HttpPost("candidates")]
    [HasPermission("supplier_portal.create_candidate")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCandidate(
        Guid tenantId,
        [FromBody] CreateCandidateRequest request,
        CancellationToken ct)
    {
        // Ensure the candidate is linked to this supplier's tenant_supplier
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _candidateService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(201, result.Value);
    }

    [HttpGet("workers")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(PagedList<SupplierWorkerListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListWorkers(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _portalService.ListWorkersAsync(tenantId, supplierResult.Value, qp, ct);
        return Ok(result);
    }

    [HttpGet("commissions")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(PagedList<SupplierCommissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCommissions(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _portalService.ListCommissionsAsync(tenantId, supplierResult.Value, qp, ct);
        return Ok(result);
    }

    [HttpGet("arrivals")]
    [HasPermission("supplier_portal.view")]
    [ProducesResponseType(typeof(PagedList<SupplierArrivalListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListArrivals(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var supplierResult = await GetSupplierIdForCurrentUser(ct);
        if (!supplierResult.IsSuccess)
            return MapResultError(supplierResult);

        var result = await _portalService.ListArrivalsAsync(tenantId, supplierResult.Value, qp, ct);
        return Ok(result);
    }

    #region Admin endpoints for managing supplier users

    [HttpGet("users")]
    [HasPermission("supplier_portal.manage")]
    [ProducesResponseType(typeof(PagedList<SupplierUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSupplierUsers(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _portalService.ListSupplierUsersAsync(qp, ct);
        return Ok(result);
    }

    [HttpPost("users")]
    [HasPermission("supplier_portal.manage")]
    [ProducesResponseType(typeof(SupplierUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSupplierUser(
        Guid tenantId,
        [FromBody] CreateSupplierUserRequest request,
        CancellationToken ct)
    {
        var result = await _portalService.CreateSupplierUserAsync(request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(201, result.Value);
    }

    [HttpPatch("users/{id:guid}")]
    [HasPermission("supplier_portal.manage")]
    [ProducesResponseType(typeof(SupplierUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierUser(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateSupplierUserRequest request,
        CancellationToken ct)
    {
        var result = await _portalService.UpdateSupplierUserAsync(id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    #endregion

    #region Helpers

    private async Task<Result<Guid>> GetSupplierIdForCurrentUser(CancellationToken ct)
    {
        var userResult = await _portalService.GetSupplierUserByUserIdAsync(_currentUser.UserId, ct);

        if (!userResult.IsSuccess)
            return Result<Guid>.Forbidden("Current user is not linked to a supplier account");

        if (!userResult.Value!.IsActive)
            return Result<Guid>.Forbidden("Supplier account is inactive");

        return Result<Guid>.Success(userResult.Value.SupplierId);
    }

    private IActionResult MapResultError<T>(Result<T> result)
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
