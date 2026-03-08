using Visa.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Visa.Contracts;

public interface IVisaApplicationService
{
    Task<PagedList<VisaApplicationListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<VisaApplicationDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<VisaApplicationDto>> CreateAsync(Guid tenantId, CreateVisaApplicationRequest request, CancellationToken ct = default);

    Task<Result<VisaApplicationDto>> UpdateAsync(Guid tenantId, Guid id, UpdateVisaApplicationRequest request, CancellationToken ct = default);

    Task<Result<VisaApplicationDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionVisaStatusRequest request, CancellationToken ct = default);

    Task<Result<List<VisaApplicationStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<VisaApplicationDocumentDto>> UploadDocumentAsync(Guid tenantId, Guid id, UploadVisaDocumentRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<List<VisaApplicationListDto>>> GetByWorkerAsync(Guid tenantId, Guid workerId, CancellationToken ct = default);
}
