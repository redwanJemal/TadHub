using Financial.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Financial.Contracts;

public interface IFinancialReportService
{
    Task<Result<MarginReportDto>> GetMarginReportAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<CashReconciliationDto>> GenerateXReportAsync(Guid tenantId, DateOnly? reportDate = null, CancellationToken ct = default);
    Task<Result<CashReconciliationDto>> CloseXReportAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PagedList<CashReconciliationListDto>> ListXReportsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<RevenueBreakdownDto>> GetRevenueBreakdownAsync(Guid tenantId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
}
