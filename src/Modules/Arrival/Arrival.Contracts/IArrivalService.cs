using Arrival.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Arrival.Contracts;

public interface IArrivalService
{
    Task<PagedList<ArrivalListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<ArrivalDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ScheduleArrivalAsync(Guid tenantId, ScheduleArrivalRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> UpdateAsync(Guid tenantId, Guid id, UpdateArrivalRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> AssignDriverAsync(Guid tenantId, Guid id, AssignDriverRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ConfirmArrivalAsync(Guid tenantId, Guid id, ConfirmArrivalRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ConfirmPickupAsync(Guid tenantId, Guid id, ConfirmPickupRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ConfirmAtAccommodationAsync(Guid tenantId, Guid id, ConfirmAccommodationRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ConfirmCustomerPickupAsync(Guid tenantId, Guid id, ConfirmCustomerPickupRequest request, CancellationToken ct = default);

    Task<Result<ArrivalDto>> ReportNoShowAsync(Guid tenantId, Guid id, ReportNoShowRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<List<ArrivalStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);

    Task<Result<ArrivalDto>> SetDriverPickupPhotoAsync(Guid tenantId, Guid id, string photoUrl, CancellationToken ct = default);
}
