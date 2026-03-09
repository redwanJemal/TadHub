using System.Linq.Expressions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Placement.Contracts;
using Placement.Contracts.DTOs;
using Placement.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Candidate.Contracts;
using Client.Contracts;

namespace Placement.Core.Services;

public class PlacementService : IPlacementService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<PlacementService> _logger;
    private readonly ICandidateService _candidateService;
    private readonly IClientService _clientService;

    private static readonly Dictionary<string, Expression<Func<Entities.Placement, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["candidateId"] = x => x.CandidateId,
        ["clientId"] = x => x.ClientId,
        ["workerId"] = x => x.WorkerId!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Placement, object>>> SortableFields = new()
    {
        ["placementCode"] = x => x.PlacementCode,
        ["status"] = x => x.Status,
        ["bookedAt"] = x => x.BookedAt,
        ["statusChangedAt"] = x => x.StatusChangedAt,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
    };

    // Active pipeline statuses (not terminal) — includes both legacy and new outside-country steps
    private static readonly PlacementStatus[] ActiveStatuses =
    [
        PlacementStatus.Booked,
        PlacementStatus.ContractCreated,
        PlacementStatus.EmploymentVisaProcessing,
        PlacementStatus.TicketArranged,
        PlacementStatus.InTransit,
        PlacementStatus.Arrived,
        PlacementStatus.MedicalInProgress,
        PlacementStatus.MedicalCleared,
        PlacementStatus.GovtProcessing,
        PlacementStatus.GovtCleared,
        PlacementStatus.Training,
        PlacementStatus.ReadyForPlacement,
        PlacementStatus.Deployed,
        PlacementStatus.Placed,
        PlacementStatus.FullPaymentReceived,
        PlacementStatus.ResidenceVisaProcessing,
        PlacementStatus.EmiratesIdProcessing,
    ];

    public PlacementService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<PlacementService> logger,
        ICandidateService candidateService,
        IClientService clientService)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
        _candidateService = candidateService;
        _clientService = clientService;
    }

    public async Task<PagedList<PlacementListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.PlacementCode.ToLower().Contains(searchLower));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<PlacementDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeStatusHistory = includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase);
        var includeCostItems = includes.Contains("costItems", StringComparer.OrdinalIgnoreCase);
        var includeChecklist = includes.Contains("checklist", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (includeStatusHistory)
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));

        if (includeCostItems)
            query = query.Include(x => x.CostItems);

        var placement = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (placement is null)
            return Result<PlacementDto>.NotFound($"Placement with ID {id} not found");

        var dto = MapToDto(placement, includeStatusHistory, includeCostItems);

        if (includeChecklist)
        {
            dto = dto with { Checklist = BuildChecklist(placement) };
        }

        return Result<PlacementDto>.Success(dto);
    }

    public async Task<Result<PlacementDto>> CreateAsync(Guid tenantId, CreatePlacementRequest request, CancellationToken ct = default)
    {
        // Validate candidate exists and is Approved
        var candidateResult = await _candidateService.GetByIdAsync(tenantId, request.CandidateId, ct: ct);
        if (!candidateResult.IsSuccess)
            return Result<PlacementDto>.NotFound($"Candidate with ID {request.CandidateId} not found");

        if (candidateResult.Value!.Status != "Approved")
            return Result<PlacementDto>.ValidationError($"Candidate must be in 'Approved' status to create a placement. Current status: '{candidateResult.Value.Status}'");

        // Validate client exists and is active
        var clientResult = await _clientService.GetByIdAsync(tenantId, request.ClientId, ct);
        if (!clientResult.IsSuccess)
            return Result<PlacementDto>.NotFound($"Client with ID {request.ClientId} not found");

        if (!clientResult.Value!.IsActive)
            return Result<PlacementDto>.ValidationError("Client must be active to create a placement");

        // Check no existing active placement for this candidate
        var hasActivePlacement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.CandidateId == request.CandidateId
                && x.Status != PlacementStatus.Completed
                && x.Status != PlacementStatus.Cancelled, ct);

        if (hasActivePlacement)
            return Result<PlacementDto>.Conflict("Candidate already has an active placement");

        // Generate placement code
        var lastCode = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PlacementCode)
            .Select(x => x.PlacementCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("PLC-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var placementCode = $"PLC-{nextNumber:D6}";

        var now = _clock.UtcNow;
        var placement = new Entities.Placement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlacementCode = placementCode,
            Status = PlacementStatus.Booked,
            StatusChangedAt = now,
            CandidateId = request.CandidateId,
            ClientId = request.ClientId,
            BookedBy = _currentUser.UserId,
            BookedByName = _currentUser.Email,
            BookedAt = now,
            BookingNotes = request.BookingNotes,
            CreatedBy = _currentUser.UserId,
        };

        var history = new PlacementStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlacementId = placement.Id,
            FromStatus = null,
            ToStatus = PlacementStatus.Booked,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = "Placement created",
        };

        _db.Set<Entities.Placement>().Add(placement);
        _db.Set<PlacementStatusHistory>().Add(history);

        // Add initial cost items if provided
        if (request.InitialCostItems?.Count > 0)
        {
            foreach (var item in request.InitialCostItems)
            {
                if (!Enum.TryParse<PlacementCostType>(item.CostType, ignoreCase: true, out var costType))
                    continue;

                Enum.TryParse<PlacementCostStatus>(item.Status, ignoreCase: true, out var costStatus);

                _db.Set<PlacementCostItem>().Add(new PlacementCostItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PlacementId = placement.Id,
                    CostType = costType,
                    Description = item.Description,
                    Amount = item.Amount,
                    Currency = item.Currency,
                    Status = costStatus,
                    CostDate = item.CostDate,
                    ReferenceNumber = item.ReferenceNumber,
                    Notes = item.Notes,
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created placement {PlacementCode} for candidate {CandidateId} and client {ClientId}",
            placementCode, request.CandidateId, request.ClientId);

        // Publish event
        await _publisher.Publish(new PlacementCreatedEvent
        {
            OccurredAt = now,
            TenantId = tenantId,
            PlacementId = placement.Id,
            CandidateId = request.CandidateId,
            ClientId = request.ClientId,
            BookedBy = _currentUser.UserId,
        }, ct);

        return Result<PlacementDto>.Success(MapToDto(placement, includeStatusHistory: false, includeCostItems: false));
    }

    public async Task<Result<PlacementDto>> UpdateAsync(Guid tenantId, Guid id, UpdatePlacementRequest request, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result<PlacementDto>.NotFound($"Placement with ID {id} not found");

        if (request.TicketDate.HasValue) placement.TicketDate = request.TicketDate.Value;
        if (request.FlightDetails is not null) placement.FlightDetails = request.FlightDetails;
        if (request.ExpectedArrivalDate.HasValue) placement.ExpectedArrivalDate = request.ExpectedArrivalDate.Value;
        if (request.BookingNotes is not null) placement.BookingNotes = request.BookingNotes;
        if (request.ContractId.HasValue) placement.ContractId = request.ContractId.Value;
        if (request.EmploymentVisaApplicationId.HasValue) placement.EmploymentVisaApplicationId = request.EmploymentVisaApplicationId.Value;
        if (request.ResidenceVisaApplicationId.HasValue) placement.ResidenceVisaApplicationId = request.ResidenceVisaApplicationId.Value;
        if (request.EmiratesIdApplicationId.HasValue) placement.EmiratesIdApplicationId = request.EmiratesIdApplicationId.Value;
        if (request.ArrivalId.HasValue) placement.ArrivalId = request.ArrivalId.Value;

        placement.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated placement {PlacementId}", id);

        return Result<PlacementDto>.Success(MapToDto(placement, includeStatusHistory: false, includeCostItems: false));
    }

    public async Task<Result<PlacementDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionPlacementStatusRequest request, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result<PlacementDto>.NotFound($"Placement with ID {id} not found");

        if (!Enum.TryParse<PlacementStatus>(request.Status, ignoreCase: true, out var targetStatus))
            return Result<PlacementDto>.ValidationError($"Invalid status '{request.Status}'");

        var error = PlacementStatusMachine.Validate(placement.Status, targetStatus, request.Reason);
        if (error is not null)
            return Result<PlacementDto>.ValidationError(error);

        return await ApplyTransition(placement, targetStatus, request.Reason, request.Notes, ct);
    }

    public async Task<Result<PlacementDto>> AdvanceStepAsync(Guid tenantId, Guid id, AdvancePlacementStepRequest request, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .Include(x => x.CostItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result<PlacementDto>.NotFound($"Placement with ID {id} not found");

        var nextStep = PlacementStatusMachine.GetNextOutsideCountryStep(placement.Status);
        if (nextStep is null)
            return Result<PlacementDto>.ValidationError($"Cannot advance from status '{placement.Status}'. No next step in the outside-country pipeline.");

        // Validate prerequisites for each step
        var validationError = ValidateStepPrerequisites(placement, nextStep.Value);
        if (validationError is not null)
            return Result<PlacementDto>.ValidationError(validationError);

        return await ApplyTransition(placement, nextStep.Value, null, request.Notes, ct);
    }

    public async Task<Result<PlacementChecklistDto>> GetChecklistAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.CostItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result<PlacementChecklistDto>.NotFound($"Placement with ID {id} not found");

        return Result<PlacementChecklistDto>.Success(BuildChecklist(placement));
    }

    public async Task<Result<List<PlacementStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (!exists)
            return Result<List<PlacementStatusHistoryDto>>.NotFound($"Placement with ID {id} not found");

        var history = await _db.Set<PlacementStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.PlacementId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<PlacementStatusHistoryDto>>.Success(history);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result.NotFound($"Placement with ID {id} not found");

        placement.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted placement {PlacementId}", id);

        return Result.Success();
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count(), ct);
    }

    public async Task<Result<PlacementBoardDto>> GetBoardAsync(Guid tenantId, CancellationToken ct = default)
    {
        var placements = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.CostItems)
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && ActiveStatuses.Contains(x.Status))
            .OrderBy(x => x.StatusChangedAt)
            .ToListAsync(ct);

        var statusCounts = placements
            .GroupBy(x => x.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // Use outside-country pipeline statuses for board columns
        foreach (var status in PlacementStatusMachine.OutsideCountryPipeline)
        {
            statusCounts.TryAdd(status.ToString(), 0);
        }

        var columns = placements
            .GroupBy(x => x.Status)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Select(x => MapToListDto(x)).ToList());

        foreach (var status in PlacementStatusMachine.OutsideCountryPipeline)
        {
            columns.TryAdd(status.ToString(), []);
        }

        return Result<PlacementBoardDto>.Success(new PlacementBoardDto
        {
            StatusCounts = statusCounts,
            Columns = columns,
        });
    }

    // Cost item operations
    public async Task<Result<PlacementCostItemDto>> AddCostItemAsync(Guid tenantId, Guid placementId, CreatePlacementCostItemRequest request, CancellationToken ct = default)
    {
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == placementId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (placement is null)
            return Result<PlacementCostItemDto>.NotFound($"Placement with ID {placementId} not found");

        if (!Enum.TryParse<PlacementCostType>(request.CostType, ignoreCase: true, out var costType))
            return Result<PlacementCostItemDto>.ValidationError($"Invalid cost type '{request.CostType}'");

        Enum.TryParse<PlacementCostStatus>(request.Status, ignoreCase: true, out var costStatus);

        var item = new PlacementCostItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlacementId = placementId,
            CostType = costType,
            Description = request.Description,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = costStatus,
            CostDate = request.CostDate,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
        };

        _db.Set<PlacementCostItem>().Add(item);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Added cost item {CostType} ({Amount}) to placement {PlacementId}",
            costType, request.Amount, placementId);

        return Result<PlacementCostItemDto>.Success(MapToCostItemDto(item));
    }

    public async Task<Result<PlacementCostItemDto>> UpdateCostItemAsync(Guid tenantId, Guid placementId, Guid itemId, UpdatePlacementCostItemRequest request, CancellationToken ct = default)
    {
        var item = await _db.Set<PlacementCostItem>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == itemId && x.PlacementId == placementId && x.TenantId == tenantId, ct);

        if (item is null)
            return Result<PlacementCostItemDto>.NotFound($"Cost item with ID {itemId} not found");

        if (request.CostType is not null && Enum.TryParse<PlacementCostType>(request.CostType, ignoreCase: true, out var ct2))
            item.CostType = ct2;
        if (request.Description is not null) item.Description = request.Description;
        if (request.Amount.HasValue) item.Amount = request.Amount.Value;
        if (request.Currency is not null) item.Currency = request.Currency;
        if (request.Status is not null && Enum.TryParse<PlacementCostStatus>(request.Status, ignoreCase: true, out var cs))
            item.Status = cs;
        if (request.CostDate.HasValue) item.CostDate = request.CostDate.Value;
        if (request.PaidAt.HasValue) item.PaidAt = request.PaidAt.Value;
        if (request.ReferenceNumber is not null) item.ReferenceNumber = request.ReferenceNumber;
        if (request.Notes is not null) item.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);

        return Result<PlacementCostItemDto>.Success(MapToCostItemDto(item));
    }

    public async Task<Result> DeleteCostItemAsync(Guid tenantId, Guid placementId, Guid itemId, CancellationToken ct = default)
    {
        var item = await _db.Set<PlacementCostItem>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == itemId && x.PlacementId == placementId && x.TenantId == tenantId, ct);

        if (item is null)
            return Result.NotFound($"Cost item with ID {itemId} not found");

        _db.Set<PlacementCostItem>().Remove(item);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    #region Step Validation

    private string? ValidateStepPrerequisites(Entities.Placement placement, PlacementStatus nextStep)
    {
        return nextStep switch
        {
            // Step 2: Contract must be linked
            PlacementStatus.ContractCreated =>
                placement.ContractId is null
                    ? "A contract must be created and linked to this placement before advancing to 'Contract Created'. Set the contractId via the update endpoint."
                    : null,

            // Step 3: Employment visa — contract must exist
            PlacementStatus.EmploymentVisaProcessing =>
                placement.ContractId is null
                    ? "Contract must be created before starting employment visa processing."
                    : null,

            // Step 4: Ticket — employment visa should be linked
            PlacementStatus.TicketArranged =>
                placement.TicketDate is null
                    ? "Ticket date must be set before advancing to 'Ticket Arranged'. Update the placement with ticketDate first."
                    : null,

            // Step 5: Arrived — ticket must be arranged
            PlacementStatus.Arrived => null, // No additional prereqs beyond state machine

            // Step 6: Deployed — maid arrived
            PlacementStatus.Deployed => null,

            // Step 7: Full payment — check cost items have at least one paid
            PlacementStatus.FullPaymentReceived =>
                (placement.CostItems == null || !placement.CostItems.Any(c => c.Status == PlacementCostStatus.Paid))
                    ? "At least one cost item must be marked as 'Paid' before confirming full payment received."
                    : null,

            // Step 8: Residence visa — full payment must be received
            PlacementStatus.ResidenceVisaProcessing => null,

            // Step 9: Emirates ID — residence visa processing must have started
            PlacementStatus.EmiratesIdProcessing => null,

            _ => null,
        };
    }

    #endregion

    #region Checklist Builder

    private static PlacementChecklistDto BuildChecklist(Entities.Placement placement)
    {
        var currentStepNum = PlacementStatusMachine.GetStepNumber(placement.Status);
        var isTerminal = PlacementStatusMachine.IsTerminal(placement.Status);

        var steps = new List<PlacementChecklistStepDto>
        {
            BuildStep(1, PlacementStatus.Booked, "Booking", "Candidate booked with partial/advance payment",
                placement.BookedAt, placement, "Record Payment", null, null),

            BuildStep(2, PlacementStatus.ContractCreated, "Contract Creation", "2-year employment contract created",
                placement.ContractCreatedAt, placement, "Create Contract", placement.ContractId, "Contract"),

            BuildStep(3, PlacementStatus.EmploymentVisaProcessing, "Employment Visa", "Employment visa application submitted and processed",
                placement.EmploymentVisaStartedAt, placement, "Start Visa Application", placement.EmploymentVisaApplicationId, "VisaApplication"),

            BuildStep(4, PlacementStatus.TicketArranged, "Ticket Processing", "Flight ticket issued and travel date set",
                placement.TicketDate.HasValue ? placement.StatusChangedAt : null, placement, "Arrange Ticket", null, null),

            BuildStep(5, PlacementStatus.Arrived, "Arrival", "Maid arrived and processed through arrival management",
                placement.ArrivedAt, placement, "Confirm Arrival", placement.ArrivalId, "Arrival"),

            BuildStep(6, PlacementStatus.Deployed, "Deployment", "Maid deployed to customer household",
                placement.DeployedAt, placement, "Confirm Deployment", null, null),

            BuildStep(7, PlacementStatus.FullPaymentReceived, "Full Payment", "Remaining balance paid by customer",
                placement.FullPaymentReceivedAt, placement, "Verify Payment", null, null),

            BuildStep(8, PlacementStatus.ResidenceVisaProcessing, "Residence Visa", "Residence visa application submitted",
                placement.ResidenceVisaStartedAt, placement, "Start Residence Visa", placement.ResidenceVisaApplicationId, "VisaApplication"),

            BuildStep(9, PlacementStatus.EmiratesIdProcessing, "Emirates ID", "Emirates ID application submitted",
                placement.EmiratesIdStartedAt, placement, "Start Emirates ID", placement.EmiratesIdApplicationId, "VisaApplication"),
        };

        var completedSteps = isTerminal && placement.Status == PlacementStatus.Completed
            ? 9
            : currentStepNum > 0 ? currentStepNum - 1 : 0;

        return new PlacementChecklistDto
        {
            Steps = steps,
            CurrentStepNumber = currentStepNum,
            TotalSteps = 9,
            ProgressPercent = Math.Round(completedSteps / 9.0 * 100, 1),
        };
    }

    private static PlacementChecklistStepDto BuildStep(
        int stepNumber,
        PlacementStatus stepStatus,
        string label,
        string description,
        DateTimeOffset? completedAt,
        Entities.Placement placement,
        string actionLabel,
        Guid? linkedEntityId,
        string? linkedEntityType)
    {
        var currentStepNum = PlacementStatusMachine.GetStepNumber(placement.Status);
        var isCompleted = placement.Status == PlacementStatus.Completed;

        string stepState;
        if (isCompleted || stepNumber < currentStepNum)
            stepState = "Completed";
        else if (stepNumber == currentStepNum)
            stepState = "InProgress";
        else
            stepState = "Pending";

        return new PlacementChecklistStepDto
        {
            StepNumber = stepNumber,
            Status = stepStatus.ToString(),
            StepStatus = stepState,
            Label = label,
            Description = description,
            CompletedAt = stepState == "Completed" ? completedAt : null,
            ActionLabel = stepState != "Completed" ? actionLabel : null,
            LinkedEntityId = linkedEntityId,
            LinkedEntityType = linkedEntityType,
        };
    }

    #endregion

    #region Transition Helper

    private async Task<Result<PlacementDto>> ApplyTransition(
        Entities.Placement placement,
        PlacementStatus targetStatus,
        string? reason,
        string? notes,
        CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var fromStatus = placement.Status;

        placement.Status = targetStatus;
        placement.StatusChangedAt = now;
        placement.StatusReason = reason;
        placement.UpdatedBy = _currentUser.UserId;

        // Set pipeline timestamp fields
        switch (targetStatus)
        {
            case PlacementStatus.ContractCreated:
                placement.ContractCreatedAt = now;
                break;
            case PlacementStatus.EmploymentVisaProcessing:
                placement.EmploymentVisaStartedAt = now;
                break;
            case PlacementStatus.Arrived:
                placement.ArrivedAt = now;
                break;
            case PlacementStatus.Deployed:
                placement.DeployedAt = now;
                break;
            case PlacementStatus.FullPaymentReceived:
                placement.FullPaymentReceivedAt = now;
                break;
            case PlacementStatus.ResidenceVisaProcessing:
                placement.ResidenceVisaStartedAt = now;
                break;
            case PlacementStatus.EmiratesIdProcessing:
                placement.EmiratesIdStartedAt = now;
                break;
            case PlacementStatus.MedicalCleared:
                placement.MedicalClearedAt = now;
                break;
            case PlacementStatus.GovtCleared:
                placement.GovtClearedAt = now;
                break;
            case PlacementStatus.Placed:
                placement.PlacedAt = now;
                break;
            case PlacementStatus.Completed:
                placement.CompletedAt = now;
                break;
            case PlacementStatus.Cancelled:
                placement.CancelledAt = now;
                placement.CancellationReason = reason;
                break;
        }

        var history = new PlacementStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = placement.TenantId,
            PlacementId = placement.Id,
            FromStatus = fromStatus,
            ToStatus = targetStatus,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Reason = reason,
            Notes = notes,
        };

        _db.Set<PlacementStatusHistory>().Add(history);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned placement {PlacementId} from {FromStatus} to {ToStatus}",
            placement.Id, fromStatus, targetStatus);

        // Publish event
        await _publisher.Publish(new PlacementStatusChangedEvent
        {
            OccurredAt = now,
            TenantId = placement.TenantId,
            PlacementId = placement.Id,
            CandidateId = placement.CandidateId,
            WorkerId = placement.WorkerId,
            FromStatus = fromStatus.ToString(),
            ToStatus = targetStatus.ToString(),
            Reason = reason,
        }, ct);

        return Result<PlacementDto>.Success(MapToDto(placement, includeStatusHistory: false, includeCostItems: false));
    }

    #endregion

    #region Mapping

    private static PlacementDto MapToDto(Entities.Placement p, bool includeStatusHistory, bool includeCostItems)
    {
        return new PlacementDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            PlacementCode = p.PlacementCode,
            Status = p.Status.ToString(),
            StatusChangedAt = p.StatusChangedAt,
            StatusReason = p.StatusReason,
            CandidateId = p.CandidateId,
            ClientId = p.ClientId,
            WorkerId = p.WorkerId,
            ContractId = p.ContractId,
            EmploymentVisaApplicationId = p.EmploymentVisaApplicationId,
            ResidenceVisaApplicationId = p.ResidenceVisaApplicationId,
            EmiratesIdApplicationId = p.EmiratesIdApplicationId,
            ArrivalId = p.ArrivalId,
            BookedBy = p.BookedBy,
            BookedByName = p.BookedByName,
            BookedAt = p.BookedAt,
            BookingNotes = p.BookingNotes,
            ContractCreatedAt = p.ContractCreatedAt,
            EmploymentVisaStartedAt = p.EmploymentVisaStartedAt,
            TicketDate = p.TicketDate,
            FlightDetails = p.FlightDetails,
            ExpectedArrivalDate = p.ExpectedArrivalDate,
            ArrivedAt = p.ArrivedAt,
            DeployedAt = p.DeployedAt,
            FullPaymentReceivedAt = p.FullPaymentReceivedAt,
            ResidenceVisaStartedAt = p.ResidenceVisaStartedAt,
            EmiratesIdStartedAt = p.EmiratesIdStartedAt,
            MedicalClearedAt = p.MedicalClearedAt,
            GovtClearedAt = p.GovtClearedAt,
            PlacedAt = p.PlacedAt,
            CompletedAt = p.CompletedAt,
            CancelledAt = p.CancelledAt,
            CancellationReason = p.CancellationReason,
            Currency = p.Currency,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            TotalCost = p.CostItems?.Sum(c => c.Status != PlacementCostStatus.Cancelled ? c.Amount : 0),
            StatusHistory = includeStatusHistory
                ? p.StatusHistory.Select(MapToHistoryDto).ToList()
                : null,
            CostItems = includeCostItems
                ? (p.CostItems ?? []).Select(MapToCostItemDto).ToList()
                : null,
        };
    }

    private static PlacementListDto MapToListDto(Entities.Placement p)
    {
        var stepNum = PlacementStatusMachine.GetStepNumber(p.Status);
        return new PlacementListDto
        {
            Id = p.Id,
            PlacementCode = p.PlacementCode,
            Status = p.Status.ToString(),
            StatusChangedAt = p.StatusChangedAt,
            CandidateId = p.CandidateId,
            ClientId = p.ClientId,
            WorkerId = p.WorkerId,
            ContractId = p.ContractId,
            BookedAt = p.BookedAt,
            ExpectedArrivalDate = p.ExpectedArrivalDate,
            TotalCost = p.CostItems?.Where(c => c.Status != PlacementCostStatus.Cancelled).Sum(c => c.Amount) ?? 0,
            CreatedAt = p.CreatedAt,
            CurrentStep = stepNum > 0 ? stepNum : 0,
            TotalSteps = 9,
        };
    }

    private static PlacementStatusHistoryDto MapToHistoryDto(PlacementStatusHistory h)
    {
        return new PlacementStatusHistoryDto
        {
            Id = h.Id,
            PlacementId = h.PlacementId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }

    private static PlacementCostItemDto MapToCostItemDto(PlacementCostItem c)
    {
        return new PlacementCostItemDto
        {
            Id = c.Id,
            PlacementId = c.PlacementId,
            CostType = c.CostType.ToString(),
            Description = c.Description,
            Amount = c.Amount,
            Currency = c.Currency,
            Status = c.Status.ToString(),
            CostDate = c.CostDate,
            PaidAt = c.PaidAt,
            ReferenceNumber = c.ReferenceNumber,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
        };
    }

    #endregion
}
