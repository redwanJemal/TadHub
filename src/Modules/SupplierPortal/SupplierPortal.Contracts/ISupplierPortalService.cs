using SupplierPortal.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace SupplierPortal.Contracts;

/// <summary>
/// Service for supplier portal operations — scoped to the authenticated supplier user.
/// </summary>
public interface ISupplierPortalService
{
    // Supplier user management
    Task<Result<SupplierUserDto>> GetSupplierUserByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<SupplierUserDto>> CreateSupplierUserAsync(CreateSupplierUserRequest request, CancellationToken ct = default);
    Task<Result<SupplierUserDto>> UpdateSupplierUserAsync(Guid id, UpdateSupplierUserRequest request, CancellationToken ct = default);
    Task<PagedList<SupplierUserDto>> ListSupplierUsersAsync(QueryParameters qp, CancellationToken ct = default);

    // Dashboard
    Task<Result<SupplierDashboardDto>> GetDashboardAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);

    // Candidates (supplier-scoped)
    Task<PagedList<SupplierCandidateListDto>> ListCandidatesAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default);

    // Workers (supplier-scoped)
    Task<PagedList<SupplierWorkerListDto>> ListWorkersAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default);

    // Commissions (supplier-scoped)
    Task<PagedList<SupplierCommissionDto>> ListCommissionsAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default);

    // Arrivals (supplier-scoped)
    Task<PagedList<SupplierArrivalListDto>> ListArrivalsAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default);
}
