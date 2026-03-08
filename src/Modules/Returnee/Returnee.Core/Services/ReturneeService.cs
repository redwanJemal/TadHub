using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Returnee.Contracts;
using Returnee.Contracts.DTOs;
using Returnee.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Returnee.Core.Services;

public class ReturneeService : IReturneeService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<ReturneeService> _logger;

    private const int ContractDurationMonths = 24;

    private static readonly Dictionary<string, Expression<Func<ReturneeCase, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["returnType"] = x => x.ReturnType,
        ["workerId"] = x => x.WorkerId,
        ["clientId"] = x => x.ClientId,
        ["contractId"] = x => x.ContractId,
    };

    private static readonly Dictionary<string, Expression<Func<ReturneeCase, object>>> SortableFields = new()
    {
        ["caseCode"] = x => x.CaseCode,
        ["status"] = x => x.Status,
        ["returnDate"] = x => x.ReturnDate,
        ["monthsWorked"] = x => x.MonthsWorked,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public ReturneeService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<ReturneeService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PagedList<ReturneeCaseListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.CaseCode.ToLower().Contains(searchLower) ||
                x.ReturnReason.ToLower().Contains(searchLower));
        }

        return await query
            .Select(x => new ReturneeCaseListDto
            {
                Id = x.Id,
                CaseCode = x.CaseCode,
                ReturnType = x.ReturnType.ToString(),
                Status = x.Status.ToString(),
                WorkerId = x.WorkerId,
                ContractId = x.ContractId,
                ClientId = x.ClientId,
                ReturnDate = x.ReturnDate,
                MonthsWorked = x.MonthsWorked,
                IsWithinGuarantee = x.IsWithinGuarantee,
                RefundAmount = x.RefundAmount,
                CreatedAt = x.CreatedAt,
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ReturneeCaseDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);
        var includeExpenses = includes.Contains("expenses", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));

        if (includeExpenses)
            query = query.Include(x => x.Expenses);

        var rc = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (rc is null)
            return Result<ReturneeCaseDto>.NotFound($"Returnee case with ID {id} not found");

        return Result<ReturneeCaseDto>.Success(MapToDto(rc, includeStatusHistory, includeExpenses));
    }

    public async Task<Result<ReturneeCaseDto>> CreateAsync(Guid tenantId, CreateReturneeCaseRequest request, CancellationToken ct = default)
    {
        // Validate return type
        if (!Enum.TryParse<ReturnType>(request.ReturnType, ignoreCase: true, out var returnType))
            return Result<ReturneeCaseDto>.ValidationError($"Invalid return type '{request.ReturnType}'. Must be 'ReturnToOffice' or 'ReturnToCountry'");

        // Validate worker exists via raw SQL
        var workerInfo = await _db.Database
            .SqlQueryRaw<WorkerInfoRaw>(
                "SELECT id AS \"Id\", status AS \"Status\", location AS \"Location\" FROM workers WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
                request.WorkerId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (workerInfo is null)
            return Result<ReturneeCaseDto>.NotFound($"Worker with ID {request.WorkerId} not found");

        // Validate contract exists via raw SQL
        var contractInfo = await _db.Database
            .SqlQueryRaw<ContractInfoRaw>(
                "SELECT id AS \"Id\", status AS \"Status\", start_date AS \"StartDate\", total_value AS \"TotalValue\", guarantee_end_date AS \"GuaranteeEndDate\" FROM contracts WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
                request.ContractId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (contractInfo is null)
            return Result<ReturneeCaseDto>.NotFound($"Contract with ID {request.ContractId} not found");

        var activeStatuses = new[] { "Active", "OnProbation", "Confirmed" };
        if (!activeStatuses.Contains(contractInfo.Status))
            return Result<ReturneeCaseDto>.ValidationError($"Contract must be in an active status. Current: '{contractInfo.Status}'");

        // Validate client exists via raw SQL
        var clientExists = await _db.Database
            .SqlQueryRaw<RawBool>(
                "SELECT EXISTS(SELECT 1 FROM clients WHERE id = {0} AND tenant_id = {1} AND is_deleted = false) AS \"Value\"",
                request.ClientId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (clientExists is null || !clientExists.Value)
            return Result<ReturneeCaseDto>.ValidationError("Client not found");

        // Check no existing active case for this contract
        var hasActiveCase = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.ContractId == request.ContractId
                && x.Status != ReturneeCaseStatus.Rejected, ct);

        if (hasActiveCase)
            return Result<ReturneeCaseDto>.Conflict("A returnee case already exists for this contract");

        // Calculate months worked
        var returnDate = request.ReturnDate;
        var contractStartDate = contractInfo.StartDate;
        var monthsWorked = CalculateMonthsWorked(contractStartDate, returnDate);

        // Determine guarantee status
        var isWithinGuarantee = false;
        GuaranteePeriodType? guaranteePeriodType = null;

        if (contractInfo.GuaranteeEndDate.HasValue)
        {
            isWithinGuarantee = returnDate <= contractInfo.GuaranteeEndDate.Value;
            // Infer guarantee period type from end date
            var guaranteeMonths = (contractInfo.GuaranteeEndDate.Value.DayNumber - contractStartDate.DayNumber) / 30;
            guaranteePeriodType = guaranteeMonths switch
            {
                <= 7 => Entities.GuaranteePeriodType.SixMonths,
                <= 13 => Entities.GuaranteePeriodType.OneYear,
                _ => Entities.GuaranteePeriodType.TwoYears,
            };
        }

        // Calculate refund
        var totalAmountPaid = contractInfo.TotalValue ?? 0m;
        var valuePerMonth = ContractDurationMonths > 0 ? totalAmountPaid / ContractDurationMonths : 0m;
        var refundAmount = Math.Max(0, totalAmountPaid - (monthsWorked * valuePerMonth));

        // Generate case code
        var lastCode = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CaseCode)
            .Select(x => x.CaseCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("RET-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var caseCode = $"RET-{nextNumber:D6}";

        var now = _clock.UtcNow;

        var returneeCase = new ReturneeCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CaseCode = caseCode,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            ClientId = request.ClientId,
            SupplierId = request.SupplierId,
            ReturnType = returnType,
            Status = ReturneeCaseStatus.Submitted,
            StatusChangedAt = now,
            ReturnDate = returnDate,
            ReturnReason = request.ReturnReason,
            MonthsWorked = monthsWorked,
            IsWithinGuarantee = isWithinGuarantee,
            GuaranteePeriodType = guaranteePeriodType,
            TotalAmountPaid = totalAmountPaid,
            RefundAmount = refundAmount,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        var history = new ReturneeCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturneeCaseId = returneeCase.Id,
            FromStatus = null,
            ToStatus = ReturneeCaseStatus.Submitted,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = "Case submitted",
        };

        _db.Set<ReturneeCase>().Add(returneeCase);
        _db.Set<ReturneeCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created returnee case {CaseCode} for worker {WorkerId}, contract {ContractId}",
            caseCode, request.WorkerId, request.ContractId);

        await _publisher.Publish(new ReturneeCaseCreatedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            ReturneeCaseId = returneeCase.Id,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            ClientId = request.ClientId,
            SupplierId = request.SupplierId,
            ReturnType = returnType.ToString(),
            ReturnReason = request.ReturnReason,
        }, ct);

        return Result<ReturneeCaseDto>.Success(MapToDto(returneeCase, false, false));
    }

    public async Task<Result<ReturneeCaseDto>> ApproveAsync(Guid tenantId, Guid id, ApproveReturneeCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<ReturneeCaseDto>.NotFound($"Returnee case with ID {id} not found");

        var allowedStatuses = new[] { ReturneeCaseStatus.Submitted, ReturneeCaseStatus.UnderReview };
        if (!allowedStatuses.Contains(rc.Status))
            return Result<ReturneeCaseDto>.ValidationError($"Case must be in 'Submitted' or 'UnderReview' status to approve. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = ReturneeCaseStatus.Approved;
        rc.StatusChangedAt = now;
        rc.ApprovedBy = _currentUser.UserId.ToString();
        rc.ApprovedAt = now;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new ReturneeCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturneeCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = ReturneeCaseStatus.Approved,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.Notes ?? "Case approved",
        };

        _db.Set<ReturneeCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Approved returnee case {CaseId}, return type: {ReturnType}", id, rc.ReturnType);

        await _publisher.Publish(new ReturneeCaseApprovedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            ReturneeCaseId = rc.Id,
            WorkerId = rc.WorkerId,
            ContractId = rc.ContractId,
            ClientId = rc.ClientId,
            SupplierId = rc.SupplierId,
            ReturnType = rc.ReturnType.ToString(),
            IsWithinGuarantee = rc.IsWithinGuarantee,
            RefundAmount = rc.RefundAmount,
            MonthsWorked = rc.MonthsWorked,
        }, ct);

        return Result<ReturneeCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<ReturneeCaseDto>> RejectAsync(Guid tenantId, Guid id, RejectReturneeCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<ReturneeCaseDto>.NotFound($"Returnee case with ID {id} not found");

        var allowedStatuses = new[] { ReturneeCaseStatus.Submitted, ReturneeCaseStatus.UnderReview };
        if (!allowedStatuses.Contains(rc.Status))
            return Result<ReturneeCaseDto>.ValidationError($"Case must be in 'Submitted' or 'UnderReview' status to reject. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = ReturneeCaseStatus.Rejected;
        rc.StatusChangedAt = now;
        rc.RejectedReason = request.Reason;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new ReturneeCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturneeCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = ReturneeCaseStatus.Rejected,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Reason = request.Reason,
            Notes = request.Notes ?? "Case rejected",
        };

        _db.Set<ReturneeCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Rejected returnee case {CaseId}", id);

        return Result<ReturneeCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<ReturneeCaseDto>> SettleAsync(Guid tenantId, Guid id, SettleReturneeCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<ReturneeCaseDto>.NotFound($"Returnee case with ID {id} not found");

        if (rc.Status != ReturneeCaseStatus.Approved)
            return Result<ReturneeCaseDto>.ValidationError($"Case must be in 'Approved' status to settle. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = ReturneeCaseStatus.Settled;
        rc.StatusChangedAt = now;
        rc.SettledAt = now;
        rc.SettlementNotes = request.SettlementNotes;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new ReturneeCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturneeCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = ReturneeCaseStatus.Settled,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.SettlementNotes ?? "Case settled",
        };

        _db.Set<ReturneeCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Settled returnee case {CaseId}", id);

        await _publisher.Publish(new ReturneeCaseSettledEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            ReturneeCaseId = rc.Id,
            WorkerId = rc.WorkerId,
            ContractId = rc.ContractId,
        }, ct);

        return Result<ReturneeCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<ReturneeExpenseDto>> AddExpenseAsync(Guid tenantId, Guid caseId, CreateReturneeExpenseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == caseId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<ReturneeExpenseDto>.NotFound($"Returnee case with ID {caseId} not found");

        if (rc.Status == ReturneeCaseStatus.Rejected)
            return Result<ReturneeExpenseDto>.ValidationError("Cannot add expenses to a rejected case");

        if (!Enum.TryParse<ExpenseType>(request.ExpenseType, ignoreCase: true, out var expenseType))
            return Result<ReturneeExpenseDto>.ValidationError($"Invalid expense type '{request.ExpenseType}'");

        if (!Enum.TryParse<PaidByParty>(request.PaidBy, ignoreCase: true, out var paidBy))
            return Result<ReturneeExpenseDto>.ValidationError($"Invalid paid by party '{request.PaidBy}'");

        var expense = new ReturneeExpense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturneeCaseId = caseId,
            ExpenseType = expenseType,
            Amount = request.Amount,
            Currency = rc.Currency,
            Description = request.Description,
            PaidBy = paidBy,
        };

        _db.Set<ReturneeExpense>().Add(expense);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Added expense {ExpenseType} ({Amount}) to returnee case {CaseId}",
            expenseType, request.Amount, caseId);

        return Result<ReturneeExpenseDto>.Success(MapToExpenseDto(expense));
    }

    public async Task<Result<RefundCalculationDto>> CalculateRefundAsync(Guid tenantId, Guid caseId, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == caseId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RefundCalculationDto>.NotFound($"Returnee case with ID {caseId} not found");

        var totalAmountPaid = rc.TotalAmountPaid ?? 0m;
        var valuePerMonth = ContractDurationMonths > 0 ? totalAmountPaid / ContractDurationMonths : 0m;
        var refundAmount = Math.Max(0, totalAmountPaid - (rc.MonthsWorked * valuePerMonth));

        return Result<RefundCalculationDto>.Success(new RefundCalculationDto
        {
            ContractId = rc.ContractId,
            TotalAmountPaid = totalAmountPaid,
            TotalContractMonths = ContractDurationMonths,
            MonthsWorked = rc.MonthsWorked,
            ValuePerMonth = Math.Round(valuePerMonth, 2),
            RefundAmount = Math.Round(refundAmount, 2),
            Currency = rc.Currency,
        });
    }

    public async Task<Result<List<ReturneeCaseStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!exists)
            return Result<List<ReturneeCaseStatusHistoryDto>>.NotFound($"Returnee case with ID {id} not found");

        var history = await _db.Set<ReturneeCaseStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ReturneeCaseId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<ReturneeCaseStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var rc = await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result.NotFound($"Returnee case with ID {id} not found");

        rc.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted returnee case {CaseId}", id);

        return Result.Success();
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<ReturneeCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count(), ct);
    }

    #region Helpers

    private static int CalculateMonthsWorked(DateOnly startDate, DateOnly returnDate)
    {
        var years = returnDate.Year - startDate.Year;
        var months = returnDate.Month - startDate.Month;
        var totalMonths = (years * 12) + months;

        // If the return day is before the start day, subtract a month (round down)
        if (returnDate.Day < startDate.Day)
            totalMonths--;

        return Math.Max(0, totalMonths);
    }

    #endregion

    #region Raw SQL DTOs

    private class WorkerInfoRaw
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    private class ContractInfoRaw
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public decimal? TotalValue { get; set; }
        public DateOnly? GuaranteeEndDate { get; set; }
    }

    private class RawBool
    {
        public bool Value { get; set; }
    }

    #endregion

    #region Mapping

    private ReturneeCaseDto MapToDto(ReturneeCase rc, bool includeStatusHistory, bool includeExpenses)
    {
        return new ReturneeCaseDto
        {
            Id = rc.Id,
            TenantId = rc.TenantId,
            CaseCode = rc.CaseCode,
            ReturnType = rc.ReturnType.ToString(),
            Status = rc.Status.ToString(),
            StatusChangedAt = rc.StatusChangedAt,
            WorkerId = rc.WorkerId,
            ContractId = rc.ContractId,
            ClientId = rc.ClientId,
            SupplierId = rc.SupplierId,
            ReturnDate = rc.ReturnDate,
            ReturnReason = rc.ReturnReason,
            MonthsWorked = rc.MonthsWorked,
            IsWithinGuarantee = rc.IsWithinGuarantee,
            GuaranteePeriodType = rc.GuaranteePeriodType?.ToString(),
            TotalAmountPaid = rc.TotalAmountPaid,
            RefundAmount = rc.RefundAmount,
            Currency = rc.Currency,
            ApprovedBy = rc.ApprovedBy,
            ApprovedAt = rc.ApprovedAt,
            RejectedReason = rc.RejectedReason,
            SettledAt = rc.SettledAt,
            SettlementNotes = rc.SettlementNotes,
            Notes = rc.Notes,
            CreatedBy = rc.CreatedBy,
            UpdatedBy = rc.UpdatedBy,
            CreatedAt = rc.CreatedAt,
            UpdatedAt = rc.UpdatedAt,
            StatusHistory = includeStatusHistory
                ? rc.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
            Expenses = includeExpenses
                ? rc.Expenses.Select(MapToExpenseDto).ToList()
                : null,
        };
    }

    private static ReturneeCaseStatusHistoryDto MapToHistoryDto(ReturneeCaseStatusHistory h)
    {
        return new ReturneeCaseStatusHistoryDto
        {
            Id = h.Id,
            ReturneeCaseId = h.ReturneeCaseId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    private static ReturneeExpenseDto MapToExpenseDto(ReturneeExpense e)
    {
        return new ReturneeExpenseDto
        {
            Id = e.Id,
            ReturneeCaseId = e.ReturneeCaseId,
            ExpenseType = e.ExpenseType.ToString(),
            Amount = e.Amount,
            Currency = e.Currency,
            Description = e.Description,
            PaidBy = e.PaidBy.ToString(),
        };
    }

    #endregion
}
