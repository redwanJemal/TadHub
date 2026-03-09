using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Accommodation.Contracts;
using Accommodation.Contracts.DTOs;
using Worker.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/accommodations")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class AccommodationsController : ControllerBase
{
    private readonly IAccommodationService _accommodationService;
    private readonly IWorkerService _workerService;
    private readonly IFileStorageService _fileStorageService;

    public AccommodationsController(
        IAccommodationService accommodationService,
        IWorkerService workerService,
        IFileStorageService fileStorageService)
    {
        _accommodationService = accommodationService;
        _workerService = workerService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(PagedList<AccommodationStayListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _accommodationService.ListAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(AccommodationStayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _accommodationService.GetByIdAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichDto(tenantId, result.Value!, ct);
        return Ok(dto);
    }

    [HttpPost("check-in")]
    [HasPermission("accommodations.manage")]
    [ProducesResponseType(typeof(AccommodationStayDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(
        Guid tenantId,
        [FromBody] CheckInRequest request,
        CancellationToken ct)
    {
        var result = await _accommodationService.CheckInAsync(tenantId, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichDto(tenantId, result.Value!, ct);
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}/check-out")]
    [HasPermission("accommodations.manage")]
    [ProducesResponseType(typeof(AccommodationStayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut(
        Guid tenantId,
        Guid id,
        [FromBody] CheckOutRequest request,
        CancellationToken ct)
    {
        var result = await _accommodationService.CheckOutAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichDto(tenantId, result.Value!, ct);
        return Ok(dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("accommodations.manage")]
    [ProducesResponseType(typeof(AccommodationStayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateStayRequest request,
        CancellationToken ct)
    {
        var result = await _accommodationService.UpdateAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("accommodations.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _accommodationService.DeleteAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    [HttpGet("current")]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(PagedList<AccommodationStayListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentOccupants(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _accommodationService.GetCurrentOccupantsAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("daily-list")]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(PagedList<AccommodationStayListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyList(
        Guid tenantId,
        [FromQuery] DateOnly date,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _accommodationService.GetDailyListAsync(tenantId, date, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("history")]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(PagedList<AccommodationStayListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStayHistory(
        Guid tenantId,
        [FromQuery] Guid workerId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _accommodationService.GetStayHistoryByWorkerAsync(tenantId, workerId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("counts")]
    [HasPermission("accommodations.view")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCounts(
        Guid tenantId,
        CancellationToken ct)
    {
        var counts = await _accommodationService.GetCountsByStatusAsync(tenantId, ct);
        return Ok(counts);
    }

    #region BFF Enrichment

    private async Task<PagedList<AccommodationStayListDto>> EnrichList(
        Guid tenantId,
        PagedList<AccommodationStayListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(a => a.WorkerId).Distinct().ToList();
        var workerMap = new Dictionary<Guid, AccommodationWorkerRefDto>();

        // Fetch sequentially — DbContext is not thread-safe
        foreach (var wid in workerIds)
        {
            var result = await _workerService.GetByIdAsync(tenantId, wid, ct: ct);
            if (!result.IsSuccess) continue;
            var w = result.Value!;

            string? presignedPhoto = null;
            if (!string.IsNullOrEmpty(w.PhotoUrl))
            {
                try { presignedPhoto = await _fileStorageService.GetPresignedDownloadUrlAsync(w.PhotoUrl, TimeSpan.FromHours(1), ct); }
                catch { presignedPhoto = w.PhotoUrl; }
            }

            workerMap[w.Id] = new AccommodationWorkerRefDto
            {
                Id = w.Id,
                FullNameEn = w.FullNameEn,
                FullNameAr = w.FullNameAr,
                WorkerCode = w.WorkerCode,
                PhotoUrl = presignedPhoto,
            };
        }

        var enriched = pagedList.Items.Select(a => a with
        {
            Worker = workerMap.GetValueOrDefault(a.WorkerId),
        }).ToList();

        return new PagedList<AccommodationStayListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<AccommodationStayDto> EnrichDto(
        Guid tenantId,
        AccommodationStayDto dto,
        CancellationToken ct)
    {
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            string? presignedPhoto = null;
            if (!string.IsNullOrEmpty(w.PhotoUrl))
            {
                try { presignedPhoto = await _fileStorageService.GetPresignedDownloadUrlAsync(w.PhotoUrl, TimeSpan.FromHours(1), ct); }
                catch { presignedPhoto = w.PhotoUrl; }
            }

            dto = dto with
            {
                Worker = new AccommodationWorkerRefDto
                {
                    Id = w.Id,
                    FullNameEn = w.FullNameEn,
                    FullNameAr = w.FullNameAr,
                    WorkerCode = w.WorkerCode,
                    PhotoUrl = presignedPhoto,
                },
            };
        }

        return dto;
    }

    #endregion

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
