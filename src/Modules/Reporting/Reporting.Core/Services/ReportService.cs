using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reporting.Contracts;
using Reporting.Contracts.DTOs;
using TadHub.Infrastructure.Api;
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
            SELECT w.id AS "Id", w.worker_code AS "WorkerCode",
                   w.full_name_en AS "FullNameEn", w.full_name_ar AS "FullNameAr",
                   w.nationality AS "Nationality", w.location::text AS "Location",
                   w.status::text AS "Status", w.gender AS "Gender",
                   w.date_of_birth AS "DateOfBirth", w.experience_years AS "ExperienceYears",
                   w.monthly_salary AS "MonthlySalary", w.tenant_supplier_id AS "TenantSupplierId",
                   s.name_en AS "SupplierNameEn", s.name_ar AS "SupplierNameAr",
                   w.created_at AS "CreatedAt"
            FROM workers w
            LEFT JOIN tenant_suppliers ts ON w.tenant_supplier_id = ts.id
            LEFT JOIN suppliers s ON ts.supplier_id = s.id
            WHERE w.tenant_id = {0} AND w.deleted_at IS NULL
              AND w.status IN ('Available', 'NewArrival', 'InTraining', 'UnderMedicalTest')
            """);

        AppendFilter(sql, parameters, qp, "nationality", "w.nationality");
        AppendFilter(sql, parameters, qp, "location", "w.location::text");
        AppendFilter(sql, parameters, qp, "status", "w.status::text");
        AppendFilter(sql, parameters, qp, "supplierId", "w.tenant_supplier_id::text");
        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "w.full_name_ar");

        sql.Append(" ORDER BY w.created_at DESC");

        var query = _db.Database.SqlQueryRaw<InventoryReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<DeployedReportItemDto>> GetDeployedReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT w.id AS "WorkerId", w.worker_code AS "WorkerCode",
                   w.full_name_en AS "FullNameEn", w.full_name_ar AS "FullNameAr",
                   w.nationality AS "Nationality",
                   c.id AS "ContractId", c.contract_code AS "ContractCode",
                   c.client_id AS "ClientId",
                   cl.name_en AS "ClientNameEn", cl.name_ar AS "ClientNameAr",
                   c.start_date AS "StartDate", c.end_date AS "EndDate",
                   c.type::text AS "ContractType",
                   c.rate AS "Rate", c.rate_period::text AS "RatePeriod"
            FROM workers w
            INNER JOIN contracts c ON c.worker_id = w.id
              AND c.tenant_id = w.tenant_id AND c.deleted_at IS NULL
              AND c.status IN ('Active', 'OnProbation')
            LEFT JOIN clients cl ON c.client_id = cl.id AND cl.deleted_at IS NULL
            WHERE w.tenant_id = {0} AND w.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "nationality", "w.nationality");
        AppendFilter(sql, parameters, qp, "contractType", "c.type::text");
        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "cl.name_en");

        sql.Append(" ORDER BY c.start_date DESC");

        var query = _db.Database.SqlQueryRaw<DeployedReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<ReturneeReportItemDto>> GetReturneeReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT rc.id AS "Id", rc.case_code AS "CaseCode",
                   rc.status::text AS "Status", rc.return_type::text AS "ReturnType",
                   rc.return_date AS "ReturnDate", rc.return_reason AS "ReturnReason",
                   rc.worker_id AS "WorkerId",
                   w.full_name_en AS "WorkerNameEn", w.full_name_ar AS "WorkerNameAr",
                   rc.client_id AS "ClientId",
                   cl.name_en AS "ClientNameEn", cl.name_ar AS "ClientNameAr",
                   rc.total_amount_paid AS "TotalAmountPaid", rc.refund_amount AS "RefundAmount",
                   rc.is_within_guarantee AS "IsWithinGuarantee",
                   rc.guarantee_period_type::text AS "GuaranteePeriodType",
                   rc.settled_at AS "SettledAt", rc.created_at AS "CreatedAt"
            FROM returnee_cases rc
            LEFT JOIN workers w ON rc.worker_id = w.id AND w.deleted_at IS NULL
            LEFT JOIN clients cl ON rc.client_id = cl.id AND cl.deleted_at IS NULL
            WHERE rc.tenant_id = {0} AND rc.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "rc.status::text");
        AppendFilter(sql, parameters, qp, "returnType", "rc.return_type::text");
        AppendDateRange(sql, parameters, qp, "rc.return_date");
        AppendSearch(sql, parameters, qp, "rc.case_code", "w.full_name_en", "cl.name_en");

        sql.Append(" ORDER BY rc.created_at DESC");

        var query = _db.Database.SqlQueryRaw<ReturneeReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<RunawayReportItemDto>> GetRunawayReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT rc.id AS "Id", rc.case_code AS "CaseCode",
                   rc.status::text AS "Status",
                   rc.reported_date AS "ReportedDate",
                   rc.worker_id AS "WorkerId",
                   w.full_name_en AS "WorkerNameEn", w.full_name_ar AS "WorkerNameAr",
                   rc.client_id AS "ClientId",
                   cl.name_en AS "ClientNameEn", cl.name_ar AS "ClientNameAr",
                   rc.is_within_guarantee AS "IsWithinGuarantee",
                   rc.guarantee_period_type::text AS "GuaranteePeriodType",
                   rc.police_report_number AS "PoliceReportNumber",
                   COALESCE((SELECT SUM(e.amount) FROM runaway_expenses e WHERE e.runaway_case_id = rc.id), 0) AS "TotalExpenses",
                   rc.settled_at AS "SettledAt", rc.created_at AS "CreatedAt"
            FROM runaway_cases rc
            LEFT JOIN workers w ON rc.worker_id = w.id AND w.deleted_at IS NULL
            LEFT JOIN clients cl ON rc.client_id = cl.id AND cl.deleted_at IS NULL
            WHERE rc.tenant_id = {0} AND rc.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "rc.status::text");
        AppendDateRange(sql, parameters, qp, "rc.reported_date::date");
        AppendSearch(sql, parameters, qp, "rc.case_code", "w.full_name_en", "cl.name_en");

        sql.Append(" ORDER BY rc.created_at DESC");

        var query = _db.Database.SqlQueryRaw<RunawayReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    // ── Operational Reports ──

    public async Task<PagedList<ArrivalReportItemDto>> GetArrivalsReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT a.id AS "Id", a.arrival_code AS "ArrivalCode",
                   a.status::text AS "Status",
                   a.worker_id AS "WorkerId",
                   w.full_name_en AS "WorkerNameEn", w.full_name_ar AS "WorkerNameAr",
                   a.flight_number AS "FlightNumber", a.airport_name AS "AirportName",
                   a.scheduled_arrival_date AS "ScheduledArrivalDate",
                   a.scheduled_arrival_time AS "ScheduledArrivalTime",
                   a.actual_arrival_time AS "ActualArrivalTime",
                   a.driver_name AS "DriverName",
                   a.driver_confirmed_pickup_at AS "DriverConfirmedPickupAt",
                   a.created_at AS "CreatedAt"
            FROM arrivals a
            LEFT JOIN workers w ON a.worker_id = w.id AND w.deleted_at IS NULL
            WHERE a.tenant_id = {0} AND a.deleted_at IS NULL
            """);

        AppendFilter(sql, parameters, qp, "status", "a.status::text");
        AppendDateRange(sql, parameters, qp, "a.scheduled_arrival_date");
        AppendSearch(sql, parameters, qp, "a.arrival_code", "w.full_name_en", "a.flight_number");

        sql.Append(" ORDER BY a.scheduled_arrival_date DESC, a.scheduled_arrival_time DESC");

        var query = _db.Database.SqlQueryRaw<ArrivalReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<AccommodationDailyItemDto>> GetAccommodationDailyListAsync(
        Guid tenantId, DateOnly date, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId, date };

        sql.Append("""
            SELECT s.id AS "Id", s.stay_code AS "StayCode",
                   s.status::text AS "Status",
                   s.worker_id AS "WorkerId",
                   w.full_name_en AS "WorkerNameEn", w.full_name_ar AS "WorkerNameAr",
                   s.room AS "Room", s.location AS "LocationName",
                   s.check_in_date AS "CheckInDate", s.check_out_date AS "CheckOutDate",
                   s.departure_reason::text AS "DepartureReason"
            FROM accommodation_stays s
            LEFT JOIN workers w ON s.worker_id = w.id AND w.deleted_at IS NULL
            WHERE s.tenant_id = {0} AND s.deleted_at IS NULL
              AND s.check_in_date <= {1}
              AND (s.check_out_date IS NULL OR s.check_out_date >= {1})
            """);

        AppendFilter(sql, parameters, qp, "location", "s.location");
        AppendFilter(sql, parameters, qp, "room", "s.room");
        AppendSearch(sql, parameters, qp, "s.stay_code", "w.full_name_en", "s.room");

        sql.Append(" ORDER BY s.location, s.room, w.full_name_en");

        var query = _db.Database.SqlQueryRaw<AccommodationDailyItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<List<DeploymentPipelineItemDto>> GetDeploymentPipelineAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var sql = """
            SELECT p.status::text AS "Stage", COUNT(*)::int AS "Count"
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
            SELECT sp.supplier_id AS "SupplierId",
                   s.name_en AS "SupplierNameEn", s.name_ar AS "SupplierNameAr",
                   COUNT(*)::int AS "PaymentCount",
                   COALESCE(SUM(CASE WHEN sp.status = 'Paid' THEN sp.amount ELSE 0 END), 0) AS "TotalPaid",
                   COALESCE(SUM(CASE WHEN sp.status = 'Pending' THEN sp.amount ELSE 0 END), 0) AS "TotalPending"
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
        sql.Append(" ORDER BY \"TotalPaid\" DESC");

        var query = _db.Database.SqlQueryRaw<SupplierCommissionItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<RefundReportItemDto>> GetRefundReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT p.id AS "PaymentId", p.payment_number AS "PaymentNumber",
                   p.status::text AS "Status",
                   p.amount AS "Amount", p.refund_amount AS "RefundAmount",
                   p.method::text AS "Method",
                   p.payment_date AS "PaymentDate",
                   p.client_id AS "ClientId",
                   cl.name_en AS "ClientNameEn", cl.name_ar AS "ClientNameAr",
                   p.invoice_id AS "InvoiceId",
                   i.invoice_number AS "InvoiceNumber",
                   p.created_at AS "CreatedAt"
            FROM payments p
            LEFT JOIN clients cl ON p.client_id = cl.id AND cl.deleted_at IS NULL
            LEFT JOIN invoices i ON p.invoice_id = i.id AND i.deleted_at IS NULL
            WHERE p.tenant_id = {0} AND p.deleted_at IS NULL
              AND p.status = 'Refunded'
            """);

        AppendDateRange(sql, parameters, qp, "p.payment_date");
        AppendFilter(sql, parameters, qp, "method", "p.method::text");
        AppendSearch(sql, parameters, qp, "p.payment_number", "cl.name_en", "i.invoice_number");

        sql.Append(" ORDER BY p.created_at DESC");

        var query = _db.Database.SqlQueryRaw<RefundReportItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<CostPerMaidItemDto>> GetCostPerMaidReportAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        var parameters = new List<object> { tenantId };

        sql.Append("""
            SELECT p.worker_id AS "WorkerId",
                   w.worker_code AS "WorkerCode",
                   w.full_name_en AS "WorkerNameEn", w.full_name_ar AS "WorkerNameAr",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Procurement' THEN ci.amount ELSE 0 END), 0) AS "ProcurementCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Flight' THEN ci.amount ELSE 0 END), 0) AS "FlightCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Medical' THEN ci.amount ELSE 0 END), 0) AS "MedicalCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Visa' THEN ci.amount ELSE 0 END), 0) AS "VisaCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Insurance' THEN ci.amount ELSE 0 END), 0) AS "InsuranceCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Accommodation' THEN ci.amount ELSE 0 END), 0) AS "AccommodationCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Training' THEN ci.amount ELSE 0 END), 0) AS "TrainingCost",
                   COALESCE(SUM(CASE WHEN ci.cost_type = 'Other' THEN ci.amount ELSE 0 END), 0) AS "OtherCost",
                   COALESCE(SUM(ci.amount), 0) AS "TotalCost"
            FROM placements p
            INNER JOIN placement_cost_items ci ON ci.placement_id = p.id
            LEFT JOIN workers w ON p.worker_id = w.id AND w.deleted_at IS NULL
            WHERE p.tenant_id = {0} AND p.deleted_at IS NULL
              AND p.worker_id IS NOT NULL
            """);

        AppendSearch(sql, parameters, qp, "w.worker_code", "w.full_name_en", "w.full_name_ar");

        sql.Append(" GROUP BY p.worker_id, w.worker_code, w.full_name_en, w.full_name_ar");
        sql.Append(" ORDER BY \"TotalCost\" DESC");

        var query = _db.Database.SqlQueryRaw<CostPerMaidItemDto>(sql.ToString(), parameters.ToArray());
        return await query.ToPagedListAsync(qp, ct);
    }

    // ── SQL Helpers ──

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
