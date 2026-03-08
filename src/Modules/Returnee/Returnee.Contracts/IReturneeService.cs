using Returnee.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Returnee.Contracts;

public interface IReturneeService
{
    Task<PagedList<ReturneeCaseListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<ReturneeCaseDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);
    Task<Result<ReturneeCaseDto>> CreateAsync(Guid tenantId, CreateReturneeCaseRequest request, CancellationToken ct = default);
    Task<Result<ReturneeCaseDto>> ApproveAsync(Guid tenantId, Guid id, ApproveReturneeCaseRequest request, CancellationToken ct = default);
    Task<Result<ReturneeCaseDto>> RejectAsync(Guid tenantId, Guid id, RejectReturneeCaseRequest request, CancellationToken ct = default);
    Task<Result<ReturneeCaseDto>> SettleAsync(Guid tenantId, Guid id, SettleReturneeCaseRequest request, CancellationToken ct = default);
    Task<Result<ReturneeExpenseDto>> AddExpenseAsync(Guid tenantId, Guid caseId, CreateReturneeExpenseRequest request, CancellationToken ct = default);
    Task<Result<RefundCalculationDto>> CalculateRefundAsync(Guid tenantId, Guid caseId, CancellationToken ct = default);
    Task<Result<List<ReturneeCaseStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
