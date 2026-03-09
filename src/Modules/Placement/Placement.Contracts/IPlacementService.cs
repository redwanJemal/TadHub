using Placement.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Placement.Contracts;

public interface IPlacementService
{
    Task<PagedList<PlacementListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<PlacementDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<PlacementDto>> CreateAsync(Guid tenantId, CreatePlacementRequest request, CancellationToken ct = default);

    Task<Result<PlacementDto>> UpdateAsync(Guid tenantId, Guid id, UpdatePlacementRequest request, CancellationToken ct = default);

    Task<Result<PlacementDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionPlacementStatusRequest request, CancellationToken ct = default);

    Task<Result<PlacementDto>> AdvanceStepAsync(Guid tenantId, Guid id, AdvancePlacementStepRequest request, CancellationToken ct = default);

    Task<Result<PlacementChecklistDto>> GetChecklistAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<List<PlacementStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);

    Task<Result<PlacementBoardDto>> GetBoardAsync(Guid tenantId, CancellationToken ct = default);

    // Cost items
    Task<Result<PlacementCostItemDto>> AddCostItemAsync(Guid tenantId, Guid placementId, CreatePlacementCostItemRequest request, CancellationToken ct = default);

    Task<Result<PlacementCostItemDto>> UpdateCostItemAsync(Guid tenantId, Guid placementId, Guid itemId, UpdatePlacementCostItemRequest request, CancellationToken ct = default);

    Task<Result> DeleteCostItemAsync(Guid tenantId, Guid placementId, Guid itemId, CancellationToken ct = default);
}
