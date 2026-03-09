using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReferenceData.Contracts;
using ReferenceData.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// Country payment packages — standard pricing schemas per country of origin.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/country-packages")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class CountryPackagesController : ControllerBase
{
    private readonly ICountryPackageService _packageService;

    public CountryPackagesController(ICountryPackageService packageService)
    {
        _packageService = packageService;
    }

    /// <summary>
    /// Lists country packages with filtering and pagination.
    /// </summary>
    [HttpGet]
    [HasPermission("packages.view")]
    [ProducesResponseType(typeof(PagedList<CountryPackageListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _packageService.ListAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a country package by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("packages.view")]
    [ProducesResponseType(typeof(CountryPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid tenantId, Guid id, CancellationToken ct)
    {
        var result = await _packageService.GetByIdAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets packages for a specific country.
    /// </summary>
    [HttpGet("by-country/{countryId:guid}")]
    [HasPermission("packages.view")]
    [ProducesResponseType(typeof(List<CountryPackageListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCountry(Guid tenantId, Guid countryId, CancellationToken ct)
    {
        var result = await _packageService.GetByCountryAsync(tenantId, countryId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets the default active package for a country.
    /// </summary>
    [HttpGet("default/{countryId:guid}")]
    [HasPermission("packages.view")]
    [ProducesResponseType(typeof(CountryPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultByCountry(Guid tenantId, Guid countryId, CancellationToken ct)
    {
        var result = await _packageService.GetDefaultByCountryAsync(tenantId, countryId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new country package.
    /// </summary>
    [HttpPost]
    [HasPermission("packages.create")]
    [ProducesResponseType(typeof(CountryPackageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateCountryPackageRequest request, CancellationToken ct)
    {
        var result = await _packageService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Updates a country package.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [HasPermission("packages.edit")]
    [ProducesResponseType(typeof(CountryPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid tenantId, Guid id, [FromBody] UpdateCountryPackageRequest request, CancellationToken ct)
    {
        var result = await _packageService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Soft-deletes a country package.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("packages.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid tenantId, Guid id, CancellationToken ct)
    {
        var result = await _packageService.DeleteAsync(tenantId, id, ct);

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
