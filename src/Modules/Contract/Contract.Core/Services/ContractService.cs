using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Contract.Contracts;
using Contract.Contracts.DTOs;
using Contract.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Worker.Core.Entities;

namespace Contract.Core.Services;

public class ContractService : IContractService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ContractService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Contract, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["type"] = x => x.Type,
        ["workerId"] = x => x.WorkerId,
        ["clientId"] = x => x.ClientId,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Contract, object>>> SortableFields = new()
    {
        ["contractCode"] = x => x.ContractCode,
        ["status"] = x => x.Status,
        ["type"] = x => x.Type,
        ["startDate"] = x => x.StartDate,
        ["rate"] = x => x.Rate,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public ContractService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<ContractService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<ContractListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.ContractCode.ToLower().Contains(searchLower));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ContractDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
        {
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        var contract = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (contract is null)
            return Result<ContractDto>.NotFound($"Contract with ID {id} not found");

        return Result<ContractDto>.Success(MapToDto(contract, includeStatusHistory));
    }

    public async Task<Result<ContractDto>> CreateAsync(Guid tenantId, CreateContractRequest request, CancellationToken ct = default)
    {
        // Validate contract type
        if (!Enum.TryParse<ContractType>(request.Type, ignoreCase: true, out var contractType))
            return Result<ContractDto>.ValidationError($"Invalid contract type '{request.Type}'");

        // Validate rate period
        if (!Enum.TryParse<RatePeriod>(request.RatePeriod, ignoreCase: true, out var ratePeriod))
            return Result<ContractDto>.ValidationError($"Invalid rate period '{request.RatePeriod}'");

        // Validate worker exists and is Available
        var worker = await _db.Set<Worker.Core.Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.WorkerId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (worker is null)
            return Result<ContractDto>.NotFound($"Worker with ID {request.WorkerId} not found");

        if (worker.Status != WorkerStatus.Available)
            return Result<ContractDto>.ValidationError($"Worker must be in 'Available' status to create a contract. Current status: '{worker.Status}'");

        // Validate client exists and is active
        var client = await _db.Set<Client.Core.Entities.Client>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.ClientId && x.TenantId == tenantId, ct);

        if (client is null)
            return Result<ContractDto>.NotFound($"Client with ID {request.ClientId} not found");

        if (!client.IsActive)
            return Result<ContractDto>.ValidationError("Client must be active to create a contract");

        // Check no existing active contract for this worker
        var hasActiveContract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.WorkerId == request.WorkerId
                && x.Status != ContractStatus.Closed
                && x.Status != ContractStatus.Cancelled
                && x.Status != ContractStatus.Terminated, ct);

        if (hasActiveContract)
            return Result<ContractDto>.Conflict("Worker already has an active contract");

        // Generate contract code
        var lastCode = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ContractCode)
            .Select(x => x.ContractCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("CTR-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var contractCode = $"CTR-{nextNumber:D6}";

        var now = _clock.UtcNow;
        var contract = new Entities.Contract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractCode = contractCode,
            Type = contractType,
            Status = ContractStatus.Draft,
            StatusChangedAt = now,
            WorkerId = request.WorkerId,
            ClientId = request.ClientId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ProbationEndDate = request.ProbationEndDate,
            GuaranteeEndDate = request.GuaranteeEndDate,
            Rate = request.Rate,
            RatePeriod = ratePeriod,
            Currency = request.Currency,
            TotalValue = request.TotalValue,
            OriginalContractId = request.OriginalContractId,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId,
        };

        var history = new ContractStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractId = contract.Id,
            FromStatus = null,
            ToStatus = ContractStatus.Draft,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
        };

        _db.Set<Entities.Contract>().Add(contract);
        _db.Set<ContractStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created contract {ContractCode} for worker {WorkerId} and client {ClientId}", contractCode, request.WorkerId, request.ClientId);

        return Result<ContractDto>.Success(MapToDto(contract, includeStatusHistory: false));
    }

    public async Task<Result<ContractDto>> UpdateAsync(Guid tenantId, Guid id, UpdateContractRequest request, CancellationToken ct = default)
    {
        var contract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (contract is null)
            return Result<ContractDto>.NotFound($"Contract with ID {id} not found");

        // Apply updates (only non-null values)
        if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
        if (request.ProbationEndDate.HasValue) contract.ProbationEndDate = request.ProbationEndDate.Value;
        if (request.GuaranteeEndDate.HasValue) contract.GuaranteeEndDate = request.GuaranteeEndDate.Value;
        if (request.ProbationPassed.HasValue) contract.ProbationPassed = request.ProbationPassed.Value;
        if (request.Rate.HasValue) contract.Rate = request.Rate.Value;
        if (request.RatePeriod is not null && Enum.TryParse<RatePeriod>(request.RatePeriod, ignoreCase: true, out var rp))
            contract.RatePeriod = rp;
        if (request.Currency is not null) contract.Currency = request.Currency;
        if (request.TotalValue.HasValue) contract.TotalValue = request.TotalValue.Value;
        if (request.Notes is not null) contract.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated contract {ContractId}", id);

        return Result<ContractDto>.Success(MapToDto(contract, includeStatusHistory: false));
    }

    public async Task<Result<ContractDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionContractStatusRequest request, CancellationToken ct = default)
    {
        var contract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (contract is null)
            return Result<ContractDto>.NotFound($"Contract with ID {id} not found");

        if (!Enum.TryParse<ContractStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<ContractDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = ContractStatusMachine.Validate(contract.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<ContractDto>.ValidationError(error);

        var now = _clock.UtcNow;
        var fromStatus = contract.Status;

        contract.Status = targetStatus;
        contract.StatusChangedAt = now;
        contract.StatusReason = request.Reason;

        // Handle termination
        if (targetStatus == ContractStatus.Terminated)
        {
            contract.TerminatedAt = now;
            contract.TerminationReason = request.Reason;
        }

        // Sync worker status
        await SyncWorkerStatus(tenantId, contract.WorkerId, fromStatus, targetStatus, request.Reason, now, ct);

        var history = new ContractStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractId = contract.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
            Reason = request.Reason,
            Notes = request.Notes,
        };

        _db.Set<ContractStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned contract {ContractId} from {FromStatus} to {ToStatus}", id, fromStatus, targetStatus);

        return Result<ContractDto>.Success(MapToDto(contract, includeStatusHistory: false));
    }

    public async Task<Result<List<ContractStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var contractExists = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!contractExists)
            return Result<List<ContractStatusHistoryDto>>.NotFound($"Contract with ID {id} not found");

        var history = await _db.Set<ContractStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ContractId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<ContractStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var contract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (contract is null)
            return Result.NotFound($"Contract with ID {id} not found");

        contract.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted contract {ContractId}", id);

        return Result.Success();
    }

    #region Worker Status Sync

    private async Task SyncWorkerStatus(
        Guid tenantId, Guid workerId,
        ContractStatus fromContract, ContractStatus toContract,
        string? reason, DateTimeOffset now,
        CancellationToken ct)
    {
        var worker = await _db.Set<Worker.Core.Entities.Worker>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == workerId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (worker is null) return;

        WorkerStatus? targetWorkerStatus = (fromContract, toContract) switch
        {
            (ContractStatus.Draft, ContractStatus.Confirmed) => WorkerStatus.Booked,
            (ContractStatus.Confirmed, ContractStatus.OnProbation) => WorkerStatus.OnProbation,
            (ContractStatus.Confirmed, ContractStatus.Active) => WorkerStatus.Active,
            (ContractStatus.OnProbation, ContractStatus.Active) => WorkerStatus.Active,
            (_, ContractStatus.Cancelled) when worker.Status == WorkerStatus.Booked => WorkerStatus.Available,
            (_, ContractStatus.Cancelled) when worker.Status == WorkerStatus.OnProbation => WorkerStatus.Available,
            (_, ContractStatus.Terminated) => WorkerStatus.PendingReplacement,
            (ContractStatus.Completed, ContractStatus.Closed) => WorkerStatus.Available,
            (ContractStatus.Terminated, ContractStatus.Closed) => WorkerStatus.Available,
            _ => null,
        };

        if (targetWorkerStatus is null) return;

        var fromWorkerStatus = worker.Status;
        worker.Status = targetWorkerStatus.Value;
        worker.StatusChangedAt = now;
        worker.StatusReason = reason;

        if (targetWorkerStatus == WorkerStatus.Active && worker.ActivatedAt is null)
            worker.ActivatedAt = now;

        if (targetWorkerStatus == WorkerStatus.PendingReplacement || targetWorkerStatus == WorkerStatus.Terminated)
        {
            worker.TerminatedAt = now;
            worker.TerminationReason = reason;
        }

        var workerHistory = new WorkerStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkerId = worker.Id,
            FromStatus = fromWorkerStatus,
            ToStatus = targetWorkerStatus.Value,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId,
            Reason = $"Contract status changed: {fromContract} → {toContract}",
            Notes = reason,
        };

        _db.Set<WorkerStatusHistory>().Add(workerHistory);
    }

    #endregion

    #region Mapping

    private static ContractDto MapToDto(Entities.Contract c, bool includeStatusHistory)
    {
        return new ContractDto
        {
            Id = c.Id,
            TenantId = c.TenantId,
            ContractCode = c.ContractCode,
            Type = c.Type.ToString(),
            Status = c.Status.ToString(),
            StatusChangedAt = c.StatusChangedAt,
            StatusReason = c.StatusReason,
            WorkerId = c.WorkerId,
            ClientId = c.ClientId,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            ProbationEndDate = c.ProbationEndDate,
            GuaranteeEndDate = c.GuaranteeEndDate,
            ProbationPassed = c.ProbationPassed,
            Rate = c.Rate,
            RatePeriod = c.RatePeriod.ToString(),
            Currency = c.Currency,
            TotalValue = c.TotalValue,
            TerminatedAt = c.TerminatedAt,
            TerminationReason = c.TerminationReason,
            TerminatedBy = c.TerminatedBy?.ToString(),
            ReplacementContractId = c.ReplacementContractId,
            OriginalContractId = c.OriginalContractId,
            Notes = c.Notes,
            CreatedBy = c.CreatedBy,
            UpdatedBy = c.UpdatedBy,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            StatusHistory = includeStatusHistory
                ? c.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
        };
    }

    private static ContractListDto MapToListDto(Entities.Contract c)
    {
        return new ContractListDto
        {
            Id = c.Id,
            ContractCode = c.ContractCode,
            Type = c.Type.ToString(),
            Status = c.Status.ToString(),
            WorkerId = c.WorkerId,
            ClientId = c.ClientId,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Rate = c.Rate,
            RatePeriod = c.RatePeriod.ToString(),
            Currency = c.Currency,
            CreatedAt = c.CreatedAt,
        };
    }

    private static ContractStatusHistoryDto MapToHistoryDto(ContractStatusHistory h)
    {
        return new ContractStatusHistoryDto
        {
            Id = h.Id,
            ContractId = h.ContractId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    #endregion
}
