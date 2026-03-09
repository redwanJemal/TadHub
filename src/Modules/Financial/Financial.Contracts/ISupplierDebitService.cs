using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface ISupplierDebitService
{
    Task<PagedList<SupplierDebitListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<SupplierDebitDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<SupplierDebitDto>> CreateAsync(Guid tenantId, CreateSupplierDebitRequest request, CancellationToken ct = default);
    Task<Result<SupplierDebitDto>> UpdateAsync(Guid tenantId, Guid id, UpdateSupplierDebitRequest request, CancellationToken ct = default);
    Task<Result<SupplierDebitDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionSupplierDebitStatusRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
