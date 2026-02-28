using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using System.Linq.Expressions;

namespace Financial.Core.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<FinancialReportService> _logger;

    private static readonly Dictionary<string, Expression<Func<CashReconciliation, object>>> SortableFields = new()
    {
        ["reportDate"] = x => x.ReportDate,
        ["grandTotal"] = x => x.GrandTotal,
        ["createdAt"] = x => x.CreatedAt,
    };

    public FinancialReportService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<FinancialReportService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<MarginReportDto>> GetMarginReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        // Revenue from paid/partially paid invoices
        var invoices = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted &&
                        (x.Status == InvoiceStatus.Paid || x.Status == InvoiceStatus.PartiallyPaid))
            .ToListAsync(ct);

        // Costs from supplier payments
        var supplierPayments = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted &&
                        x.Status == SupplierPaymentStatus.Paid)
            .ToListAsync(ct);

        var totalRevenue = invoices.Sum(x => x.PaidAmount);
        var totalCost = supplierPayments.Sum(x => x.Amount);
        var grossMargin = totalRevenue - totalCost;
        var marginPercentage = totalRevenue > 0 ? Math.Round(grossMargin / totalRevenue * 100, 2) : 0;

        // Per-contract breakdown
        var contractIds = invoices.Select(x => x.ContractId)
            .Union(supplierPayments.Where(x => x.ContractId.HasValue).Select(x => x.ContractId!.Value))
            .Distinct();

        var lines = contractIds.Select(contractId =>
        {
            var revenue = invoices.Where(x => x.ContractId == contractId).Sum(x => x.PaidAmount);
            var cost = supplierPayments.Where(x => x.ContractId == contractId).Sum(x => x.Amount);
            var margin = revenue - cost;

            return new MarginLineDto
            {
                ContractId = contractId,
                WorkerId = invoices.FirstOrDefault(x => x.ContractId == contractId)?.WorkerId,
                ClientId = invoices.FirstOrDefault(x => x.ContractId == contractId)?.ClientId,
                Revenue = revenue,
                Cost = cost,
                Margin = margin,
                MarginPercentage = revenue > 0 ? Math.Round(margin / revenue * 100, 2) : 0,
            };
        }).ToList();

        return Result<MarginReportDto>.Success(new MarginReportDto
        {
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            GrossMargin = grossMargin,
            MarginPercentage = marginPercentage,
            Lines = lines,
        });
    }

    public async Task<Result<CashReconciliationDto>> GenerateXReportAsync(Guid tenantId, DateOnly? reportDate = null, CancellationToken ct = default)
    {
        var date = reportDate ?? _clock.Today;

        // Check if report already exists for this date
        var existing = await _db.Set<CashReconciliation>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.ReportDate == date, ct);

        if (existing is not null)
            return Result<CashReconciliationDto>.Conflict($"X-Report for {date} already exists");

        // Aggregate completed payments for the date
        var payments = await _db.Set<Payment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted &&
                        x.Status == PaymentStatus.Completed &&
                        x.PaymentDate == date)
            .ToListAsync(ct);

        var report = new CashReconciliation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReportDate = date,
            CashTotal = payments.Where(x => x.Method == PaymentMethod.Cash).Sum(x => x.Amount),
            CardTotal = payments.Where(x => x.Method == PaymentMethod.Card).Sum(x => x.Amount),
            BankTransferTotal = payments.Where(x => x.Method == PaymentMethod.BankTransfer).Sum(x => x.Amount),
            ChequeTotal = payments.Where(x => x.Method == PaymentMethod.Cheque).Sum(x => x.Amount),
            EDirhamTotal = payments.Where(x => x.Method == PaymentMethod.EDirham).Sum(x => x.Amount),
            OnlineTotal = payments.Where(x => x.Method == PaymentMethod.Online).Sum(x => x.Amount),
            TransactionCount = payments.Count,
            CreatedBy = _currentUser.UserId,
        };

        report.GrandTotal = report.CashTotal + report.CardTotal + report.BankTransferTotal +
                            report.ChequeTotal + report.EDirhamTotal + report.OnlineTotal;

        _db.Set<CashReconciliation>().Add(report);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Generated X-Report for {Date} with {Count} transactions totaling {Total}",
            date, report.TransactionCount, report.GrandTotal);

        return Result<CashReconciliationDto>.Success(MapToDto(report));
    }

    public async Task<Result<CashReconciliationDto>> CloseXReportAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var report = await _db.Set<CashReconciliation>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (report is null)
            return Result<CashReconciliationDto>.NotFound($"X-Report with ID {id} not found");

        if (report.IsClosed)
            return Result<CashReconciliationDto>.ValidationError("X-Report is already closed");

        report.IsClosed = true;
        report.ClosedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Closed X-Report {ReportId} for {Date}", id, report.ReportDate);

        return Result<CashReconciliationDto>.Success(MapToDto(report));
    }

    public async Task<PagedList<CashReconciliationListDto>> ListXReportsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<CashReconciliation>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => new CashReconciliationListDto
            {
                Id = x.Id,
                ReportDate = x.ReportDate,
                CashierName = x.CashierName,
                GrandTotal = x.GrandTotal,
                TransactionCount = x.TransactionCount,
                IsClosed = x.IsClosed,
                CreatedAt = x.CreatedAt,
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<RevenueBreakdownDto>> GetRevenueBreakdownAsync(Guid tenantId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var query = _db.Set<Payment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted &&
                        x.Status == PaymentStatus.Completed);

        if (from.HasValue)
            query = query.Where(x => x.PaymentDate >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.PaymentDate <= to.Value);

        var payments = await query.ToListAsync(ct);

        var totalRevenue = payments.Sum(x => x.Amount);

        // By month
        var byPeriod = payments
            .GroupBy(x => x.PaymentDate.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // By payment method
        var byMethod = payments
            .GroupBy(x => x.Method.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        return Result<RevenueBreakdownDto>.Success(new RevenueBreakdownDto
        {
            TotalRevenue = totalRevenue,
            ByPeriod = byPeriod,
            ByPaymentMethod = byMethod,
        });
    }

    #region Mapping

    private static CashReconciliationDto MapToDto(CashReconciliation r)
    {
        return new CashReconciliationDto
        {
            Id = r.Id,
            TenantId = r.TenantId,
            ReportDate = r.ReportDate,
            CashierId = r.CashierId,
            CashierName = r.CashierName,
            CashTotal = r.CashTotal,
            CardTotal = r.CardTotal,
            BankTransferTotal = r.BankTransferTotal,
            ChequeTotal = r.ChequeTotal,
            EDirhamTotal = r.EDirhamTotal,
            OnlineTotal = r.OnlineTotal,
            GrandTotal = r.GrandTotal,
            TransactionCount = r.TransactionCount,
            Notes = r.Notes,
            IsClosed = r.IsClosed,
            ClosedAt = r.ClosedAt,
            CreatedAt = r.CreatedAt,
        };
    }

    #endregion
}
