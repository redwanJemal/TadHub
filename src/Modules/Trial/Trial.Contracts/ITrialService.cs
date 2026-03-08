using Trial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Trial.Contracts;

public interface ITrialService
{
    Task<PagedList<TrialListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<TrialDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<TrialDto>> CreateAsync(Guid tenantId, CreateTrialRequest request, CancellationToken ct = default);

    Task<Result<TrialDto>> CompleteAsync(Guid tenantId, Guid id, CompleteTrialRequest request, CancellationToken ct = default);

    Task<Result<TrialDto>> CancelAsync(Guid tenantId, Guid id, CancelTrialRequest request, CancellationToken ct = default);

    Task<Result<List<TrialStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
