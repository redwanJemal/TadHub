using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface IPaymentService
{
    Task<PagedList<PaymentListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<PaymentDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<PaymentDto>> RecordPaymentAsync(Guid tenantId, RecordPaymentRequest request, CancellationToken ct = default);
    Task<Result<PaymentDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionPaymentStatusRequest request, CancellationToken ct = default);
    Task<Result<PaymentDto>> RefundPaymentAsync(Guid tenantId, Guid id, RefundPaymentRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
