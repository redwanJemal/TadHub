using Accommodation.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Accommodation.Contracts;

public interface IAccommodationService
{
    Task<PagedList<AccommodationStayListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<AccommodationStayDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<AccommodationStayDto>> CheckInAsync(Guid tenantId, CheckInRequest request, CancellationToken ct = default);

    Task<Result<AccommodationStayDto>> CheckOutAsync(Guid tenantId, Guid id, CheckOutRequest request, CancellationToken ct = default);

    Task<Result<AccommodationStayDto>> UpdateAsync(Guid tenantId, Guid id, UpdateStayRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<PagedList<AccommodationStayListDto>> GetCurrentOccupantsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<PagedList<AccommodationStayListDto>> GetDailyListAsync(Guid tenantId, DateOnly date, QueryParameters qp, CancellationToken ct = default);

    Task<PagedList<AccommodationStayListDto>> GetStayHistoryByWorkerAsync(Guid tenantId, Guid workerId, QueryParameters qp, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
