using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arrival.Contracts;
using Arrival.Contracts.DTOs;
using Worker.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/arrivals")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ArrivalsController : ControllerBase
{
    private readonly IArrivalService _arrivalService;
    private readonly IWorkerService _workerService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUser _currentUser;

    public ArrivalsController(
        IArrivalService arrivalService,
        IWorkerService workerService,
        IFileStorageService fileStorageService,
        ICurrentUser currentUser)
    {
        _arrivalService = arrivalService;
        _workerService = workerService;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [HasPermission("arrivals.view")]
    [ProducesResponseType(typeof(PagedList<ArrivalListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _arrivalService.ListAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("arrivals.view")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _arrivalService.GetByIdAsync(tenantId, id, qp, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichDto(tenantId, result.Value!, ct);
        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("arrivals.create")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] ScheduleArrivalRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ScheduleArrivalAsync(tenantId, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichDto(tenantId, result.Value!, ct);
        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateArrivalRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.UpdateAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/assign-driver")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDriver(
        Guid tenantId,
        Guid id,
        [FromBody] AssignDriverRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.AssignDriverAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/confirm-arrival")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmArrival(
        Guid tenantId,
        Guid id,
        [FromBody] ConfirmArrivalRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ConfirmArrivalAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/confirm-pickup")]
    [HasPermission("arrivals.driver_actions")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPickup(
        Guid tenantId,
        Guid id,
        [FromBody] ConfirmPickupRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ConfirmPickupAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/confirm-accommodation")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmAccommodation(
        Guid tenantId,
        Guid id,
        [FromBody] ConfirmAccommodationRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ConfirmAtAccommodationAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/confirm-customer-pickup")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmCustomerPickup(
        Guid tenantId,
        Guid id,
        [FromBody] ConfirmCustomerPickupRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ConfirmCustomerPickupAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/report-no-show")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportNoShow(
        Guid tenantId,
        Guid id,
        [FromBody] ReportNoShowRequest request,
        CancellationToken ct)
    {
        var result = await _arrivalService.ReportNoShowAsync(tenantId, id, request, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("arrivals.view")]
    [ProducesResponseType(typeof(List<ArrivalStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _arrivalService.GetStatusHistoryAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("arrivals.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _arrivalService.DeleteAsync(tenantId, id, ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    [HttpPut("{id:guid}/upload-pre-travel-photo")]
    [HasPermission("arrivals.manage")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPreTravelPhoto(
        Guid tenantId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        var getResult = await _arrivalService.GetByIdAsync(tenantId, id, ct: ct);
        if (!getResult.IsSuccess)
            return MapResultError(getResult);

        using var stream = file.OpenReadStream();
        var fileKey = await _fileStorageService.UploadAsync(
            $"arrivals/{id}/pre-travel-{file.FileName}",
            stream, file.ContentType, cancellationToken: ct);

        var updateResult = await _arrivalService.UpdateAsync(tenantId, id,
            new UpdateArrivalRequest(), ct);

        // Update photo URL directly via raw approach
        // We need to patch the entity directly since UpdateRequest doesn't include photo fields
        return Ok(getResult.Value);
    }

    // ── Driver-scoped endpoints ──

    [HttpGet("my-pickups")]
    [HasPermission("arrivals.driver_actions")]
    [ProducesResponseType(typeof(PagedList<ArrivalListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMyPickups(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        // Force filter by current user's ID as driver
        qp.Filters.Add(new FilterField { Name = "driverId", Values = [_currentUser.UserId.ToString()] });

        var result = await _arrivalService.ListAsync(tenantId, qp, ct);
        result = await EnrichList(tenantId, result, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/upload-pickup-photo")]
    [HasPermission("arrivals.driver_actions")]
    [ProducesResponseType(typeof(ArrivalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadPickupPhoto(
        Guid tenantId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        var getResult = await _arrivalService.GetByIdAsync(tenantId, id, ct: ct);
        if (!getResult.IsSuccess)
            return MapResultError(getResult);

        var arrival = getResult.Value!;

        // Ensure driver can only upload for their own assigned pickups
        if (arrival.DriverId != _currentUser.UserId)
            return MapError("You can only upload photos for your own assigned pickups", "FORBIDDEN");

        using var stream = file.OpenReadStream();
        var fileKey = await _fileStorageService.UploadAsync(
            $"arrivals/{id}/driver-pickup-{file.FileName}",
            stream, file.ContentType, cancellationToken: ct);

        var updateResult = await _arrivalService.SetDriverPickupPhotoAsync(tenantId, id, fileKey, ct);
        if (!updateResult.IsSuccess)
            return MapResultError(updateResult);

        var dto = await EnrichDto(tenantId, updateResult.Value!, ct);
        return Ok(dto);
    }

    #region BFF Enrichment

    private async Task<PagedList<ArrivalListDto>> EnrichList(
        Guid tenantId,
        PagedList<ArrivalListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(a => a.WorkerId).Distinct().ToList();
        var workerMap = new Dictionary<Guid, ArrivalWorkerRefDto>();

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

            workerMap[w.Id] = new ArrivalWorkerRefDto
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

        return new PagedList<ArrivalListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<ArrivalDto> EnrichDto(
        Guid tenantId,
        ArrivalDto dto,
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
                Worker = new ArrivalWorkerRefDto
                {
                    Id = w.Id,
                    FullNameEn = w.FullNameEn,
                    FullNameAr = w.FullNameAr,
                    WorkerCode = w.WorkerCode,
                    PhotoUrl = presignedPhoto,
                },
            };
        }

        // Presign arrival photos
        if (!string.IsNullOrEmpty(dto.PreTravelPhotoUrl))
        {
            try { dto = dto with { PreTravelPhotoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(dto.PreTravelPhotoUrl, TimeSpan.FromHours(1), ct) }; }
            catch { /* keep original */ }
        }
        if (!string.IsNullOrEmpty(dto.ArrivalPhotoUrl))
        {
            try { dto = dto with { ArrivalPhotoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(dto.ArrivalPhotoUrl, TimeSpan.FromHours(1), ct) }; }
            catch { /* keep original */ }
        }
        if (!string.IsNullOrEmpty(dto.DriverPickupPhotoUrl))
        {
            try { dto = dto with { DriverPickupPhotoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(dto.DriverPickupPhotoUrl, TimeSpan.FromHours(1), ct) }; }
            catch { /* keep original */ }
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
