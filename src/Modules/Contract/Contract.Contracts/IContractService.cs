using Contract.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Contract.Contracts;

public interface IContractService
{
    Task<PagedList<ContractListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<ContractDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<ContractDto>> CreateAsync(Guid tenantId, CreateContractRequest request, CancellationToken ct = default);

    Task<Result<ContractDto>> UpdateAsync(Guid tenantId, Guid id, UpdateContractRequest request, CancellationToken ct = default);

    Task<Result<ContractDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionContractStatusRequest request, CancellationToken ct = default);

    Task<Result<List<ContractStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
