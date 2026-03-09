using Reporting.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Reporting.Contracts;

public interface IReportService
{
    // Workforce
    Task<PagedList<InventoryReportItemDto>> GetInventoryReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<DeployedReportItemDto>> GetDeployedReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<ReturneeReportItemDto>> GetReturneeReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<RunawayReportItemDto>> GetRunawayReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    // Operational
    Task<PagedList<ArrivalReportItemDto>> GetArrivalsReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<AccommodationDailyItemDto>> GetAccommodationDailyListAsync(Guid tenantId, DateOnly date, QueryParameters qp, CancellationToken ct = default);
    Task<List<DeploymentPipelineItemDto>> GetDeploymentPipelineAsync(Guid tenantId, CancellationToken ct = default);

    // Finance Extensions
    Task<PagedList<SupplierCommissionItemDto>> GetSupplierCommissionReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<RefundReportItemDto>> GetRefundReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<CostPerMaidItemDto>> GetCostPerMaidReportAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
}
