using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trial.Contracts;
using Trial.Contracts.DTOs;
using Trial.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Trial.Core.Services;

public class TrialService : ITrialService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<TrialService> _logger;

    private const int TrialDurationDays = 5;

    private static readonly Dictionary<string, Expression<Func<Entities.Trial, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["workerId"] = x => x.WorkerId,
        ["clientId"] = x => x.ClientId,
        ["placementId"] = x => x.PlacementId!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Trial, object>>> SortableFields = new()
    {
        ["trialCode"] = x => x.TrialCode,
        ["status"] = x => x.Status,
        ["startDate"] = x => x.StartDate,
        ["endDate"] = x => x.EndDate,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    public TrialService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<TrialService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PagedList<TrialListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.TrialCode.ToLower().Contains(searchLower));
        }

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        return await query
            .Select(x => new TrialListDto
            {
                Id = x.Id,
                TrialCode = x.TrialCode,
                Status = x.Status.ToString(),
                WorkerId = x.WorkerId,
                ClientId = x.ClientId,
                PlacementId = x.PlacementId,
                ContractId = x.ContractId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DaysRemaining = x.Status == TrialStatus.Active
                    ? Math.Max(0, x.EndDate.DayNumber - today.DayNumber)
                    : 0,
                Outcome = x.Outcome != null ? x.Outcome.ToString() : null,
                CreatedAt = x.CreatedAt,
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<TrialDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));

        var trial = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (trial is null)
            return Result<TrialDto>.NotFound($"Trial with ID {id} not found");

        return Result<TrialDto>.Success(MapToDto(trial, includeStatusHistory));
    }

    public async Task<Result<TrialDto>> CreateAsync(Guid tenantId, CreateTrialRequest request, CancellationToken ct = default)
    {
        // Validate worker exists and is inside country via raw SQL
        var workerInfo = await _db.Database
            .SqlQueryRaw<WorkerInfoRaw>(
                "SELECT id AS \"Id\", status AS \"Status\", location AS \"Location\" FROM workers WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
                request.WorkerId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (workerInfo is null)
            return Result<TrialDto>.NotFound($"Worker with ID {request.WorkerId} not found");

        if (workerInfo.Location != "InCountry")
            return Result<TrialDto>.ValidationError("Only workers located inside the country can be put on trial");

        var allowedStatuses = new[] { "Available", "NewArrival" };
        if (!allowedStatuses.Contains(workerInfo.Status))
            return Result<TrialDto>.ValidationError($"Worker must be in 'Available' or 'NewArrival' status to start a trial. Current status: '{workerInfo.Status}'");

        // Validate client exists via raw SQL
        var clientExists = await _db.Database
            .SqlQueryRaw<RawBool>(
                "SELECT EXISTS(SELECT 1 FROM clients WHERE id = {0} AND tenant_id = {1} AND is_deleted = false AND is_active = true) AS \"Value\"",
                request.ClientId, tenantId)
            .FirstOrDefaultAsync(ct);

        if (clientExists is null || !clientExists.Value)
            return Result<TrialDto>.ValidationError("Client not found or is not active");

        // Check no existing active trial for this worker
        var hasActiveTrial = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.WorkerId == request.WorkerId
                && x.Status == TrialStatus.Active, ct);

        if (hasActiveTrial)
            return Result<TrialDto>.Conflict("Worker already has an active trial");

        // Generate trial code
        var lastCode = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.TrialCode)
            .Select(x => x.TrialCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("TRL-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var trialCode = $"TRL-{nextNumber:D6}";

        var now = _clock.UtcNow;
        var endDate = request.StartDate.AddDays(TrialDurationDays);

        var trial = new Entities.Trial
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrialCode = trialCode,
            Status = TrialStatus.Active,
            StatusChangedAt = now,
            WorkerId = request.WorkerId,
            ClientId = request.ClientId,
            PlacementId = request.PlacementId,
            StartDate = request.StartDate,
            EndDate = endDate,
            CreatedBy = _currentUser.UserId,
        };

        var history = new TrialStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrialId = trial.Id,
            FromStatus = null,
            ToStatus = TrialStatus.Active,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.Notes ?? "Trial created",
        };

        _db.Set<Entities.Trial>().Add(trial);
        _db.Set<TrialStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created trial {TrialCode} for worker {WorkerId} and client {ClientId}",
            trialCode, request.WorkerId, request.ClientId);

        await _publisher.Publish(new TrialCreatedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            TrialId = trial.Id,
            WorkerId = request.WorkerId,
            ClientId = request.ClientId,
            PlacementId = request.PlacementId,
            StartDate = request.StartDate,
            EndDate = endDate,
        }, ct);

        return Result<TrialDto>.Success(MapToDto(trial, includeStatusHistory: false));
    }

    public async Task<Result<TrialDto>> CompleteAsync(Guid tenantId, Guid id, CompleteTrialRequest request, CancellationToken ct = default)
    {
        var trial = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (trial is null)
            return Result<TrialDto>.NotFound($"Trial with ID {id} not found");

        if (trial.Status != TrialStatus.Active)
            return Result<TrialDto>.ValidationError($"Only active trials can be completed. Current status: '{trial.Status}'");

        if (!Enum.TryParse<TrialOutcome>(request.Outcome, ignoreCase: true, out var outcome))
            return Result<TrialDto>.ValidationError($"Invalid outcome '{request.Outcome}'. Must be 'ProceedToContract' or 'ReturnToInventory'");

        var now = _clock.UtcNow;
        var fromStatus = trial.Status;
        var targetStatus = outcome == TrialOutcome.ProceedToContract ? TrialStatus.Successful : TrialStatus.Failed;

        trial.Status = targetStatus;
        trial.StatusChangedAt = now;
        trial.Outcome = outcome;
        trial.OutcomeNotes = request.OutcomeNotes;
        trial.OutcomeDate = now;
        trial.UpdatedBy = _currentUser.UserId;

        var history = new TrialStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrialId = trial.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = request.OutcomeNotes ?? $"Trial completed with outcome: {outcome}",
        };

        _db.Set<TrialStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Completed trial {TrialId} with outcome {Outcome}", id, outcome);

        await _publisher.Publish(new TrialCompletedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            TrialId = trial.Id,
            WorkerId = trial.WorkerId,
            ClientId = trial.ClientId,
            PlacementId = trial.PlacementId,
            Outcome = outcome.ToString(),
            OutcomeNotes = request.OutcomeNotes,
        }, ct);

        return Result<TrialDto>.Success(MapToDto(trial, includeStatusHistory: false));
    }

    public async Task<Result<TrialDto>> CancelAsync(Guid tenantId, Guid id, CancelTrialRequest request, CancellationToken ct = default)
    {
        var trial = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (trial is null)
            return Result<TrialDto>.NotFound($"Trial with ID {id} not found");

        if (trial.Status != TrialStatus.Active)
            return Result<TrialDto>.ValidationError($"Only active trials can be cancelled. Current status: '{trial.Status}'");

        var now = _clock.UtcNow;
        var fromStatus = trial.Status;

        trial.Status = TrialStatus.Cancelled;
        trial.StatusChangedAt = now;
        trial.UpdatedBy = _currentUser.UserId;

        var history = new TrialStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrialId = trial.Id,
            FromStatus = fromStatus,
            ToStatus = TrialStatus.Cancelled,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Reason = request.Reason,
            Notes = "Trial cancelled",
        };

        _db.Set<TrialStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled trial {TrialId}", id);

        await _publisher.Publish(new TrialCancelledEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            TrialId = trial.Id,
            WorkerId = trial.WorkerId,
            ClientId = trial.ClientId,
            Reason = request.Reason,
        }, ct);

        return Result<TrialDto>.Success(MapToDto(trial, includeStatusHistory: false));
    }

    public async Task<Result<List<TrialStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!exists)
            return Result<List<TrialStatusHistoryDto>>.NotFound($"Trial with ID {id} not found");

        var history = await _db.Set<TrialStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TrialId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<TrialStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var trial = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (trial is null)
            return Result.NotFound($"Trial with ID {id} not found");

        trial.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted trial {TrialId}", id);

        return Result.Success();
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<Entities.Trial>()
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
        public string Location { get; set; } = string.Empty;
    }

    private class RawBool
    {
        public bool Value { get; set; }
    }

    #endregion

    #region Mapping

    private TrialDto MapToDto(Entities.Trial t, bool includeStatusHistory)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        return new TrialDto
        {
            Id = t.Id,
            TenantId = t.TenantId,
            TrialCode = t.TrialCode,
            Status = t.Status.ToString(),
            StatusChangedAt = t.StatusChangedAt,
            WorkerId = t.WorkerId,
            ClientId = t.ClientId,
            PlacementId = t.PlacementId,
            ContractId = t.ContractId,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            DaysRemaining = t.Status == TrialStatus.Active
                ? Math.Max(0, t.EndDate.DayNumber - today.DayNumber)
                : 0,
            Outcome = t.Outcome?.ToString(),
            OutcomeNotes = t.OutcomeNotes,
            OutcomeDate = t.OutcomeDate,
            CreatedBy = t.CreatedBy,
            UpdatedBy = t.UpdatedBy,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            StatusHistory = includeStatusHistory
                ? t.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
        };
    }

    private static TrialStatusHistoryDto MapToHistoryDto(TrialStatusHistory h)
    {
        return new TrialStatusHistoryDto
        {
            Id = h.Id,
            TrialId = h.TrialId,
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
