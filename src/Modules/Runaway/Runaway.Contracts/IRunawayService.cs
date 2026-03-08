using Runaway.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Runaway.Contracts;

public interface IRunawayService
{
    Task<PagedList<RunawayCaseListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> ReportAsync(Guid tenantId, ReportRunawayCaseRequest request, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> UpdateAsync(Guid tenantId, Guid id, UpdateRunawayCaseRequest request, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> ConfirmAsync(Guid tenantId, Guid id, ConfirmRunawayCaseRequest request, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> SettleAsync(Guid tenantId, Guid id, SettleRunawayCaseRequest request, CancellationToken ct = default);
    Task<Result<RunawayCaseDto>> CloseAsync(Guid tenantId, Guid id, CloseRunawayCaseRequest request, CancellationToken ct = default);
    Task<Result<RunawayExpenseDto>> AddExpenseAsync(Guid tenantId, Guid caseId, CreateRunawayExpenseRequest request, CancellationToken ct = default);
    Task<Result<List<RunawayCaseStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
