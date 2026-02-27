using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Candidate.Contracts;
using Candidate.Contracts.DTOs;
using ReferenceData.Contracts;
using Supplier.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant-scoped candidate management endpoints.
/// Composes data from Candidate, Supplier, and ReferenceData modules at the API layer.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/candidates")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly ISupplierService _supplierService;
    private readonly IJobCategoryService _jobCategoryService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITenantFileService _tenantFileService;

    private static readonly string[] AllowedPhotoTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] AllowedVideoTypes = ["video/mp4", "video/webm"];
    private static readonly string[] AllowedPassportTypes = ["application/pdf", "image/jpeg", "image/png"];
    private const long MaxPhotoSize = 5 * 1024 * 1024; // 5MB
    private const long MaxVideoSize = 50 * 1024 * 1024; // 50MB
    private const long MaxPassportSize = 10 * 1024 * 1024; // 10MB

    public CandidatesController(
        ICandidateService candidateService,
        ISupplierService supplierService,
        IJobCategoryService jobCategoryService,
        IFileStorageService fileStorageService,
        ITenantFileService tenantFileService)
    {
        _candidateService = candidateService;
        _supplierService = supplierService;
        _jobCategoryService = jobCategoryService;
        _fileStorageService = fileStorageService;
        _tenantFileService = tenantFileService;
    }

    /// <summary>
    /// Lists candidates for this tenant with filtering, sorting, search, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[status]=Received,UnderReview
    /// - filter[sourceType]=Supplier
    /// - filter[nationality]=PH,IN
    /// - filter[tenantSupplierId]=guid
    /// - filter[gender]=Male
    /// - filter[createdBy]=guid
    /// - filter[passportNumber]=ABC123
    /// - filter[externalReference]=REF001
    /// - filter[jobCategoryId]=guid
    /// - filter[religion]=Islam
    /// - filter[maritalStatus]=Single
    /// - filter[educationLevel]=Bachelor
    ///
    /// Sort:
    /// - sort=-createdAt (default, newest first)
    /// - sort=fullNameEn
    /// - sort=status
    ///
    /// Search:
    /// - search=john (searches name, passport number, external reference)
    ///
    /// Include:
    /// - include=supplier (enriches response with supplier names)
    /// </remarks>
    [HttpGet]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(PagedList<CandidateListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _candidateService.ListAsync(tenantId, qp, ct);

        var includes = qp.GetIncludeList();
        if (includes.Contains("supplier", StringComparer.OrdinalIgnoreCase))
        {
            result = await EnrichWithSupplierNames(tenantId, result, ct);
        }

        result = await EnrichListWithJobCategories(result, ct);
        result = await EnrichListWithPresignedUrls(result, ct);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new candidate.
    /// </summary>
    [HttpPost]
    [HasPermission("candidates.create")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateCandidateRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            return MapResultError(result);
        }

        var candidateId = result.Value!.Id;

        // Attach deferred file uploads (from Create page)
        if (request.PhotoFileId.HasValue || request.PassportFileId.HasValue)
        {
            var updateRequest = new UpdateCandidateRequest();
            var fileIdsToAttach = new List<Guid>();

            if (request.PhotoFileId.HasValue)
            {
                var photoFile = await _tenantFileService.GetByIdAsync(request.PhotoFileId.Value, ct);
                if (photoFile is not null)
                {
                    // Look up the TenantFile to get the storage key
                    var tf = await _tenantFileService.GetByIdAsync(request.PhotoFileId.Value, ct);
                    // We need the storage key — retrieve via the entity directly
                    updateRequest = updateRequest with { PhotoUrl = await GetStorageKeyForFile(request.PhotoFileId.Value, ct) };
                    fileIdsToAttach.Add(request.PhotoFileId.Value);
                }
            }

            if (request.PassportFileId.HasValue)
            {
                updateRequest = updateRequest with { PassportDocumentUrl = await GetStorageKeyForFile(request.PassportFileId.Value, ct) };
                fileIdsToAttach.Add(request.PassportFileId.Value);
            }

            if (updateRequest.PhotoUrl is not null || updateRequest.PassportDocumentUrl is not null)
            {
                await _candidateService.UpdateAsync(tenantId, candidateId, updateRequest, ct);
            }

            if (fileIdsToAttach.Count > 0)
            {
                await _tenantFileService.AttachMultipleAsync(fileIdsToAttach, "Candidate", candidateId, ct);
            }

            // Re-fetch to get updated data
            var refreshed = await _candidateService.GetByIdAsync(tenantId, candidateId, new TadHub.SharedKernel.Api.QueryParameters(), ct);
            if (refreshed.IsSuccess)
            {
                var dto = await EnrichWithPresignedUrls(refreshed.Value!, ct);
                return CreatedAtAction(nameof(GetById), new { tenantId, id = candidateId }, dto);
            }
        }

        return CreatedAtAction(
            nameof(GetById),
            new { tenantId, id = candidateId },
            result.Value);
    }

    /// <summary>
    /// Gets a candidate by ID.
    /// </summary>
    /// <remarks>
    /// Include:
    /// - include=statusHistory (includes status change history)
    /// - include=supplier (includes supplier name)
    /// - include=statusHistory,supplier (both)
    /// </remarks>
    [HttpGet("{id:guid}")]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _candidateService.GetByIdAsync(tenantId, id, qp, ct);

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

    /// <summary>
    /// Updates a candidate (partial update).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [HasPermission("candidates.edit")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateCandidateRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Uploads a photo for a candidate.
    /// </summary>
    [HttpPost("{id:guid}/photo")]
    [HasPermission("candidates.edit")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(MaxPhotoSize + 1024)] // slight buffer for multipart overhead
    public async Task<IActionResult> UploadPhoto(
        Guid tenantId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Problem(ApiError.BadRequest("File is empty", HttpContext.Request.Path));

        if (file.Length > MaxPhotoSize)
            return Problem(ApiError.BadRequest("Photo must be less than 5MB", HttpContext.Request.Path));

        if (!AllowedPhotoTypes.Contains(file.ContentType.ToLower()))
            return Problem(ApiError.BadRequest("Photo must be JPEG, PNG, or WebP", HttpContext.Request.Path));

        await using var stream = file.OpenReadStream();
        var tenantFile = await _tenantFileService.UploadAsync(
            tenantId, file.FileName, stream, file.ContentType, file.Length, "photo", ct);

        await _tenantFileService.AttachToEntityAsync(tenantFile.Id, "Candidate", id, ct);

        var updateResult = await _candidateService.UpdateAsync(tenantId, id,
            new UpdateCandidateRequest { PhotoUrl = tenantFile.StorageKey }, ct);

        if (!updateResult.IsSuccess)
            return MapResultError(updateResult);

        var dto = await EnrichWithPresignedUrls(updateResult.Value!, ct);
        return Ok(dto);
    }

    /// <summary>
    /// Uploads a video for a candidate.
    /// </summary>
    [HttpPost("{id:guid}/video")]
    [HasPermission("candidates.edit")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(MaxVideoSize + 1024)]
    public async Task<IActionResult> UploadVideo(
        Guid tenantId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Problem(ApiError.BadRequest("File is empty", HttpContext.Request.Path));

        if (file.Length > MaxVideoSize)
            return Problem(ApiError.BadRequest("Video must be less than 50MB", HttpContext.Request.Path));

        if (!AllowedVideoTypes.Contains(file.ContentType.ToLower()))
            return Problem(ApiError.BadRequest("Video must be MP4 or WebM", HttpContext.Request.Path));

        await using var stream = file.OpenReadStream();
        var tenantFile = await _tenantFileService.UploadAsync(
            tenantId, file.FileName, stream, file.ContentType, file.Length, "video", ct);

        await _tenantFileService.AttachToEntityAsync(tenantFile.Id, "Candidate", id, ct);

        var updateResult = await _candidateService.UpdateAsync(tenantId, id,
            new UpdateCandidateRequest { VideoUrl = tenantFile.StorageKey }, ct);

        if (!updateResult.IsSuccess)
            return MapResultError(updateResult);

        var dto = await EnrichWithPresignedUrls(updateResult.Value!, ct);
        return Ok(dto);
    }

    /// <summary>
    /// Uploads a passport document for a candidate.
    /// </summary>
    [HttpPost("{id:guid}/passport")]
    [HasPermission("candidates.edit")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(MaxPassportSize + 1024)]
    public async Task<IActionResult> UploadPassport(
        Guid tenantId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Problem(ApiError.BadRequest("File is empty", HttpContext.Request.Path));

        if (file.Length > MaxPassportSize)
            return Problem(ApiError.BadRequest("Passport document must be less than 10MB", HttpContext.Request.Path));

        if (!AllowedPassportTypes.Contains(file.ContentType.ToLower()))
            return Problem(ApiError.BadRequest("Passport document must be PDF, JPEG, or PNG", HttpContext.Request.Path));

        // Upload via TenantFileService for tracking
        await using var stream = file.OpenReadStream();
        var tenantFile = await _tenantFileService.UploadAsync(
            tenantId, file.FileName, stream, file.ContentType, file.Length, "passport", ct);

        // Attach to candidate and set URL
        await _tenantFileService.AttachToEntityAsync(tenantFile.Id, "Candidate", id, ct);

        var storageKey = await GetStorageKeyForFile(tenantFile.Id, ct);
        var updateResult = await _candidateService.UpdateAsync(tenantId, id,
            new UpdateCandidateRequest { PassportDocumentUrl = storageKey }, ct);

        if (!updateResult.IsSuccess)
            return MapResultError(updateResult);

        var dto = await EnrichWithPresignedUrls(updateResult.Value!, ct);
        return Ok(dto);
    }

    /// <summary>
    /// Transitions a candidate's status.
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [HasPermission("candidates.manage_status")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionStatusRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the status history for a candidate.
    /// </summary>
    [HttpGet("{id:guid}/status-history")]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(List<CandidateStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _candidateService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Soft deletes a candidate.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("candidates.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _candidateService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region File Helpers

    private async Task<string?> GetStorageKeyForFile(Guid fileId, CancellationToken ct)
    {
        return await _tenantFileService.GetStorageKeyAsync(fileId, ct);
    }

    #endregion

    #region Supplier Enrichment

    /// <summary>
    /// Batch-enriches a paged list of candidates with supplier names.
    /// Calls ISupplierService (Supplier module contract) to resolve names.
    /// </summary>
    private async Task<PagedList<CandidateListDto>> EnrichWithSupplierNames(
        Guid tenantId,
        PagedList<CandidateListDto> pagedList,
        CancellationToken ct)
    {
        var supplierIds = pagedList.Items
            .Where(c => c.TenantSupplierId.HasValue)
            .Select(c => c.TenantSupplierId!.Value)
            .Distinct()
            .ToList();

        if (supplierIds.Count == 0)
            return pagedList;

        var nameMap = await ResolveSupplierNames(tenantId, supplierIds, ct);

        var enriched = pagedList.Items.Select(c =>
            c.TenantSupplierId.HasValue && nameMap.TryGetValue(c.TenantSupplierId.Value, out var name)
                ? c with { Supplier = new CandidateSupplierDto { Id = c.TenantSupplierId.Value, Name = name } }
                : c
        ).ToList();

        return new PagedList<CandidateListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    /// <summary>
    /// Enriches a single candidate DTO with supplier name.
    /// </summary>
    private async Task<CandidateDto> EnrichWithSupplierName(
        Guid tenantId,
        CandidateDto dto,
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
            return dto with { Supplier = new CandidateSupplierDto { Id = dto.TenantSupplierId.Value, Name = tsResult.Value!.Supplier.NameEn } };
        }

        return dto;
    }

    /// <summary>
    /// Resolves tenant-supplier IDs to supplier names via ISupplierService.
    /// </summary>
    private async Task<Dictionary<Guid, string>> ResolveSupplierNames(
        Guid tenantId,
        List<Guid> tenantSupplierIds,
        CancellationToken ct)
    {
        // Fetch all tenant suppliers with their supplier details
        var qp = new QueryParameters { PageSize = tenantSupplierIds.Count, Include = "supplier" };
        var suppliers = await _supplierService.ListTenantSuppliersAsync(tenantId, qp, ct);

        return suppliers.Items
            .Where(ts => tenantSupplierIds.Contains(ts.Id) && ts.Supplier is not null)
            .ToDictionary(ts => ts.Id, ts => ts.Supplier!.NameEn);
    }

    #endregion

    #region Job Category Enrichment

    private async Task<CandidateDto> EnrichWithJobCategory(CandidateDto dto, CancellationToken ct)
    {
        if (!dto.JobCategoryId.HasValue)
            return dto;

        var result = await _jobCategoryService.GetByIdAsync(dto.JobCategoryId.Value, ct);
        if (result.IsSuccess)
            return dto with { JobCategory = new JobCategoryInfoDto(dto.JobCategoryId.Value, result.Value!.NameEn) };

        return dto;
    }

    private async Task<PagedList<CandidateListDto>> EnrichListWithJobCategories(
        PagedList<CandidateListDto> pagedList,
        CancellationToken ct)
    {
        var categoryIds = pagedList.Items
            .Where(c => c.JobCategoryId.HasValue)
            .Select(c => c.JobCategoryId!.Value)
            .Distinct()
            .ToList();

        if (categoryIds.Count == 0)
            return pagedList;

        var allCategories = await _jobCategoryService.GetAllAsync(ct);
        var categoryMap = allCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.NameEn);

        var enriched = pagedList.Items.Select(c =>
            c.JobCategoryId.HasValue && categoryMap.TryGetValue(c.JobCategoryId.Value, out var name)
                ? c with { JobCategory = new JobCategoryInfoDto(c.JobCategoryId.Value, name) }
                : c
        ).ToList();

        return new PagedList<CandidateListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    #endregion

    #region Presigned URL Enrichment

    private async Task<CandidateDto> EnrichWithPresignedUrls(CandidateDto dto, CancellationToken ct)
    {
        var photoUrl = dto.PhotoUrl;
        var videoUrl = dto.VideoUrl;
        var passportUrl = dto.PassportDocumentUrl;

        if (!string.IsNullOrEmpty(photoUrl))
        {
            try { photoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(photoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as file key if presigning fails */ }
        }

        if (!string.IsNullOrEmpty(videoUrl))
        {
            try { videoUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(videoUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as file key if presigning fails */ }
        }

        if (!string.IsNullOrEmpty(passportUrl))
        {
            try { passportUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(passportUrl, TimeSpan.FromHours(1), ct); }
            catch { /* leave as file key if presigning fails */ }
        }

        if (photoUrl != dto.PhotoUrl || videoUrl != dto.VideoUrl || passportUrl != dto.PassportDocumentUrl)
            return dto with { PhotoUrl = photoUrl, VideoUrl = videoUrl, PassportDocumentUrl = passportUrl };

        return dto;
    }

    private async Task<PagedList<CandidateListDto>> EnrichListWithPresignedUrls(
        PagedList<CandidateListDto> pagedList,
        CancellationToken ct)
    {
        var itemsWithPhotos = pagedList.Items.Where(c => !string.IsNullOrEmpty(c.PhotoUrl)).ToList();
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

        var enriched = pagedList.Items.Select(c =>
            !string.IsNullOrEmpty(c.PhotoUrl) && urlMap.TryGetValue(c.PhotoUrl, out var presigned)
                ? c with { PhotoUrl = presigned }
                : c
        ).ToList();

        return new PagedList<CandidateListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
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

    private IActionResult Problem(ApiError error)
    {
        return new ObjectResult(error) { StatusCode = error.Status, ContentTypes = { "application/problem+json" } };
    }

    #endregion
}
