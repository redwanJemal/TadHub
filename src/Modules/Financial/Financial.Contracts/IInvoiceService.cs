using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface IInvoiceService
{
    Task<PagedList<InvoiceListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<InvoiceDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);
    Task<Result<InvoiceDto>> CreateAsync(Guid tenantId, CreateInvoiceRequest request, CancellationToken ct = default);
    Task<Result<InvoiceDto>> GenerateForContractAsync(Guid tenantId, GenerateInvoiceRequest request, CancellationToken ct = default);
    Task<Result<InvoiceDto>> UpdateAsync(Guid tenantId, Guid id, UpdateInvoiceRequest request, CancellationToken ct = default);
    Task<Result<InvoiceDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionInvoiceStatusRequest request, CancellationToken ct = default);
    Task<Result<InvoiceDto>> CreateCreditNoteAsync(Guid tenantId, Guid invoiceId, CreateCreditNoteRequest request, CancellationToken ct = default);
    Task<Result<InvoiceDto>> ApplyDiscountAsync(Guid tenantId, Guid invoiceId, ApplyDiscountRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<InvoiceSummaryDto>> GetSummaryAsync(Guid tenantId, CancellationToken ct = default);
}
