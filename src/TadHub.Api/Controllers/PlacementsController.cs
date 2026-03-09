using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Placement.Contracts;
using Placement.Contracts.DTOs;
using Candidate.Contracts;
using Worker.Contracts;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/placements")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class PlacementsController : ControllerBase
{
    private readonly IPlacementService _placementService;
    private readonly ICandidateService _candidateService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;
    private readonly IFileStorageService _fileStorageService;

    public PlacementsController(
        IPlacementService placementService,
        ICandidateService candidateService,
        IWorkerService workerService,
        IClientService clientService,
        IFileStorageService fileStorageService)
    {
        _placementService = placementService;
        _candidateService = candidateService;
        _workerService = workerService;
        _clientService = clientService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    [HasPermission("placements.view")]
    [ProducesResponseType(typeof(PagedList<PlacementListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _placementService.ListAsync(tenantId, qp, ct);
        result = await EnrichListWithParties(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("placements.view")]
    [ProducesResponseType(typeof(PlacementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _placementService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("placements.create")]
    [ProducesResponseType(typeof(PlacementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreatePlacementRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(typeof(PlacementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdatePlacementRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/transition")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(typeof(PlacementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionPlacementStatusRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/advance-step")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(typeof(PlacementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdvanceStep(
        Guid tenantId,
        Guid id,
        [FromBody] AdvancePlacementStepRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.AdvanceStepAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/checklist")]
    [HasPermission("placements.view")]
    [ProducesResponseType(typeof(PlacementChecklistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChecklist(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _placementService.GetChecklistAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("placements.view")]
    [ProducesResponseType(typeof(List<PlacementStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _placementService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("board")]
    [HasPermission("placements.view")]
    [ProducesResponseType(typeof(PlacementBoardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoard(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _placementService.GetBoardAsync(tenantId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        // Enrich all board items with candidate/client info
        var board = result.Value!;
        var enrichedColumns = new Dictionary<string, List<PlacementListDto>>();
        foreach (var (status, items) in board.Columns)
        {
            if (items.Count == 0)
            {
                enrichedColumns[status] = items;
                continue;
            }

            var pagedList = new PagedList<PlacementListDto>(items, items.Count, 1, items.Count);
            var enriched = await EnrichListWithParties(tenantId, pagedList, ct);
            enrichedColumns[status] = enriched.Items.ToList();
        }

        return Ok(new PlacementBoardDto
        {
            StatusCounts = board.StatusCounts,
            Columns = enrichedColumns,
        });
    }

    // Cost items
    [HttpPost("{id:guid}/cost-items")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(typeof(PlacementCostItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCostItem(
        Guid tenantId,
        Guid id,
        [FromBody] CreatePlacementCostItemRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.AddCostItemAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(201, result.Value);
    }

    [HttpPatch("{id:guid}/cost-items/{itemId:guid}")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(typeof(PlacementCostItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCostItem(
        Guid tenantId,
        Guid id,
        Guid itemId,
        [FromBody] UpdatePlacementCostItemRequest request,
        CancellationToken ct)
    {
        var result = await _placementService.UpdateCostItemAsync(tenantId, id, itemId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}/cost-items/{itemId:guid}")]
    [HasPermission("placements.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCostItem(
        Guid tenantId,
        Guid id,
        Guid itemId,
        CancellationToken ct)
    {
        var result = await _placementService.DeleteCostItemAsync(tenantId, id, itemId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("placements.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _placementService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<PlacementListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<PlacementListDto> pagedList,
        CancellationToken ct)
    {
        var candidateIds = pagedList.Items.Select(p => p.CandidateId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(p => p.ClientId).Distinct().ToList();

        // Fetch sequentially — DbContext is not thread-safe for concurrent operations
        var candidateMap = new Dictionary<Guid, PlacementCandidateDto>();
        var clientMap = new Dictionary<Guid, PlacementClientDto>();
        var photoUrlMap = new Dictionary<string, string>();

        foreach (var id in candidateIds)
        {
            var result = await _candidateService.GetByIdAsync(tenantId, id, ct: ct);
            if (!result.IsSuccess) continue;
            var c = result.Value!;

            string? presignedPhoto = null;
            if (!string.IsNullOrEmpty(c.PhotoUrl))
            {
                if (!photoUrlMap.TryGetValue(c.PhotoUrl, out presignedPhoto))
                {
                    try { presignedPhoto = await _fileStorageService.GetPresignedDownloadUrlAsync(c.PhotoUrl, TimeSpan.FromHours(1), ct); }
                    catch { presignedPhoto = c.PhotoUrl; }
                    photoUrlMap[c.PhotoUrl] = presignedPhoto;
                }
            }
            candidateMap[c.Id] = new PlacementCandidateDto
            {
                Id = c.Id,
                FullNameEn = c.FullNameEn,
                FullNameAr = c.FullNameAr,
                Nationality = c.Nationality,
                PhotoUrl = presignedPhoto,
            };
        }

        foreach (var id in clientIds)
        {
            var result = await _clientService.GetByIdAsync(tenantId, id, ct);
            if (!result.IsSuccess) continue;
            var c = result.Value!;

            clientMap[c.Id] = new PlacementClientDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
            };
        }

        var enriched = pagedList.Items.Select(p => p with
        {
            Candidate = candidateMap.GetValueOrDefault(p.CandidateId),
            Client = clientMap.GetValueOrDefault(p.ClientId),
        }).ToList();

        return new PagedList<PlacementListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<PlacementDto> EnrichWithParties(
        Guid tenantId,
        PlacementDto dto,
        CancellationToken ct)
    {
        // Fetch sequentially — DbContext is not thread-safe for concurrent operations
        var candidateResult = await _candidateService.GetByIdAsync(tenantId, dto.CandidateId, ct: ct);
        if (candidateResult.IsSuccess)
        {
            var c = candidateResult.Value!;
            string? presignedPhoto = null;
            if (!string.IsNullOrEmpty(c.PhotoUrl))
            {
                try { presignedPhoto = await _fileStorageService.GetPresignedDownloadUrlAsync(c.PhotoUrl, TimeSpan.FromHours(1), ct); }
                catch { presignedPhoto = c.PhotoUrl; }
            }
            dto = dto with
            {
                Candidate = new PlacementCandidateDto
                {
                    Id = c.Id,
                    FullNameEn = c.FullNameEn,
                    FullNameAr = c.FullNameAr,
                    Nationality = c.Nationality,
                    PhotoUrl = presignedPhoto,
                },
            };
        }

        var clientResult = await _clientService.GetByIdAsync(tenantId, dto.ClientId, ct);
        if (clientResult.IsSuccess)
        {
            var c = clientResult.Value!;
            dto = dto with
            {
                Client = new PlacementClientDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                },
            };
        }

        if (dto.WorkerId.HasValue)
        {
            var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId.Value, ct: ct);
            if (workerResult.IsSuccess)
            {
                var w = workerResult.Value!;
                dto = dto with
                {
                    Worker = new PlacementWorkerDto
                    {
                        Id = w.Id,
                        FullNameEn = w.FullNameEn,
                        FullNameAr = w.FullNameAr,
                        WorkerCode = w.WorkerCode,
                    },
                };
            }
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
