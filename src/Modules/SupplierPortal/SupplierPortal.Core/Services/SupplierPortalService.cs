using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupplierPortal.Contracts;
using SupplierPortal.Contracts.DTOs;
using SupplierPortal.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace SupplierPortal.Core.Services;

public class SupplierPortalService : ISupplierPortalService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SupplierPortalService> _logger;

    public SupplierPortalService(AppDbContext db, ILogger<SupplierPortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Supplier User Management

    public async Task<Result<SupplierUserDto>> GetSupplierUserByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Set<SupplierUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (user is null)
            return Result<SupplierUserDto>.NotFound("Supplier user not found");

        var dto = await MapToDto(user, ct);
        return Result<SupplierUserDto>.Success(dto);
    }

    public async Task<Result<SupplierUserDto>> CreateSupplierUserAsync(CreateSupplierUserRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Set<SupplierUser>()
            .AnyAsync(x => x.UserId == request.UserId, ct);

        if (exists)
            return Result<SupplierUserDto>.Conflict("A supplier user already exists for this user ID");

        var entity = new SupplierUser
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SupplierId = request.SupplierId,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = true,
        };

        _db.Set<SupplierUser>().Add(entity);
        await _db.SaveChangesAsync(ct);

        var dto = await MapToDto(entity, ct);
        return Result<SupplierUserDto>.Success(dto);
    }

    public async Task<Result<SupplierUserDto>> UpdateSupplierUserAsync(Guid id, UpdateSupplierUserRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Set<SupplierUser>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            return Result<SupplierUserDto>.NotFound("Supplier user not found");

        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.DisplayName is not null) entity.DisplayName = request.DisplayName;
        if (request.Email is not null) entity.Email = request.Email;
        if (request.Phone is not null) entity.Phone = request.Phone;

        await _db.SaveChangesAsync(ct);

        var dto = await MapToDto(entity, ct);
        return Result<SupplierUserDto>.Success(dto);
    }

    public async Task<PagedList<SupplierUserDto>> ListSupplierUsersAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<SupplierUser>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x =>
                (x.DisplayName != null && x.DisplayName.ToLower().Contains(search)) ||
                (x.Email != null && x.Email.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(qp.Skip)
            .Take(qp.PageSize)
            .ToListAsync(ct);

        // Bulk-load supplier names to avoid N+1 queries
        var supplierIds = items.Select(x => x.SupplierId).Distinct().ToList();
        var supplierNameMap = new Dictionary<Guid, (string? NameEn, string? NameAr)>();
        if (supplierIds.Count > 0)
        {
            var supplierNames = await _db.Database
                .SqlQueryRaw<SupplierNameWithIdRow>(
                    "SELECT id AS \"Id\", name_en AS \"NameEn\", name_ar AS \"NameAr\" FROM suppliers WHERE id = ANY({0})",
                    supplierIds.ToArray())
                .ToListAsync(ct);

            foreach (var s in supplierNames)
            {
                supplierNameMap[s.Id] = (s.NameEn, s.NameAr);
            }
        }

        var dtos = items.Select(item => MapToDto(item, supplierNameMap)).ToList();

        return new PagedList<SupplierUserDto>(dtos, totalCount, qp.Page, qp.PageSize);
    }

    #endregion

    #region Dashboard

    public async Task<Result<SupplierDashboardDto>> GetDashboardAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default)
    {
        var tenantSupplierIds = await GetTenantSupplierIdsAsync(tenantId, supplierId, ct);

        if (tenantSupplierIds.Count == 0)
            return Result<SupplierDashboardDto>.Success(new SupplierDashboardDto());

        // Build parameterized IN clause: {1}, {2}, ...
        var (inClause, allParams) = BuildInClause(tenantSupplierIds, startIndex: 1);
        var paramsWithTenant = new object[] { tenantId }.Concat(allParams).ToArray();

        // Candidate counts by status via raw SQL
        var candidateSql = "SELECT status AS \"Status\", COUNT(*)::int AS \"Count\" FROM candidates WHERE tenant_id = {0} AND tenant_supplier_id IN (" + inClause + ") AND is_deleted = false GROUP BY status";
        var candidateCounts = await _db.Database
            .SqlQueryRaw<StatusCount>(candidateSql, paramsWithTenant)
            .ToListAsync(ct);

        var totalCandidates = candidateCounts.Sum(x => x.Count);
        var pendingCandidates = candidateCounts.Where(x => x.Status == "Registered" || x.Status == "UnderReview").Sum(x => x.Count);
        var approvedCandidates = candidateCounts.Where(x => x.Status == "Approved").Sum(x => x.Count);
        var rejectedCandidates = candidateCounts.Where(x => x.Status == "Rejected").Sum(x => x.Count);

        // Worker counts
        var workerSql = "SELECT status AS \"Status\", COUNT(*)::int AS \"Count\" FROM workers WHERE tenant_id = {0} AND tenant_supplier_id IN (" + inClause + ") AND is_deleted = false GROUP BY status";
        var workerCounts = await _db.Database
            .SqlQueryRaw<StatusCount>(workerSql, paramsWithTenant)
            .ToListAsync(ct);

        var totalWorkers = workerCounts.Sum(x => x.Count);
        var activeWorkers = workerCounts.Where(x => x.Status == "Active" || x.Status == "OnContract").Sum(x => x.Count);
        var deployedWorkers = workerCounts.Where(x => x.Status == "OnContract" || x.Status == "Deployed").Sum(x => x.Count);

        // Commission totals via raw SQL
        var commissionTotals = await _db.Database
            .SqlQueryRaw<CommissionSummary>(
                "SELECT COALESCE(SUM(amount), 0)::numeric AS \"TotalAmount\", COALESCE(SUM(CASE WHEN status = 'Paid' THEN amount ELSE 0 END), 0)::numeric AS \"PaidAmount\", COALESCE(SUM(CASE WHEN status = 'Pending' THEN amount ELSE 0 END), 0)::numeric AS \"PendingAmount\" FROM supplier_payments WHERE tenant_id = {0} AND supplier_id = {1} AND is_deleted = false",
                tenantId, supplierId)
            .FirstOrDefaultAsync(ct);

        return Result<SupplierDashboardDto>.Success(new SupplierDashboardDto
        {
            TotalCandidates = totalCandidates,
            PendingCandidates = pendingCandidates,
            ApprovedCandidates = approvedCandidates,
            RejectedCandidates = rejectedCandidates,
            TotalWorkers = totalWorkers,
            ActiveWorkers = activeWorkers,
            DeployedWorkers = deployedWorkers,
            TotalCommissions = commissionTotals?.TotalAmount ?? 0,
            PendingCommissions = commissionTotals?.PendingAmount ?? 0,
            PaidCommissions = commissionTotals?.PaidAmount ?? 0,
        });
    }

    #endregion

    #region Candidates

    public async Task<PagedList<SupplierCandidateListDto>> ListCandidatesAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default)
    {
        var tenantSupplierIds = await GetTenantSupplierIdsAsync(tenantId, supplierId, ct);

        if (tenantSupplierIds.Count == 0)
            return PagedList<SupplierCandidateListDto>.Empty(qp.Page, qp.PageSize);

        var (inClause, inParams) = BuildInClause(tenantSupplierIds, startIndex: 1);
        var baseParams = new List<object> { tenantId };
        baseParams.AddRange(inParams);

        var whereClauses = "tenant_id = {0} AND tenant_supplier_id IN (" + inClause + ") AND is_deleted = false";

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchIdx = baseParams.Count;
            baseParams.Add("%" + qp.Search + "%");
            whereClauses += " AND (full_name_en ILIKE {" + searchIdx + "} OR full_name_ar ILIKE {" + searchIdx + "} OR passport_number ILIKE {" + searchIdx + "})";
        }

        var statusFilterField = qp.Filters.FirstOrDefault(f => f.Name == "status");
        if (statusFilterField is not null && statusFilterField.Values.Count > 0)
        {
            var statusClauses = new List<string>();
            foreach (var v in statusFilterField.Values)
            {
                var idx = baseParams.Count;
                baseParams.Add(v);
                statusClauses.Add("{" + idx + "}");
            }
            whereClauses += " AND status IN (" + string.Join(",", statusClauses) + ")";
        }

        var paramsArray = baseParams.ToArray();

        var countSql = "SELECT COUNT(*)::int AS \"Value\" FROM candidates WHERE " + whereClauses;
        var totalCount = await _db.Database.SqlQueryRaw<int>(countSql, paramsArray).FirstOrDefaultAsync(ct);

        var offsetIdx = baseParams.Count;
        baseParams.Add(qp.Skip);
        var limitIdx = baseParams.Count;
        baseParams.Add(qp.PageSize);
        paramsArray = baseParams.ToArray();

        var dataSql = "SELECT id AS \"Id\", full_name_en AS \"FullNameEn\", full_name_ar AS \"FullNameAr\", nationality AS \"Nationality\", status AS \"Status\", photo_url AS \"PhotoUrl\", passport_number AS \"PassportNumber\", created_at AS \"CreatedAt\" FROM candidates WHERE " + whereClauses + " ORDER BY created_at DESC OFFSET {" + offsetIdx + "} LIMIT {" + limitIdx + "}";

        var items = await _db.Database.SqlQueryRaw<SupplierCandidateListDto>(dataSql, paramsArray).ToListAsync(ct);

        return new PagedList<SupplierCandidateListDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    #endregion

    #region Workers

    public async Task<PagedList<SupplierWorkerListDto>> ListWorkersAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default)
    {
        var tenantSupplierIds = await GetTenantSupplierIdsAsync(tenantId, supplierId, ct);

        if (tenantSupplierIds.Count == 0)
            return PagedList<SupplierWorkerListDto>.Empty(qp.Page, qp.PageSize);

        var (inClause, inParams) = BuildInClause(tenantSupplierIds, startIndex: 1);
        var baseParams = new List<object> { tenantId };
        baseParams.AddRange(inParams);

        var whereClauses = "tenant_id = {0} AND tenant_supplier_id IN (" + inClause + ") AND is_deleted = false";

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchIdx = baseParams.Count;
            baseParams.Add("%" + qp.Search + "%");
            whereClauses += " AND (full_name_en ILIKE {" + searchIdx + "} OR full_name_ar ILIKE {" + searchIdx + "} OR worker_code ILIKE {" + searchIdx + "})";
        }

        var statusFilterField = qp.Filters.FirstOrDefault(f => f.Name == "status");
        if (statusFilterField is not null && statusFilterField.Values.Count > 0)
        {
            var statusClauses = new List<string>();
            foreach (var v in statusFilterField.Values)
            {
                var idx = baseParams.Count;
                baseParams.Add(v);
                statusClauses.Add("{" + idx + "}");
            }
            whereClauses += " AND status IN (" + string.Join(",", statusClauses) + ")";
        }

        var paramsArray = baseParams.ToArray();

        var countSql = "SELECT COUNT(*)::int AS \"Value\" FROM workers WHERE " + whereClauses;
        var totalCount = await _db.Database.SqlQueryRaw<int>(countSql, paramsArray).FirstOrDefaultAsync(ct);

        var offsetIdx = baseParams.Count;
        baseParams.Add(qp.Skip);
        var limitIdx = baseParams.Count;
        baseParams.Add(qp.PageSize);
        paramsArray = baseParams.ToArray();

        var dataSql = "SELECT id AS \"Id\", worker_code AS \"WorkerCode\", full_name_en AS \"FullNameEn\", full_name_ar AS \"FullNameAr\", nationality AS \"Nationality\", status AS \"Status\", photo_url AS \"PhotoUrl\", created_at AS \"CreatedAt\" FROM workers WHERE " + whereClauses + " ORDER BY created_at DESC OFFSET {" + offsetIdx + "} LIMIT {" + limitIdx + "}";

        var items = await _db.Database.SqlQueryRaw<SupplierWorkerListDto>(dataSql, paramsArray).ToListAsync(ct);

        return new PagedList<SupplierWorkerListDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    #endregion

    #region Commissions

    public async Task<PagedList<SupplierCommissionDto>> ListCommissionsAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default)
    {
        var baseParams = new List<object> { tenantId, supplierId };
        var whereClauses = "sp.tenant_id = {0} AND sp.supplier_id = {1} AND sp.is_deleted = false";

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchIdx = baseParams.Count;
            baseParams.Add("%" + qp.Search + "%");
            whereClauses += " AND (sp.payment_number ILIKE {" + searchIdx + "} OR sp.reference_number ILIKE {" + searchIdx + "})";
        }

        var statusFilterField = qp.Filters.FirstOrDefault(f => f.Name == "status");
        if (statusFilterField is not null && statusFilterField.Values.Count > 0)
        {
            var statusClauses = new List<string>();
            foreach (var v in statusFilterField.Values)
            {
                var idx = baseParams.Count;
                baseParams.Add(v);
                statusClauses.Add("{" + idx + "}");
            }
            whereClauses += " AND sp.status IN (" + string.Join(",", statusClauses) + ")";
        }

        var paramsArray = baseParams.ToArray();

        var countSql = "SELECT COUNT(*)::int AS \"Value\" FROM supplier_payments sp WHERE " + whereClauses;
        var totalCount = await _db.Database.SqlQueryRaw<int>(countSql, paramsArray).FirstOrDefaultAsync(ct);

        var offsetIdx = baseParams.Count;
        baseParams.Add(qp.Skip);
        var limitIdx = baseParams.Count;
        baseParams.Add(qp.PageSize);
        paramsArray = baseParams.ToArray();

        var dataSql = "SELECT sp.id AS \"Id\", sp.payment_number AS \"ReferenceNumber\", sp.amount AS \"Amount\", sp.currency AS \"Currency\", sp.status AS \"Status\", w.full_name_en AS \"WorkerNameEn\", sp.notes AS \"Notes\", sp.paid_at AS \"PaymentDate\", sp.created_at AS \"CreatedAt\" FROM supplier_payments sp LEFT JOIN workers w ON w.id = sp.worker_id WHERE " + whereClauses + " ORDER BY sp.created_at DESC OFFSET {" + offsetIdx + "} LIMIT {" + limitIdx + "}";

        var items = await _db.Database.SqlQueryRaw<SupplierCommissionDto>(dataSql, paramsArray).ToListAsync(ct);

        return new PagedList<SupplierCommissionDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    #endregion

    #region Arrivals

    public async Task<PagedList<SupplierArrivalListDto>> ListArrivalsAsync(Guid tenantId, Guid supplierId, QueryParameters qp, CancellationToken ct = default)
    {
        var tenantSupplierIds = await GetTenantSupplierIdsAsync(tenantId, supplierId, ct);

        if (tenantSupplierIds.Count == 0)
            return PagedList<SupplierArrivalListDto>.Empty(qp.Page, qp.PageSize);

        var (inClause, inParams) = BuildInClause(tenantSupplierIds, startIndex: 1);
        var baseParams = new List<object> { tenantId };
        baseParams.AddRange(inParams);

        var whereClauses = "a.tenant_id = {0} AND w.tenant_supplier_id IN (" + inClause + ") AND a.is_deleted = false";

        var paramsArray = baseParams.ToArray();

        var countSql = "SELECT COUNT(*)::int AS \"Value\" FROM arrivals a INNER JOIN workers w ON w.id = a.worker_id WHERE " + whereClauses;
        var totalCount = await _db.Database.SqlQueryRaw<int>(countSql, paramsArray).FirstOrDefaultAsync(ct);

        var offsetIdx = baseParams.Count;
        baseParams.Add(qp.Skip);
        var limitIdx = baseParams.Count;
        baseParams.Add(qp.PageSize);
        paramsArray = baseParams.ToArray();

        var dataSql = "SELECT a.id AS \"Id\", w.full_name_en AS \"WorkerNameEn\", a.flight_number AS \"FlightNumber\", a.arrival_date AS \"ArrivalDate\", a.status AS \"Status\", a.airport_code AS \"AirportCode\", (a.pre_travel_photo_url IS NOT NULL) AS \"HasPreTravelPhoto\", a.created_at AS \"CreatedAt\" FROM arrivals a INNER JOIN workers w ON w.id = a.worker_id WHERE " + whereClauses + " ORDER BY a.created_at DESC OFFSET {" + offsetIdx + "} LIMIT {" + limitIdx + "}";

        var items = await _db.Database.SqlQueryRaw<SupplierArrivalListDto>(dataSql, paramsArray).ToListAsync(ct);

        return new PagedList<SupplierArrivalListDto>(items, totalCount, qp.Page, qp.PageSize);
    }

    #endregion

    #region Helpers

    private async Task<List<Guid>> GetTenantSupplierIdsAsync(Guid tenantId, Guid supplierId, CancellationToken ct)
    {
        return await _db.Database
            .SqlQueryRaw<Guid>(
                "SELECT id AS \"Value\" FROM tenant_suppliers WHERE tenant_id = {0} AND supplier_id = {1}",
                tenantId, supplierId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Builds a parameterized IN clause for SqlQueryRaw.
    /// Returns ("{startIndex},{startIndex+1},...") and the corresponding parameter objects.
    /// </summary>
    private static (string InClause, object[] Params) BuildInClause(List<Guid> ids, int startIndex)
    {
        var placeholders = new List<string>();
        var parameters = new object[ids.Count];
        for (var i = 0; i < ids.Count; i++)
        {
            placeholders.Add("{" + (startIndex + i) + "}");
            parameters[i] = ids[i];
        }
        return (string.Join(",", placeholders), parameters);
    }

    private async Task<SupplierUserDto> MapToDto(SupplierUser entity, CancellationToken ct)
    {
        var supplierNames = await _db.Database
            .SqlQueryRaw<SupplierNameRow>(
                "SELECT name_en AS \"NameEn\", name_ar AS \"NameAr\" FROM suppliers WHERE id = {0}",
                entity.SupplierId)
            .FirstOrDefaultAsync(ct);

        return new SupplierUserDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            SupplierId = entity.SupplierId,
            IsActive = entity.IsActive,
            DisplayName = entity.DisplayName,
            Email = entity.Email,
            Phone = entity.Phone,
            SupplierNameEn = supplierNames?.NameEn,
            SupplierNameAr = supplierNames?.NameAr,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private static SupplierUserDto MapToDto(SupplierUser entity, Dictionary<Guid, (string? NameEn, string? NameAr)> supplierNameMap)
    {
        supplierNameMap.TryGetValue(entity.SupplierId, out var names);

        return new SupplierUserDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            SupplierId = entity.SupplierId,
            IsActive = entity.IsActive,
            DisplayName = entity.DisplayName,
            Email = entity.Email,
            Phone = entity.Phone,
            SupplierNameEn = names.NameEn,
            SupplierNameAr = names.NameAr,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    #endregion
}

// Internal helper types for raw SQL projections
internal sealed class StatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

internal sealed class CommissionSummary
{
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}

internal sealed class SupplierNameRow
{
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
}

internal sealed class SupplierNameWithIdRow
{
    public Guid Id { get; set; }
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
}
