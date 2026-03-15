using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Runaway.Contracts;
using Runaway.Contracts.DTOs;
using Runaway.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Runaway.Core.Services;

public class RunawayService : IRunawayService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<RunawayService> _logger;

    private static readonly Dictionary<string, Expression<Func<RunawayCase, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["workerId"] = x => x.WorkerId,
        ["clientId"] = x => x.ClientId,
        ["contractId"] = x => x.ContractId,
        ["isWithinGuarantee"] = x => x.IsWithinGuarantee,
    };

    private static readonly Dictionary<string, Expression<Func<RunawayCase, object>>> SortableFields = new()
    {
        ["caseCode"] = x => x.CaseCode,
        ["status"] = x => x.Status,
        ["reportedDate"] = x => x.ReportedDate,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public RunawayService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<RunawayService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PagedList<RunawayCaseListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<RunawayCase>()
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
                (x.PoliceReportNumber != null && x.PoliceReportNumber.ToLower().Contains(searchLower)) ||
                (x.LastKnownLocation != null && x.LastKnownLocation.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => new RunawayCaseListDto
            {
                Id = x.Id,
                CaseCode = x.CaseCode,
                Status = x.Status.ToString(),
                WorkerId = x.WorkerId,
                ContractId = x.ContractId,
                ClientId = x.ClientId,
                SupplierId = x.SupplierId,
                ReportedDate = x.ReportedDate,
                IsWithinGuarantee = x.IsWithinGuarantee,
                PoliceReportNumber = x.PoliceReportNumber,
                CreatedAt = x.CreatedAt,
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<RunawayCaseDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);
        var includeExpenses = includes.Contains("expenses", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));

        if (includeExpenses)
            query = query.Include(x => x.Expenses);

        var rc = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (rc is null)
            return Result<RunawayCaseDto>.NotFound($"Runaway case with ID {id} not found");

        return Result<RunawayCaseDto>.Success(MapToDto(rc, includeStatusHistory, includeExpenses));
    }

    public async Task<Result<RunawayCaseDto>> ReportAsync(Guid tenantId, ReportRunawayCaseRequest request, CancellationToken ct = default)
    {
        // Validate worker exists via raw SQL
        var workerInfo = await _db.Database
            .SqlQueryRaw<WorkerInfoRaw>(
                "SELECT id AS \"Id\", status AS \"Status\" FROM workers WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
                request.WorkerId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (workerInfo is null)
            return Result<RunawayCaseDto>.NotFound($"Worker with ID {request.WorkerId} not found");

        // Validate contract exists via raw SQL
        var contractInfo = await _db.Database
            .SqlQueryRaw<ContractInfoRaw>(
                "SELECT id AS \"Id\", status AS \"Status\", start_date AS \"StartDate\", guarantee_end_date AS \"GuaranteeEndDate\" FROM contracts WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
                request.ContractId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (contractInfo is null)
            return Result<RunawayCaseDto>.NotFound($"Contract with ID {request.ContractId} not found");

        // Validate client exists via raw SQL
        var clientExists = await _db.Database
            .SqlQueryRaw<RawBool>(
                "SELECT EXISTS(SELECT 1 FROM clients WHERE id = {0} AND tenant_id = {1} AND is_deleted = false) AS \"Value\"",
                request.ClientId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (clientExists is null || !clientExists.Value)
            return Result<RunawayCaseDto>.ValidationError("Client not found");

        // Check no existing active runaway case for this worker
        var hasActiveCase = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.WorkerId == request.WorkerId
                && x.Status != RunawayCaseStatus.Closed, ct);

        if (hasActiveCase)
            return Result<RunawayCaseDto>.Conflict("An active runaway case already exists for this worker");

        // Determine guarantee status
        var isWithinGuarantee = false;
        GuaranteePeriodType? guaranteePeriodType = null;

        if (contractInfo.GuaranteeEndDate.HasValue)
        {
            var reportDate = DateOnly.FromDateTime(request.ReportedDate.DateTime);
            isWithinGuarantee = reportDate <= contractInfo.GuaranteeEndDate.Value;
            var guaranteeMonths = (contractInfo.GuaranteeEndDate.Value.DayNumber - contractInfo.StartDate.DayNumber) / 30;
            guaranteePeriodType = guaranteeMonths switch
            {
                <= 7 => Entities.GuaranteePeriodType.SixMonths,
                <= 13 => Entities.GuaranteePeriodType.OneYear,
                _ => Entities.GuaranteePeriodType.TwoYears,
            };
        }

        // Generate case code
        var lastCode = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CaseCode)
            .Select(x => x.CaseCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("RUN-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var caseCode = $"RUN-{nextNumber:D6}";

        var now = _clock.UtcNow;

        var runawayCase = new RunawayCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CaseCode = caseCode,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            ClientId = request.ClientId,
            SupplierId = request.SupplierId,
            Status = RunawayCaseStatus.Reported,
            StatusChangedAt = now,
            ReportedDate = request.ReportedDate,
            ReportedBy = request.ReportedBy,
            LastKnownLocation = request.LastKnownLocation,
            PoliceReportNumber = request.PoliceReportNumber,
            PoliceReportDate = request.PoliceReportDate,
            IsWithinGuarantee = isWithinGuarantee,
            GuaranteePeriodType = guaranteePeriodType,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        var history = new RunawayCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunawayCaseId = runawayCase.Id,
            FromStatus = null,
            ToStatus = RunawayCaseStatus.Reported,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = "Runaway case reported",
        };

        _db.Set<RunawayCase>().Add(runawayCase);
        _db.Set<RunawayCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Reported runaway case {CaseCode} for worker {WorkerId}, contract {ContractId}. Within guarantee: {IsWithinGuarantee}",
            caseCode, request.WorkerId, request.ContractId, isWithinGuarantee);

        await _publisher.Publish(new RunawayCaseReportedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            RunawayCaseId = runawayCase.Id,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            ClientId = request.ClientId,
            SupplierId = request.SupplierId,
            IsWithinGuarantee = isWithinGuarantee,
        }, ct);

        return Result<RunawayCaseDto>.Success(MapToDto(runawayCase, false, false));
    }

    public async Task<Result<RunawayCaseDto>> UpdateAsync(Guid tenantId, Guid id, UpdateRunawayCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RunawayCaseDto>.NotFound($"Runaway case with ID {id} not found");

        if (rc.Status == RunawayCaseStatus.Closed)
            return Result<RunawayCaseDto>.ValidationError("Cannot update a closed case");

        if (request.LastKnownLocation is not null) rc.LastKnownLocation = request.LastKnownLocation;
        if (request.PoliceReportNumber is not null) rc.PoliceReportNumber = request.PoliceReportNumber;
        if (request.PoliceReportDate.HasValue) rc.PoliceReportDate = request.PoliceReportDate;
        if (request.Notes is not null) rc.Notes = request.Notes;

        rc.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        return Result<RunawayCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<RunawayCaseDto>> ConfirmAsync(Guid tenantId, Guid id, ConfirmRunawayCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RunawayCaseDto>.NotFound($"Runaway case with ID {id} not found");

        var allowedStatuses = new[] { RunawayCaseStatus.Reported, RunawayCaseStatus.UnderInvestigation };
        if (!allowedStatuses.Contains(rc.Status))
            return Result<RunawayCaseDto>.ValidationError($"Case must be in 'Reported' or 'UnderInvestigation' status to confirm. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = RunawayCaseStatus.Confirmed;
        rc.StatusChangedAt = now;
        rc.ConfirmedAt = now;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new RunawayCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunawayCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = RunawayCaseStatus.Confirmed,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.Notes ?? "Case confirmed",
        };

        _db.Set<RunawayCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Confirmed runaway case {CaseId}", id);

        await _publisher.Publish(new RunawayCaseConfirmedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            RunawayCaseId = rc.Id,
            WorkerId = rc.WorkerId,
            ContractId = rc.ContractId,
            ClientId = rc.ClientId,
            SupplierId = rc.SupplierId,
            IsWithinGuarantee = rc.IsWithinGuarantee,
        }, ct);

        return Result<RunawayCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<RunawayCaseDto>> SettleAsync(Guid tenantId, Guid id, SettleRunawayCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RunawayCaseDto>.NotFound($"Runaway case with ID {id} not found");

        if (rc.Status != RunawayCaseStatus.Confirmed)
            return Result<RunawayCaseDto>.ValidationError($"Case must be in 'Confirmed' status to settle. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = RunawayCaseStatus.Settled;
        rc.StatusChangedAt = now;
        rc.SettledAt = now;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new RunawayCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunawayCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = RunawayCaseStatus.Settled,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.Notes ?? "Case settled",
        };

        _db.Set<RunawayCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Settled runaway case {CaseId}", id);

        return Result<RunawayCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<RunawayCaseDto>> CloseAsync(Guid tenantId, Guid id, CloseRunawayCaseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RunawayCaseDto>.NotFound($"Runaway case with ID {id} not found");

        if (rc.Status != RunawayCaseStatus.Settled)
            return Result<RunawayCaseDto>.ValidationError($"Case must be in 'Settled' status to close. Current: '{rc.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = rc.Status;

        rc.Status = RunawayCaseStatus.Closed;
        rc.StatusChangedAt = now;
        rc.ClosedAt = now;
        rc.UpdatedBy = _currentUser.UserId;

        var history = new RunawayCaseStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunawayCaseId = rc.Id,
            FromStatus = fromStatus,
            ToStatus = RunawayCaseStatus.Closed,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.Notes ?? "Case closed",
        };

        _db.Set<RunawayCaseStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Closed runaway case {CaseId}", id);

        return Result<RunawayCaseDto>.Success(MapToDto(rc, false, false));
    }

    public async Task<Result<RunawayExpenseDto>> AddExpenseAsync(Guid tenantId, Guid caseId, CreateRunawayExpenseRequest request, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == caseId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result<RunawayExpenseDto>.NotFound($"Runaway case with ID {caseId} not found");

        if (rc.Status == RunawayCaseStatus.Closed)
            return Result<RunawayExpenseDto>.ValidationError("Cannot add expenses to a closed case");

        if (!Enum.TryParse<RunawayExpenseType>(request.ExpenseType, ignoreCase: true, out var expenseType))
            return Result<RunawayExpenseDto>.ValidationError($"Invalid expense type '{request.ExpenseType}'");

        if (!Enum.TryParse<PaidByParty>(request.PaidBy, ignoreCase: true, out var paidBy))
            return Result<RunawayExpenseDto>.ValidationError($"Invalid paid by party '{request.PaidBy}'");

        var expense = new RunawayExpense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunawayCaseId = caseId,
            ExpenseType = expenseType,
            Amount = request.Amount,
            Currency = "AED",
            Description = request.Description,
            PaidBy = paidBy,
        };

        _db.Set<RunawayExpense>().Add(expense);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Added expense {ExpenseType} ({Amount}) to runaway case {CaseId}",
            expenseType, request.Amount, caseId);

        return Result<RunawayExpenseDto>.Success(MapToExpenseDto(expense));
    }

    public async Task<Result<List<RunawayCaseStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!exists)
            return Result<List<RunawayCaseStatusHistoryDto>>.NotFound($"Runaway case with ID {id} not found");

        var history = await _db.Set<RunawayCaseStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.RunawayCaseId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<RunawayCaseStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var rc = await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (rc is null)
            return Result.NotFound($"Runaway case with ID {id} not found");

        rc.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted runaway case {CaseId}", id);

        return Result.Success();
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<RunawayCase>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count(), ct);
    }

    #region Raw SQL DTOs

    private class WorkerInfoRaw
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class ContractInfoRaw
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? GuaranteeEndDate { get; set; }
    }

    private class RawBool
    {
        public bool Value { get; set; }
    }

    #endregion

    #region Mapping

    private RunawayCaseDto MapToDto(RunawayCase rc, bool includeStatusHistory, bool includeExpenses)
    {
        return new RunawayCaseDto
        {
            Id = rc.Id,
            TenantId = rc.TenantId,
            CaseCode = rc.CaseCode,
            Status = rc.Status.ToString(),
            StatusChangedAt = rc.StatusChangedAt,
            WorkerId = rc.WorkerId,
            ContractId = rc.ContractId,
            ClientId = rc.ClientId,
            SupplierId = rc.SupplierId,
            ReportedDate = rc.ReportedDate,
            ReportedBy = rc.ReportedBy,
            LastKnownLocation = rc.LastKnownLocation,
            PoliceReportNumber = rc.PoliceReportNumber,
            PoliceReportDate = rc.PoliceReportDate,
            IsWithinGuarantee = rc.IsWithinGuarantee,
            GuaranteePeriodType = rc.GuaranteePeriodType?.ToString(),
            Notes = rc.Notes,
            ConfirmedAt = rc.ConfirmedAt,
            SettledAt = rc.SettledAt,
            ClosedAt = rc.ClosedAt,
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

    private static RunawayCaseStatusHistoryDto MapToHistoryDto(RunawayCaseStatusHistory h)
    {
        return new RunawayCaseStatusHistoryDto
        {
            Id = h.Id,
            RunawayCaseId = h.RunawayCaseId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    private static RunawayExpenseDto MapToExpenseDto(RunawayExpense e)
    {
        return new RunawayExpenseDto
        {
            Id = e.Id,
            RunawayCaseId = e.RunawayCaseId,
            ExpenseType = e.ExpenseType.ToString(),
            Amount = e.Amount,
            Currency = e.Currency,
            Description = e.Description,
            PaidBy = e.PaidBy.ToString(),
        };
    }

    #endregion
}
