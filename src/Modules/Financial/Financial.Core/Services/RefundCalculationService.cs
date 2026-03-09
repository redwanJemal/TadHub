using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Financial.Core.Services;

public class RefundCalculationService : IRefundCalculationService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<RefundCalculationService> _logger;

    public RefundCalculationService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<RefundCalculationService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<RefundCalculationDto>> CalculateRefundAsync(
        Guid tenantId,
        Guid contractId,
        DateOnly returnDate,
        int contractMonths,
        string partialMonthMethod,
        CancellationToken ct = default)
    {
        // Get contract start date and total paid via raw SQL (cross-module)
        var contractInfo = await _db.Database
            .SqlQueryRaw<ContractInfoRaw>(
                "SELECT start_date AS \"StartDate\", total_amount AS \"TotalAmount\" FROM contracts WHERE id = {0} AND tenant_id = {1} AND is_deleted = false LIMIT 1",
                contractId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (contractInfo is null)
            return Result<RefundCalculationDto>.NotFound($"Contract with ID {contractId} not found");

        // Calculate total paid from invoices
        var totalPaid = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ContractId == contractId && !x.IsDeleted)
            .Where(x => x.Status == InvoiceStatus.Paid || x.Status == InvoiceStatus.PartiallyPaid)
            .SumAsync(x => x.PaidAmount, ct);

        if (totalPaid <= 0)
            return Result<RefundCalculationDto>.ValidationError("No payments found for this contract");

        var startDate = contractInfo.StartDate;

        // Calculate months worked
        var totalDays = returnDate.DayNumber - startDate.DayNumber;
        if (totalDays < 0)
            return Result<RefundCalculationDto>.ValidationError("Return date cannot be before contract start date");

        decimal monthsWorked;
        if (partialMonthMethod == "RoundDown")
        {
            // Count full months only
            var fullMonths = 0;
            var cursor = startDate;
            while (cursor.AddMonths(1) <= returnDate)
            {
                fullMonths++;
                cursor = startDate.AddMonths(fullMonths);
            }
            monthsWorked = fullMonths;
        }
        else // ProRata
        {
            // Calculate months with partial month pro-rated
            var fullMonths = 0;
            var cursor = startDate;
            while (cursor.AddMonths(1) <= returnDate)
            {
                fullMonths++;
                cursor = startDate.AddMonths(fullMonths);
            }
            // Remaining days as fraction of month
            var nextMonth = startDate.AddMonths(fullMonths + 1);
            var daysInPartialMonth = nextMonth.DayNumber - cursor.DayNumber;
            var remainingDays = returnDate.DayNumber - cursor.DayNumber;
            var partialFraction = daysInPartialMonth > 0 ? (decimal)remainingDays / daysInPartialMonth : 0;
            monthsWorked = fullMonths + partialFraction;
        }

        var valuePerMonth = totalPaid / contractMonths;
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        // Round to 2 decimal places
        refundAmount = Math.Round(refundAmount, 2);
        valuePerMonth = Math.Round(valuePerMonth, 2);
        monthsWorked = Math.Round(monthsWorked, 2);

        _logger.LogInformation(
            "Calculated refund for contract {ContractId}: TotalPaid={TotalPaid}, MonthsWorked={MonthsWorked}, RefundAmount={RefundAmount}",
            contractId, totalPaid, monthsWorked, refundAmount);

        return Result<RefundCalculationDto>.Success(new RefundCalculationDto
        {
            ContractId = contractId,
            TotalPaid = totalPaid,
            ContractMonths = contractMonths,
            MonthsWorked = monthsWorked,
            ValuePerMonth = valuePerMonth,
            RefundAmount = refundAmount,
            PartialMonthMethod = partialMonthMethod,
            ContractStartDate = startDate,
            ReturnDate = returnDate,
        });
    }

    public async Task<Result<CommissionCalculationDto>> CalculateCommissionAsync(
        Guid tenantId,
        Guid placementId,
        string calculationType,
        decimal fixedAmount,
        decimal percentage,
        CancellationToken ct = default)
    {
        // Get placement info via raw SQL
        var placementInfo = await _db.Database
            .SqlQueryRaw<PlacementInfoRaw>(
                "SELECT p.id AS \"PlacementId\", p.candidate_id AS \"CandidateId\", p.client_id AS \"ClientId\", c.supplier_id AS \"SupplierId\", ct.total_amount AS \"ContractValue\" " +
                "FROM placements p " +
                "LEFT JOIN candidates c ON c.id = p.candidate_id " +
                "LEFT JOIN contracts ct ON ct.placement_id = p.id AND ct.tenant_id = {1} AND ct.is_deleted = false " +
                "WHERE p.id = {0} AND p.tenant_id = {1} AND p.is_deleted = false LIMIT 1",
                placementId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (placementInfo is null)
            return Result<CommissionCalculationDto>.NotFound($"Placement with ID {placementId} not found");

        if (placementInfo.SupplierId == Guid.Empty || placementInfo.SupplierId == null)
            return Result<CommissionCalculationDto>.ValidationError("No supplier associated with this placement");

        decimal commissionAmount;
        switch (calculationType)
        {
            case "Fixed":
                commissionAmount = fixedAmount;
                break;
            case "Percentage":
                var contractValue = placementInfo.ContractValue ?? 0;
                commissionAmount = Math.Round(contractValue * percentage / 100m, 2);
                break;
            default: // Custom
                commissionAmount = fixedAmount;
                break;
        }

        if (commissionAmount <= 0)
            return Result<CommissionCalculationDto>.ValidationError("Commission amount must be greater than zero");

        // Create the supplier payment record
        var lastNumber = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PaymentNumber)
            .Select(x => x.PaymentNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastNumber is not null)
        {
            var dashIndex = lastNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastNumber[(dashIndex + 1)..], out var parsed))
                nextNumber = parsed + 1;
        }

        var payment = new SupplierPayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentNumber = $"SPAY-{nextNumber:D6}",
            Status = SupplierPaymentStatus.Pending,
            PaymentType = SupplierPaymentType.Commission,
            SupplierId = placementInfo.SupplierId.Value,
            PlacementId = placementId,
            Amount = commissionAmount,
            Currency = "AED",
            Method = PaymentMethod.BankTransfer,
            PaymentDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime),
            Notes = $"Auto-calculated commission ({calculationType}) for placement",
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<SupplierPayment>().Add(payment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created commission payment {PaymentNumber} for supplier {SupplierId}, amount={Amount}",
            payment.PaymentNumber, placementInfo.SupplierId, commissionAmount);

        return Result<CommissionCalculationDto>.Success(new CommissionCalculationDto
        {
            PlacementId = placementId,
            SupplierId = placementInfo.SupplierId.Value,
            CalculationType = calculationType,
            CommissionAmount = commissionAmount,
            ContractValue = placementInfo.ContractValue,
            Percentage = calculationType == "Percentage" ? percentage : null,
            SupplierPaymentId = payment.Id,
        });
    }

    // Raw SQL projection types
    private sealed class ContractInfoRaw
    {
        public DateOnly StartDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    private sealed class PlacementInfoRaw
    {
        public Guid PlacementId { get; set; }
        public Guid CandidateId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? SupplierId { get; set; }
        public decimal? ContractValue { get; set; }
    }
}
