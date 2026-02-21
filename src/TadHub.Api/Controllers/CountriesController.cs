using Microsoft.AspNetCore.Mvc;
using ReferenceData.Contracts;
using ReferenceData.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// Country reference data endpoints (public).
/// ISO 3166-1 compliant with bilingual support.
/// </summary>
[ApiController]
[Route("api/v1/countries")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    /// <summary>
    /// Lists countries with filtering and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[code]=AE,US (ISO alpha-2 codes)
    /// - filter[isActive]=true
    /// - filter[isCommonNationality]=true
    /// 
    /// Sort:
    /// - sort=displayOrder (default)
    /// - sort=code
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<CountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _countryService.ListAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a country by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _countryService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a country by ISO alpha-2 code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _countryService.GetByCodeAsync(code, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets lightweight country references for dropdowns.
    /// Only active countries, ordered by display order.
    /// </summary>
    [HttpGet("refs")]
    [ProducesResponseType(typeof(List<CountryRefDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferences(CancellationToken ct)
    {
        var result = await _countryService.GetReferencesAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets common Tadbeer source nationalities.
    /// Philippines, Indonesia, Ethiopia, India, Sri Lanka, Nepal, Bangladesh, Uganda, Kenya, Ghana.
    /// </summary>
    [HttpGet("common-nationalities")]
    [ProducesResponseType(typeof(List<CountryRefDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommonNationalities(CancellationToken ct)
    {
        var result = await _countryService.GetCommonNationalitiesAsync(ct);
        return Ok(result);
    }
}
