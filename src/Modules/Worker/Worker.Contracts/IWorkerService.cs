using Worker.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Worker.Contracts;

/// <summary>
/// Service for managing workers within a tenant.
/// Workers are created automatically when candidates are converted — no CreateAsync method.
/// </summary>
public interface IWorkerService
{
    Task<PagedList<WorkerListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<WorkerDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<WorkerDto>> UpdateAsync(Guid tenantId, Guid id, UpdateWorkerRequest request, CancellationToken ct = default);

    Task<Result<WorkerDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionWorkerStatusRequest request, CancellationToken ct = default);

    Task<Result<List<WorkerStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<WorkerCvDto>> GetCvAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
