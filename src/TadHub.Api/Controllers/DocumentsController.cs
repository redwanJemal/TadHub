using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Document.Contracts;
using Worker.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IWorkerService _workerService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITenantFileService _tenantFileService;

    public DocumentsController(
        IDocumentService documentService,
        IWorkerService workerService,
        IFileStorageService fileStorageService,
        ITenantFileService tenantFileService)
    {
        _documentService = documentService;
        _workerService = workerService;
        _fileStorageService = fileStorageService;
        _tenantFileService = tenantFileService;
    }

    // ──── Per-Worker endpoints ────

    [HttpGet("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents")]
    [HasPermission("documents.view")]
    [ProducesResponseType(typeof(PagedList<WorkerDocumentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByWorker(
        Guid tenantId,
        Guid workerId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _documentService.ListByWorkerAsync(tenantId, workerId, qp, ct);
        return Ok(result);
    }

    [HttpGet("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents/{id:guid}")]
    [HasPermission("documents.view")]
    [ProducesResponseType(typeof(WorkerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid workerId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _documentService.GetByIdAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;

        // Enrich file URL with presigned download URL
        if (!string.IsNullOrEmpty(dto.FileUrl))
        {
            try
            {
                var presignedUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(dto.FileUrl, cancellationToken: ct);
                dto = dto with { FileUrl = presignedUrl };
            }
            catch { /* leave as storage key */ }
        }

        return Ok(dto);
    }

    [HttpPost("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents")]
    [HasPermission("documents.create")]
    [ProducesResponseType(typeof(WorkerDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        Guid workerId,
        [FromBody] CreateWorkerDocumentRequest request,
        CancellationToken ct)
    {
        // Ensure the request workerId matches the route
        var req = request with { WorkerId = workerId };
        var result = await _documentService.CreateAsync(tenantId, req, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return CreatedAtAction(nameof(GetById),
            new { tenantId, workerId, id = result.Value!.Id },
            result.Value);
    }

    [HttpPatch("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents/{id:guid}")]
    [HasPermission("documents.edit")]
    [ProducesResponseType(typeof(WorkerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid workerId,
        Guid id,
        [FromBody] UpdateWorkerDocumentRequest request,
        CancellationToken ct)
    {
        var result = await _documentService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents/{id:guid}/file")]
    [HasPermission("documents.edit")]
    [ProducesResponseType(typeof(WorkerDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> UploadFile(
        Guid tenantId,
        Guid workerId,
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        var docResult = await _documentService.GetByIdAsync(tenantId, id, ct);
        if (!docResult.IsSuccess)
            return MapResultError(docResult);

        // Upload file via TenantFileService
        var fileResult = await _tenantFileService.UploadAsync(
            tenantId, file.FileName, file.OpenReadStream(),
            file.ContentType, file.Length, "document", ct);

        // Attach to entity
        await _tenantFileService.AttachToEntityAsync(fileResult.Id, "WorkerDocument", id, ct);

        // Store the storage key on the document so presigned URLs can be generated at read time
        await _documentService.SetFileUrlAsync(tenantId, id, fileResult.StorageKey, ct);

        var updated = await _documentService.GetByIdAsync(tenantId, id, ct);
        return Ok(updated.IsSuccess ? updated.Value : docResult.Value);
    }

    [HttpDelete("api/v1/tenants/{tenantId:guid}/workers/{workerId:guid}/documents/{id:guid}")]
    [HasPermission("documents.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid workerId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _documentService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    // ──── Tenant-wide endpoints ────

    [HttpGet("api/v1/tenants/{tenantId:guid}/documents")]
    [HasPermission("documents.view")]
    [ProducesResponseType(typeof(PagedList<WorkerDocumentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAll(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _documentService.ListAllAsync(tenantId, qp, ct);

        // Enrich with worker names
        result = await EnrichWithWorkerInfo(tenantId, result, ct);

        return Ok(result);
    }

    [HttpGet("api/v1/tenants/{tenantId:guid}/documents/expiring")]
    [HasPermission("documents.view")]
    [ProducesResponseType(typeof(PagedList<WorkerDocumentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiring(
        Guid tenantId,
        [FromQuery] int days = 30,
        [FromQuery] QueryParameters qp = default!,
        CancellationToken ct = default)
    {
        var result = await _documentService.GetExpiringDocumentsAsync(tenantId, days, qp, ct);
        result = await EnrichWithWorkerInfo(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("api/v1/tenants/{tenantId:guid}/documents/compliance")]
    [HasPermission("documents.view")]
    [ProducesResponseType(typeof(ComplianceSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompliance(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _documentService.GetComplianceSummaryAsync(tenantId, ct);
        return Ok(result);
    }

    // ──── BFF Enrichment ────

    private async Task<PagedList<WorkerDocumentListDto>> EnrichWithWorkerInfo(
        Guid tenantId,
        PagedList<WorkerDocumentListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(d => d.WorkerId).Distinct().ToList();
        if (workerIds.Count == 0) return pagedList;

        var workerMap = new Dictionary<Guid, (string Name, string Code)>();
        var workersResult = await _workerService.ListAsync(tenantId,
            new QueryParameters { PageSize = workerIds.Count }, ct);

        foreach (var w in workersResult.Items.Where(w => workerIds.Contains(w.Id)))
        {
            workerMap[w.Id] = (w.FullNameEn, w.WorkerCode);
        }

        var enriched = pagedList.Items.Select(d => d with
        {
            WorkerName = workerMap.GetValueOrDefault(d.WorkerId).Name,
            WorkerCode = workerMap.GetValueOrDefault(d.WorkerId).Code,
        }).ToList();

        return new PagedList<WorkerDocumentListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    // ──── Error Helpers ────

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
}
