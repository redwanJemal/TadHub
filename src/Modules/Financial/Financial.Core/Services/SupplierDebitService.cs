using System.Linq.Expressions;
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

namespace Financial.Core.Services;

public class SupplierDebitService : ISupplierDebitService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SupplierDebitService> _logger;

    private static readonly Dictionary<string, Expression<Func<SupplierDebit, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["supplierId"] = x => x.SupplierId,
        ["workerId"] = x => x.WorkerId!,
        ["contractId"] = x => x.ContractId!,
        ["debitType"] = x => x.DebitType,
        ["caseType"] = x => x.CaseType!,
    };

    private static readonly Dictionary<string, Expression<Func<SupplierDebit, object>>> SortableFields = new()
    {
        ["debitNumber"] = x => x.DebitNumber,
        ["status"] = x => x.Status,
        ["amount"] = x => x.Amount,
        ["debitType"] = x => x.DebitType,
        ["createdAt"] = x => x.CreatedAt,
    };

    public SupplierDebitService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<SupplierDebitService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<SupplierDebitListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.DebitNumber.ToLower().Contains(searchLower) ||
                x.Description.ToLower().Contains(searchLower));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<SupplierDebitDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var debit = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (debit is null)
            return Result<SupplierDebitDto>.NotFound($"Supplier debit with ID {id} not found");

        return Result<SupplierDebitDto>.Success(MapToDto(debit));
    }

    public async Task<Result<SupplierDebitDto>> CreateAsync(Guid tenantId, CreateSupplierDebitRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<SupplierDebitType>(request.DebitType, ignoreCase: true, out var debitType))
            return Result<SupplierDebitDto>.ValidationError($"Invalid debit type '{request.DebitType}'");

        SupplierDebitCaseType? caseType = null;
        if (request.CaseType is not null && !Enum.TryParse(request.CaseType, ignoreCase: true, out SupplierDebitCaseType parsed))
            return Result<SupplierDebitDto>.ValidationError($"Invalid case type '{request.CaseType}'");
        else if (request.CaseType is not null)
            caseType = Enum.Parse<SupplierDebitCaseType>(request.CaseType, ignoreCase: true);

        // Generate debit number
        var lastNumber = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.DebitNumber)
            .Select(x => x.DebitNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastNumber is not null)
        {
            var dashIndex = lastNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastNumber[(dashIndex + 1)..], out var parsedNum))
                nextNumber = parsedNum + 1;
        }

        var debit = new SupplierDebit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DebitNumber = $"SDBT-{nextNumber:D6}",
            Status = SupplierDebitStatus.Outstanding,
            SupplierId = request.SupplierId,
            WorkerId = request.WorkerId,
            ContractId = request.ContractId,
            CaseType = caseType,
            CaseId = request.CaseId,
            DebitType = debitType,
            Description = request.Description,
            Amount = request.Amount,
            Currency = request.Currency,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<SupplierDebit>().Add(debit);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created supplier debit {DebitNumber} for supplier {SupplierId}", debit.DebitNumber, request.SupplierId);

        return Result<SupplierDebitDto>.Success(MapToDto(debit));
    }

    public async Task<Result<SupplierDebitDto>> UpdateAsync(Guid tenantId, Guid id, UpdateSupplierDebitRequest request, CancellationToken ct = default)
    {
        var debit = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (debit is null)
            return Result<SupplierDebitDto>.NotFound($"Supplier debit with ID {id} not found");

        if (request.Description is not null) debit.Description = request.Description;
        if (request.Amount.HasValue) debit.Amount = request.Amount.Value;
        if (request.Notes is not null) debit.Notes = request.Notes;

        debit.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated supplier debit {DebitId}", id);

        return Result<SupplierDebitDto>.Success(MapToDto(debit));
    }

    public async Task<Result<SupplierDebitDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionSupplierDebitStatusRequest request, CancellationToken ct = default)
    {
        var debit = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (debit is null)
            return Result<SupplierDebitDto>.NotFound($"Supplier debit with ID {id} not found");

        if (!Enum.TryParse<SupplierDebitStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<SupplierDebitDto>.ValidationError($"Invalid status '{request.Status}'");

        var fromStatus = debit.Status;
        debit.Status = targetStatus;

        if (targetStatus == SupplierDebitStatus.Settled)
        {
            debit.SettledAt = _clock.UtcNow;
            debit.SettlementPaymentId = request.SettlementPaymentId;
        }

        debit.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned supplier debit {DebitId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<SupplierDebitDto>.Success(MapToDto(debit));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var debit = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (debit is null)
            return Result.NotFound($"Supplier debit with ID {id} not found");

        debit.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted supplier debit {DebitId}", id);

        return Result.Success();
    }

    #region Mapping

    private static SupplierDebitDto MapToDto(SupplierDebit d)
    {
        return new SupplierDebitDto
        {
            Id = d.Id,
            TenantId = d.TenantId,
            DebitNumber = d.DebitNumber,
            Status = d.Status.ToString(),
            SupplierId = d.SupplierId,
            WorkerId = d.WorkerId,
            ContractId = d.ContractId,
            CaseType = d.CaseType?.ToString(),
            CaseId = d.CaseId,
            DebitType = d.DebitType.ToString(),
            Description = d.Description,
            Amount = d.Amount,
            Currency = d.Currency,
            SettlementPaymentId = d.SettlementPaymentId,
            SettledAt = d.SettledAt,
            Notes = d.Notes,
            CreatedBy = d.CreatedBy,
            UpdatedBy = d.UpdatedBy,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
        };
    }

    private static SupplierDebitListDto MapToListDto(SupplierDebit d)
    {
        return new SupplierDebitListDto
        {
            Id = d.Id,
            DebitNumber = d.DebitNumber,
            Status = d.Status.ToString(),
            SupplierId = d.SupplierId,
            WorkerId = d.WorkerId,
            ContractId = d.ContractId,
            CaseType = d.CaseType?.ToString(),
            DebitType = d.DebitType.ToString(),
            Description = d.Description,
            Amount = d.Amount,
            Currency = d.Currency,
            CreatedAt = d.CreatedAt,
        };
    }

    #endregion
}
