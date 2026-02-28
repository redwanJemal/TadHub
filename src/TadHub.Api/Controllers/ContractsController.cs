using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Contract.Contracts;
using Contract.Contracts.DTOs;
using Worker.Contracts;
using Worker.Contracts.DTOs;
using Client.Contracts;
using Tenancy.Contracts;
using TadHub.Api.Documents;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/contracts")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;
    private readonly ITenantService _tenantService;
    private readonly IFileStorageService _fileStorageService;

    public ContractsController(
        IContractService contractService,
        IWorkerService workerService,
        IClientService clientService,
        ITenantService tenantService,
        IFileStorageService fileStorageService)
    {
        _contractService = contractService;
        _workerService = workerService;
        _clientService = clientService;
        _tenantService = tenantService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [HasPermission("contracts.view")]
    [ProducesResponseType(typeof(PagedList<ContractListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _contractService.ListAsync(tenantId, qp, ct);

        // Enrich with worker and client info
        result = await EnrichListWithParties(tenantId, result, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("contracts.view")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _contractService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("contracts.create")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateContractRequest request,
        CancellationToken ct)
    {
        var result = await _contractService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("contracts.edit")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateContractRequest request,
        CancellationToken ct)
    {
        var result = await _contractService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/status")]
    [HasPermission("contracts.manage_status")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionContractStatusRequest request,
        CancellationToken ct)
    {
        var result = await _contractService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("contracts.view")]
    [ProducesResponseType(typeof(List<ContractStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _contractService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/pdf")]
    [HasPermission("contracts.view")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _contractService.GetByIdAsync(tenantId, id, ct: ct);
        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = await EnrichWithParties(tenantId, result.Value!, ct);

        // Get tenant info for branding
        var tenantResult = await _tenantService.GetByIdAsync(tenantId, ct);
        var tenantName = tenantResult.IsSuccess ? tenantResult.Value!.Name : "TadHub";
        var tenantNameAr = tenantResult.IsSuccess ? tenantResult.Value!.NameAr : null;

        // Download tenant logo from MinIO
        var logoKey = tenantResult.IsSuccess ? tenantResult.Value!.LogoUrl : null;
        byte[]? tenantLogo = null;
        if (!string.IsNullOrEmpty(logoKey))
        {
            try { tenantLogo = await _fileStorageService.DownloadAsync(logoKey, ct); }
            catch { /* leave null */ }
        }

        var pdfData = new ContractPdfData(dto, tenantName, tenantNameAr, tenantLogo);
        var document = new ContractDocument(pdfData);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"Contract-{dto.ContractCode}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("contracts.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _contractService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<ContractListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<ContractListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(c => c.WorkerId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(c => c.ClientId).Distinct().ToList();

        // Fetch workers
        var workerMap = new Dictionary<Guid, ContractWorkerDto>();
        if (workerIds.Count > 0)
        {
            var workersResult = await _workerService.ListAsync(tenantId, new QueryParameters { PageSize = workerIds.Count }, ct);
            foreach (var w in workersResult.Items.Where(w => workerIds.Contains(w.Id)))
            {
                workerMap[w.Id] = new ContractWorkerDto
                {
                    Id = w.Id,
                    FullNameEn = w.FullNameEn,
                    FullNameAr = w.FullNameAr,
                    WorkerCode = w.WorkerCode,
                };
            }
        }

        // Fetch clients
        var clientMap = new Dictionary<Guid, ContractClientDto>();
        if (clientIds.Count > 0)
        {
            var clientsResult = await _clientService.ListAsync(tenantId, new QueryParameters { PageSize = clientIds.Count }, ct);
            foreach (var c in clientsResult.Items.Where(c => clientIds.Contains(c.Id)))
            {
                clientMap[c.Id] = new ContractClientDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                };
            }
        }

        var enriched = pagedList.Items.Select(c => c with
        {
            Worker = workerMap.GetValueOrDefault(c.WorkerId),
            Client = clientMap.GetValueOrDefault(c.ClientId),
        }).ToList();

        return new PagedList<ContractListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<ContractDto> EnrichWithParties(
        Guid tenantId,
        ContractDto dto,
        CancellationToken ct)
    {
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            dto = dto with
            {
                Worker = new ContractWorkerDto
                {
                    Id = w.Id,
                    FullNameEn = w.FullNameEn,
                    FullNameAr = w.FullNameAr,
                    WorkerCode = w.WorkerCode,
                },
            };
        }

        var clientResult = await _clientService.GetByIdAsync(tenantId, dto.ClientId, ct);
        if (clientResult.IsSuccess)
        {
            var c = clientResult.Value!;
            dto = dto with
            {
                Client = new ContractClientDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
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
