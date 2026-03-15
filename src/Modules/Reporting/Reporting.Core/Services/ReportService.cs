using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reporting.Contracts;
using Reporting.Contracts.DTOs;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Reporting.Core.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReportService> _logger;

    public ReportService(AppDbContext db, ILogger<ReportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Workforce Reports ──

    public async Task<PagedList<InventoryReportItemDto>> GetInventoryReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT w.id AS id, w.worker_code AS worker_code,
                   w.full_name_en AS full_name_en, w.full_name_ar AS full_name_ar,
                   w.nationality AS nationality, w.location::text AS location,
                   w.status::text AS status, w.gender AS gender,
                   w.date_of_birth AS date_of_birth, w.experience_years AS experience_years,
                   w.monthly_salary AS monthly_salary, w.tenant_supplier_id AS tenant_supplier_id,
                   s.name_en AS supplier_name_en, s.name_ar AS supplier_name_ar,
                   w.created_at AS created_at
            FROM workers w
            LEFT JOIN tenant_suppliers ts ON w.tenant_supplier_id = ts.id
            LEFT JOIN suppliers s ON ts.supplier_id = s.id
            WHERE w.tenant_id = {0} AND w.is_deleted = false
              AND w.status IN ('Available', 'NewArrival', 'InTraining', 'UnderMedicalTest')
            """);

        AppendFilter(sql, parameters, qp, "nationality", "w.nationality");
        AppendFilter(sql, parameters, qp, "location", "w.location::text");
        AppendFilter(sql, parameters, qp, "status", "w.status::text");
        AppendFilter(sql, parameters, qp, "supplierId", "w.tenant_supplier_id::text");
        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "w.full_name_ar");

        return await ExecutePagedRawAsync<InventoryReportItemDto>(
            sql, parameters, "w.created_at DESC", qp, ct);
    }

    public async Task<PagedList<DeployedReportItemDto>> GetDeployedReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT w.id AS worker_id, w.worker_code AS worker_code,
                   w.full_name_en AS full_name_en, w.full_name_ar AS full_name_ar,
                   w.nationality AS nationality,
                   c.id AS contract_id, c.contract_code AS contract_code,
                   c.client_id AS client_id,
                   cl.name_en AS client_name_en, cl.name_ar AS client_name_ar,
                   c.start_date AS start_date, c.end_date AS end_date,
                   c.type::text AS contract_type,
                   c.rate AS rate, c.rate_period::text AS rate_period
            FROM workers w
            INNER JOIN contracts c ON c.worker_id = w.id
              AND c.tenant_id = w.tenant_id AND c.deleted_at IS NULL
              AND c.status IN ('Active', 'OnProbation')
            LEFT JOIN clients cl ON c.client_id = cl.id
            WHERE w.tenant_id = {0} AND w.is_deleted = false
            """);

        AppendFilter(sql, parameters, qp, "nationality", "w.nationality");
        AppendFilter(sql, parameters, qp, "contractType", "c.type::text");
        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "cl.name_en");

        return await ExecutePagedRawAsync<DeployedReportItemDto>(
            sql, parameters, "c.start_date DESC", qp, ct);
    }

    public async Task<PagedList<ReturneeReportItemDto>> GetReturneeReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT rc.id AS id, rc.case_code AS case_code,
                   rc.status::text AS status, rc.return_type::text AS return_type,
                   rc.return_date AS return_date, rc.return_reason AS return_reason,
                   rc.worker_id AS worker_id,
                   w.full_name_en AS worker_name_en, w.full_name_ar AS worker_name_ar,
                   rc.client_id AS client_id,
                   cl.name_en AS client_name_en, cl.name_ar AS client_name_ar,
                   rc.total_amount_paid AS total_amount_paid, rc.refund_amount AS refund_amount,
                   rc.is_within_guarantee AS is_within_guarantee,
                   rc.guarantee_period_type::text AS guarantee_period_type,
                   rc.settled_at AS settled_at, rc.created_at AS created_at
            FROM returnee_cases rc
            LEFT JOIN workers w ON rc.worker_id = w.id AND w.is_deleted = false
            LEFT JOIN clients cl ON rc.client_id = cl.id
            WHERE rc.tenant_id = {0} AND rc.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "rc.status::text");
        AppendFilter(sql, parameters, qp, "returnType", "rc.return_type::text");
        AppendDateRange(sql, parameters, qp, "rc.return_date");
        AppendSearch(sql, parameters, qp, "rc.case_code", "w.full_name_en", "cl.name_en");

        return await ExecutePagedRawAsync<ReturneeReportItemDto>(
            sql, parameters, "rc.created_at DESC", qp, ct);
    }

    public async Task<PagedList<RunawayReportItemDto>> GetRunawayReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT rc.id AS id, rc.case_code AS case_code,
                   rc.status::text AS status,
                   rc.reported_date AS reported_date,
                   rc.worker_id AS worker_id,
                   w.full_name_en AS worker_name_en, w.full_name_ar AS worker_name_ar,
                   rc.client_id AS client_id,
                   cl.name_en AS client_name_en, cl.name_ar AS client_name_ar,
                   rc.is_within_guarantee AS is_within_guarantee,
                   rc.guarantee_period_type::text AS guarantee_period_type,
                   rc.police_report_number AS police_report_number,
                   COALESCE((SELECT SUM(e.amount) FROM runaway_expenses e WHERE e.runaway_case_id = rc.id), 0) AS total_expenses,
                   rc.settled_at AS settled_at, rc.created_at AS created_at
            FROM runaway_cases rc
            LEFT JOIN workers w ON rc.worker_id = w.id AND w.is_deleted = false
            LEFT JOIN clients cl ON rc.client_id = cl.id
            WHERE rc.tenant_id = {0} AND rc.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "rc.status::text");
        AppendDateRange(sql, parameters, qp, "rc.reported_date::date");
        AppendSearch(sql, parameters, qp, "rc.case_code", "w.full_name_en", "cl.name_en");

        return await ExecutePagedRawAsync<RunawayReportItemDto>(
            sql, parameters, "rc.created_at DESC", qp, ct);
    }

    // ── Operational Reports ──

    public async Task<PagedList<ArrivalReportItemDto>> GetArrivalsReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT a.id AS id, a.arrival_code AS arrival_code,
                   a.status::text AS status,
                   a.worker_id AS worker_id,
                   w.full_name_en AS worker_name_en, w.full_name_ar AS worker_name_ar,
                   a.flight_number AS flight_number, a.airport_name AS airport_name,
                   a.scheduled_arrival_date AS scheduled_arrival_date,
                   a.scheduled_arrival_time AS scheduled_arrival_time,
                   a.actual_arrival_time AS actual_arrival_time,
                   a.driver_name AS driver_name,
                   a.driver_confirmed_pickup_at AS driver_confirmed_pickup_at,
                   a.created_at AS created_at
            FROM arrivals a
            LEFT JOIN workers w ON a.worker_id = w.id AND w.is_deleted = false
            WHERE a.tenant_id = {0} AND a.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "a.status::text");
        AppendDateRange(sql, parameters, qp, "a.scheduled_arrival_date");
        AppendSearch(sql, parameters, qp, "a.arrival_code", "w.full_name_en", "a.flight_number");

        return await ExecutePagedRawAsync<ArrivalReportItemDto>(
            sql, parameters, "a.scheduled_arrival_date DESC, a.scheduled_arrival_time DESC", qp, ct);
    }

    public async Task<PagedList<AccommodationDailyItemDto>> GetAccommodationDailyListAsync(
        Guid tenantId, DateOnly date, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId, date };

        sql.Append("""
            SELECT s.id AS id, s.stay_code AS stay_code,
                   s.status::text AS status,
                   s.worker_id AS worker_id,
                   w.full_name_en AS worker_name_en, w.full_name_ar AS worker_name_ar,
                   s.room AS room, s.location AS location_name,
                   s.check_in_date AS check_in_date, s.check_out_date AS check_out_date,
                   s.departure_reason::text AS departure_reason
            FROM accommodation_stays s
            LEFT JOIN workers w ON s.worker_id = w.id AND w.is_deleted = false
            WHERE s.tenant_id = {0} AND s.deleted_at IS NULL
              AND s.check_in_date <= {1}
              AND (s.check_out_date IS NULL OR s.check_out_date >= {1})
            """);

        AppendFilter(sql, parameters, qp, "location", "s.location");
        AppendFilter(sql, parameters, qp, "room", "s.room");
        AppendSearch(sql, parameters, qp, "s.stay_code", "w.full_name_en", "s.room");

        return await ExecutePagedRawAsync<AccommodationDailyItemDto>(
            sql, parameters, "s.location, s.room, w.full_name_en", qp, ct);
    }

    public async Task<List<DeploymentPipelineItemDto>> GetDeploymentPipelineAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var sql = """
            SELECT p.status::text AS stage, COUNT(*)::int AS count
            FROM placements p
            WHERE p.tenant_id = {0} AND p.deleted_at IS NULL
              AND p.status NOT IN ('Completed', 'Cancelled')
            GROUP BY p.status
            ORDER BY p.status
            """;

        return await _db.Database.SqlQueryRaw<DeploymentPipelineItemDto>(sql, tenantId)
            .ToListAsync(ct);
    }

    // ── Finance Reports (Extensions) ──

    public async Task<PagedList<SupplierCommissionItemDto>> GetSupplierCommissionReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT sp.supplier_id AS supplier_id,
                   s.name_en AS supplier_name_en, s.name_ar AS supplier_name_ar,
                   COUNT(*)::int AS payment_count,
                   COALESCE(SUM(CASE WHEN sp.status = 'Paid' THEN sp.amount ELSE 0 END), 0) AS total_paid,
                   COALESCE(SUM(CASE WHEN sp.status = 'Pending' THEN sp.amount ELSE 0 END), 0) AS total_pending
            FROM supplier_payments sp
            LEFT JOIN tenant_suppliers ts ON sp.supplier_id = ts.id
            LEFT JOIN suppliers s ON ts.supplier_id = s.id
            WHERE sp.tenant_id = {0} AND sp.deleted_at IS NULL
              AND sp.payment_type = 'Commission'
            """);

        AppendDateRange(sql, parameters, qp, "sp.payment_date");

        var supplierFilter = qp.Filters.FirstOrDefault(f => f.Name == "supplierId")?.Values.FirstOrDefault();
        if (!string.IsNullOrEmpty(supplierFilter) && Guid.TryParse(supplierFilter, out _))
        {
            parameters.Add(supplierFilter);
            sql.Append($" AND sp.supplier_id::text = {{{parameters.Count - 1}}}");
        }

        sql.Append(" GROUP BY sp.supplier_id, s.name_en, s.name_ar");

        return await ExecutePagedRawAsync<SupplierCommissionItemDto>(
            sql, parameters, @"""TotalPaid"" DESC", qp, ct);
    }

    public async Task<PagedList<RefundReportItemDto>> GetRefundReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT p.id AS payment_id, p.payment_number AS payment_number,
                   p.status::text AS status,
                   p.amount AS amount, p.refund_amount AS refund_amount,
                   p.method::text AS method,
                   p.payment_date AS payment_date,
                   p.client_id AS client_id,
                   cl.name_en AS client_name_en, cl.name_ar AS client_name_ar,
                   p.invoice_id AS invoice_id,
                   i.invoice_number AS invoice_number,
                   p.created_at AS created_at
            FROM payments p
            LEFT JOIN clients cl ON p.client_id = cl.id
            LEFT JOIN invoices i ON p.invoice_id = i.id AND i.deleted_at IS NULL
            WHERE p.tenant_id = {0} AND p.deleted_at IS NULL
              AND p.status = 'Refunded'
            """);

        AppendDateRange(sql, parameters, qp, "p.payment_date");
        AppendFilter(sql, parameters, qp, "method", "p.method::text");
        AppendSearch(sql, parameters, qp, "p.payment_number", "cl.name_en", "i.invoice_number");

        return await ExecutePagedRawAsync<RefundReportItemDto>(
            sql, parameters, "p.created_at DESC", qp, ct);
    }

    public async Task<PagedList<CostPerMaidItemDto>> GetCostPerMaidReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT p.worker_id AS worker_id,
                   w.worker_code AS worker_code,
                   w.full_name_en AS worker_name_en, w.full_name_ar AS worker_name_ar,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Procurement' THEN ci.amount ELSE 0 END), 0) AS procurement_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Flight' THEN ci.amount ELSE 0 END), 0) AS flight_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Medical' THEN ci.amount ELSE 0 END), 0) AS medical_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Visa' THEN ci.amount ELSE 0 END), 0) AS visa_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Insurance' THEN ci.amount ELSE 0 END), 0) AS insurance_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Accommodation' THEN ci.amount ELSE 0 END), 0) AS accommodation_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Training' THEN ci.amount ELSE 0 END), 0) AS training_cost,
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Other' THEN ci.amount ELSE 0 END), 0) AS other_cost,
                   COALESCE(SUM(ci.amount), 0) AS total_cost
            FROM placements p
            INNER JOIN placement_cost_items ci ON ci.placement_id = p.id
            LEFT JOIN workers w ON p.worker_id = w.id AND w.is_deleted = false
            WHERE p.tenant_id = {0} AND p.deleted_at IS NULL
              AND p.worker_id IS NOT NULL
            """);

        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "w.full_name_ar");

        sql.Append(" GROUP BY p.worker_id, w.worker_code, w.full_name_en, w.full_name_ar");

        return await ExecutePagedRawAsync<CostPerMaidItemDto>(
            sql, parameters, @"""TotalCost"" DESC", qp, ct);
    }

    // ── Pagination & SQL Helpers ──

    /// <summary>
    /// Allowed ORDER BY clauses. Only these exact strings may be used.
    /// This prevents SQL injection via the orderBy parameter.
    /// </summary>
    private static readonly HashSet<string> AllowedOrderByClauses = new(StringComparer.Ordinal)
    {
        "w.created_at DESC",
        "c.start_date DESC",
        "rc.created_at DESC",
        "a.scheduled_arrival_date DESC, a.scheduled_arrival_time DESC",
        "s.location, s.room, w.full_name_en",
        @"""TotalPaid"" DESC",
        @"""TotalCost"" DESC",
        "p.created_at DESC",
    };

    /// <summary>
    /// Executes a raw SQL query with manual pagination (COUNT + LIMIT/OFFSET).
    /// This avoids EF Core wrapping SqlQueryRaw in subqueries that break column aliases.
    /// </summary>
    private async Task<PagedList<T>> ExecutePagedRawAsync<T>(
        StringBuilder baseSql, List<object> parameters, string orderBy,
        QueryParameters qp, CancellationToken ct) where T : class
    {
        if (!AllowedOrderByClauses.Contains(orderBy))
            throw new ArgumentException($"Invalid ORDER BY clause: '{orderBy}'", nameof(orderBy));

        var page = Math.Max(1, qp.Page);
        var pageSize = Math.Clamp(qp.PageSize, 1, 100);
        var offset = (page - 1) * pageSize;

        // COUNT query — wrap the base SQL (no ORDER BY) in a subquery
        var countSql = $"SELECT COUNT(*)::int AS \"Value\" FROM ({baseSql}) AS _cnt";
        var totalCount = await _db.Database
            .SqlQueryRaw<int>(countSql, parameters.ToArray())
            .FirstOrDefaultAsync(ct);

        if (totalCount == 0)
            return PagedList<T>.Empty(page, pageSize);

        // Items query — add ORDER BY (validated) + parameterized LIMIT/OFFSET
        var limitIdx = parameters.Count;
        parameters.Add(pageSize);
        var offsetIdx = parameters.Count;
        parameters.Add(offset);

        baseSql.Append($" ORDER BY {orderBy} LIMIT {{{limitIdx}}} OFFSET {{{offsetIdx}}}");
        var items = await _db.Database
            .SqlQueryRaw<T>(baseSql.ToString(), parameters.ToArray())
            .ToListAsync(ct);

        return new PagedList<T>(items, totalCount, page, pageSize);
    }

    private static void AppendFilter(StringBuilder sql, List<object> parameters, QueryParameters qp, string filterName, string column)
    {
        var value = qp.Filters.FirstOrDefault(f => f.Name == filterName)?.Values.FirstOrDefault();
        if (string.IsNullOrEmpty(value)) return;

        parameters.Add(value);
        sql.Append($" AND {column} = {{{parameters.Count - 1}}}");
    }

    private static void AppendDateRange(StringBuilder sql, List<object> parameters, QueryParameters qp, string column)
    {
        var from = qp.Filters.FirstOrDefault(f => f.Name == "from")?.Values.FirstOrDefault();
        var to = qp.Filters.FirstOrDefault(f => f.Name == "to")?.Values.FirstOrDefault();

        if (!string.IsNullOrEmpty(from) && DateOnly.TryParse(from, out var fromDate))
        {
            parameters.Add(fromDate);
            sql.Append($" AND {column} >= {{{parameters.Count - 1}}}");
        }

        if (!string.IsNullOrEmpty(to) && DateOnly.TryParse(to, out var toDate))
        {
            parameters.Add(toDate);
            sql.Append($" AND {column} <= {{{parameters.Count - 1}}}");
        }
    }

    private static void AppendSearch(StringBuilder sql, List<object> parameters, QueryParameters qp, params string[] columns)
    {
        if (string.IsNullOrWhiteSpace(qp.Search)) return;

        parameters.Add($"%{qp.Search}%");
        var idx = parameters.Count - 1;
        var conditions = columns.Select(c => $"{c} ILIKE {{{idx}}}");
        sql.Append($" AND ({string.Join(" OR ", conditions)})");
    }
}
