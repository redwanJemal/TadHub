using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface ISupplierPaymentService
{
    Task<PagedList<SupplierPaymentListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<SupplierPaymentDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<SupplierPaymentDto>> CreateAsync(Guid tenantId, CreateSupplierPaymentRequest request, CancellationToken ct = default);
    Task<Result<SupplierPaymentDto>> UpdateAsync(Guid tenantId, Guid id, UpdateSupplierPaymentRequest request, CancellationToken ct = default);
    Task<Result<SupplierPaymentDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionSupplierPaymentStatusRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
