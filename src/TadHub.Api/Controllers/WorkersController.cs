using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker.Contracts;
using Worker.Contracts.DTOs;
using ReferenceData.Contracts;
using Supplier.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant-scoped worker management endpoints.
/// Workers are auto-created when candidates are converted — no Create endpoint.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/workers")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;
    private readonly ISupplierService _supplierService;
    private readonly IJobCategoryService _jobCategoryService;
    private readonly IFileStorageService _fileStorageService;

    public WorkersController(
        IWorkerService workerService,
        ISupplierService supplierService,
        IJobCategoryService jobCategoryService,
        IFileStorageService fileStorageService)
    {
        _workerService = workerService;
        _supplierService = supplierService;
        _jobCategoryService = jobCategoryService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [HasPermission("workers.view")]
    [ProducesResponseType(typeof(PagedList<WorkerListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _workerService.ListAsync(tenantId, qp, ct);

        var includes = qp.GetIncludeList();
        if (includes.Contains("supplier", StringComparer.OrdinalIgnoreCase))
        {
            result = await EnrichWithSupplierNames(tenantId, result, ct);
        }

        result = await EnrichListWithJobCategories(result, ct);
        result = await EnrichListWithPresignedUrls(result, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("workers.view")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _workerService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;

        var includes = qp.GetIncludeList();
        if (includes.Contains("supplier", StringComparer.OrdinalIgnoreCase) && dto.TenantSupplierId.HasValue)
        {
            dto = await EnrichWithSupplierName(tenantId, dto, ct);
        }

        dto = await EnrichWithJobCategory(dto, ct);
        dto = await EnrichWithPresignedUrls(dto, ct);

        return Ok(dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("workers.edit")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateWorkerRequest request,
        CancellationToken ct)
    {
        var result = await _workerService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission("workers.manage_status")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionWorkerStatusRequest request,
        CancellationToken ct)
    {
        var result = await _workerService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("workers.view")]
    [ProducesResponseType(typeof(List<WorkerStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _workerService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/cv")]
    [HasPermission("workers.view")]
    [ProducesResponseType(typeof(WorkerCvDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCv(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _workerService.GetCvAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var cv = result.Value!;

        // Enrich job category
        if (cv.JobCategoryId.HasValue)
        {
            var catResult = await _jobCategoryService.GetByIdAsync(cv.JobCategoryId.Value, ct);
            if (catResult.IsSuccess)
                cv = cv with { JobCategory = new JobCategoryInfoDto(cv.JobCategoryId.Value, catResult.Value!.NameEn) };
        }

        // Enrich presigned URLs
        var photoUrl = cv.PhotoUrl;
        var videoUrl = cv.VideoUrl;
        var passportUrl = cv.PassportDocumentUrl;

        if (!string.IsNullOrEmpty(photoUrl))
        {
            try { photoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(photoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }
        if (!string.IsNullOrEmpty(videoUrl))
        {
            try { videoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(videoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }
        if (!string.IsNullOrEmpty(passportUrl))
        {
            try { passportUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(passportUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }

        if (photoUrl != cv.PhotoUrl || videoUrl != cv.VideoUrl || passportUrl != cv.PassportDocumentUrl)
            cv = cv with { PhotoUrl = photoUrl, VideoUrl = videoUrl, PassportDocumentUrl = passportUrl };

        return Ok(cv);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("workers.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _workerService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region Supplier Enrichment

    private async Task<PagedList<WorkerListDto>> EnrichWithSupplierNames(
        Guid tenantId,
        PagedList<WorkerListDto> pagedList,
        CancellationToken ct)
    {
        var supplierIds = pagedList.Items
            .Where(w => w.TenantSupplierId.HasValue)
            .Select(w => w.TenantSupplierId!.Value)
            .Distinct()
            .ToList();

        if (supplierIds.Count == 0)
            return pagedList;

        var nameMap = await ResolveSupplierNames(tenantId, supplierIds, ct);

        var enriched = pagedList.Items.Select(w =>
            w.TenantSupplierId.HasValue && nameMap.TryGetValue(w.TenantSupplierId.Value, out var name)
                ? w with { Supplier = new WorkerSupplierDto { Id = w.TenantSupplierId.Value, Name = name } }
                : w
        ).ToList();

        return new PagedList<WorkerListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<WorkerDto> EnrichWithSupplierName(
        Guid tenantId,
        WorkerDto dto,
        CancellationToken ct)
    {
        if (!dto.TenantSupplierId.HasValue)
            return dto;

        var tsResult = await _supplierService.GetTenantSupplierByIdAsync(
            tenantId,
            dto.TenantSupplierId.Value,
            new QueryParameters { Include = "supplier" },
            ct);

        if (tsResult.IsSuccess && tsResult.Value!.Supplier is not null)
        {
            return dto with { Supplier = new WorkerSupplierDto { Id = dto.TenantSupplierId.Value, Name = tsResult.Value!.Supplier.NameEn } };
        }

        return dto;
    }

    private async Task<Dictionary<Guid, string>> ResolveSupplierNames(
        Guid tenantId,
        List<Guid> tenantSupplierIds,
        CancellationToken ct)
    {
        var qp = new QueryParameters { PageSize = tenantSupplierIds.Count, Include = "supplier" };
        var suppliers = await _supplierService.ListTenantSuppliersAsync(tenantId, qp, ct);

        return suppliers.Items
            .Where(ts => tenantSupplierIds.Contains(ts.Id) && ts.Supplier is not null)
            .ToDictionary(ts => ts.Id, ts => ts.Supplier!.NameEn);
    }

    #endregion

    #region Job Category Enrichment

    private async Task<WorkerDto> EnrichWithJobCategory(WorkerDto dto, CancellationToken ct)
    {
        if (!dto.JobCategoryId.HasValue)
            return dto;

        var result = await _jobCategoryService.GetByIdAsync(dto.JobCategoryId.Value, ct);
        if (result.IsSuccess)
            return dto with { JobCategory = new JobCategoryInfoDto(dto.JobCategoryId.Value, result.Value!.NameEn) };

        return dto;
    }

    private async Task<PagedList<WorkerListDto>> EnrichListWithJobCategories(
        PagedList<WorkerListDto> pagedList,
        CancellationToken ct)
    {
        var categoryIds = pagedList.Items
            .Where(w => w.JobCategoryId.HasValue)
            .Select(w => w.JobCategoryId!.Value)
            .Distinct()
            .ToList();

        if (categoryIds.Count == 0)
            return pagedList;

        var allCategories = await _jobCategoryService.GetAllAsync(ct);
        var categoryMap = allCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.NameEn);

        var enriched = pagedList.Items.Select(w =>
            w.JobCategoryId.HasValue && categoryMap.TryGetValue(w.JobCategoryId.Value, out var name)
                ? w with { JobCategory = new JobCategoryInfoDto(w.JobCategoryId.Value, name) }
                : w
        ).ToList();

        return new PagedList<WorkerListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    #endregion

    #region Presigned URL Enrichment

    private async Task<WorkerDto> EnrichWithPresignedUrls(WorkerDto dto, CancellationToken ct)
    {
        var photoUrl = dto.PhotoUrl;
        var videoUrl = dto.VideoUrl;
        var passportUrl = dto.PassportDocumentUrl;

        if (!string.IsNullOrEmpty(photoUrl))
        {
            try { photoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(photoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }
        if (!string.IsNullOrEmpty(videoUrl))
        {
            try { videoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(videoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }
        if (!string.IsNullOrEmpty(passportUrl))
        {
            try { passportUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(passportUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as storage key */ }
        }

        if (photoUrl != dto.PhotoUrl || videoUrl != dto.VideoUrl || passportUrl != dto.PassportDocumentUrl)
            return dto with { PhotoUrl = photoUrl, VideoUrl = videoUrl, PassportDocumentUrl = passportUrl };

        return dto;
    }

    private async Task<PagedList<WorkerListDto>> EnrichListWithPresignedUrls(
        PagedList<WorkerListDto> pagedList,
        CancellationToken ct)
    {
        var itemsWithPhotos = pagedList.Items.Where(w => !string.IsNullOrEmpty(w.PhotoUrl)).ToList();
        if (itemsWithPhotos.Count == 0)
            return pagedList;

        var urlMap = new Dictionary<string, string>();
        foreach (var item in itemsWithPhotos)
        {
            if (!urlMap.ContainsKey(item.PhotoUrl!))
            {
                try { urlMap[item.PhotoUrl!] = await _fileStorageService.GetPresignedDownloadUrlAsync(item.PhotoUrl!, TimeSpan.FromHours(1), ct); }
                catch { urlMap[item.PhotoUrl!] = item.PhotoUrl!; }
            }
        }

        var enriched = pagedList.Items.Select(w =>
            !string.IsNullOrEmpty(w.PhotoUrl) && urlMap.TryGetValue(w.PhotoUrl, out var presigned)
                ? w with { PhotoUrl = presigned }
                : w
        ).ToList();

        return new PagedList<WorkerListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
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
