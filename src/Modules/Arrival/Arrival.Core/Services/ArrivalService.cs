using System.Linq.Expressions;
using Arrival.Contracts;
using Arrival.Contracts.DTOs;
using Arrival.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Arrival.Core.Services;

public class ArrivalService : IArrivalService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<ArrivalService> _logger;

    public ArrivalService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<ArrivalService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    private static readonly Dictionary<string, Expression<Func<Entities.Arrival, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["workerId"] = x => x.WorkerId,
        ["placementId"] = x => x.PlacementId,
        ["driverId"] = x => x.DriverId!,
        ["supplierId"] = x => x.SupplierId!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Arrival, object>>> SortableFields = new()
    {
        ["scheduledArrivalDate"] = x => x.ScheduledArrivalDate,
        ["createdAt"] = x => x.CreatedAt,
        ["status"] = x => x.Status,
        ["arrivalCode"] = x => x.ArrivalCode,
    };

    public async Task<PagedList<ArrivalListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        query = query.ApplyFilters(qp.Filters, FilterableFields);

        // Date range filter
        var dateFromFilter = qp.Filters.FirstOrDefault(f => f.Name == "dateFrom");
        if (dateFromFilter != null)
        {
            var dateFromStr = dateFromFilter.Values.FirstOrDefault();
            if (dateFromStr != null && DateOnly.TryParse(dateFromStr, out var dateFrom))
            {
                query = query.Where(x => x.ScheduledArrivalDate >= dateFrom);
            }
        }
        var dateToFilter = qp.Filters.FirstOrDefault(f => f.Name == "dateTo");
        if (dateToFilter != null)
        {
            var dateToStr = dateToFilter.Values.FirstOrDefault();
            if (dateToStr != null && DateOnly.TryParse(dateToStr, out var dateTo))
            {
                query = query.Where(x => x.ScheduledArrivalDate <= dateTo);
            }
        }

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x =>
                x.ArrivalCode.ToLower().Contains(search) ||
                (x.FlightNumber != null && x.FlightNumber.ToLower().Contains(search)) ||
                (x.DriverName != null && x.DriverName.ToLower().Contains(search)));
        }

        query = query.ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ArrivalDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id);

        var includes = qp?.Include?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        if (includes.Contains("statusHistory", StringComparer.OrdinalIgnoreCase))
        {
            query = query.Include(x => x.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        var arrival = await query.FirstOrDefaultAsync(ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ScheduleArrivalAsync(Guid tenantId, ScheduleArrivalRequest request, CancellationToken ct = default)
    {
        // Check for existing active arrival for this placement
        var existingArrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.PlacementId == request.PlacementId
                && x.Status != ArrivalStatus.NoShow
                && x.Status != ArrivalStatus.Cancelled, ct);

        if (existingArrival)
            return Result<ArrivalDto>.Conflict("An active arrival already exists for this placement");

        // Generate arrival code
        var maxCode = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ArrivalCode)
            .Select(x => x.ArrivalCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (maxCode != null && maxCode.StartsWith("ARR-") && int.TryParse(maxCode[4..], out var current))
        {
            nextNumber = current + 1;
        }

        var now = _clock.UtcNow;
        var arrival = new Entities.Arrival
        {
            TenantId = tenantId,
            ArrivalCode = $"ARR-{nextNumber:D6}",
            Status = ArrivalStatus.Scheduled,
            StatusChangedAt = now,
            WorkerId = request.WorkerId,
            PlacementId = request.PlacementId,
            SupplierId = request.SupplierId,
            FlightNumber = request.FlightNumber,
            AirportCode = request.AirportCode,
            AirportName = request.AirportName,
            ScheduledArrivalDate = request.ScheduledArrivalDate,
            ScheduledArrivalTime = request.ScheduledArrivalTime,
            Notes = request.Notes,
        };

        arrival.StatusHistory.Add(new ArrivalStatusHistory
        {
            TenantId = tenantId,
            FromStatus = null,
            ToStatus = ArrivalStatus.Scheduled,
            ChangedAt = now,
            ChangedBy = _currentUser.UserId.ToString(),
            Notes = "Arrival scheduled",
        });

        _db.Set<Entities.Arrival>().Add(arrival);
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new ArrivalScheduledEvent
        {
            TenantId = tenantId,
            ArrivalId = arrival.Id,
            PlacementId = arrival.PlacementId,
            WorkerId = arrival.WorkerId,
            ScheduledArrivalDate = arrival.ScheduledArrivalDate,
            FlightNumber = arrival.FlightNumber,
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("Arrival {Code} scheduled for placement {PlacementId}", arrival.ArrivalCode, arrival.PlacementId);

        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> UpdateAsync(Guid tenantId, Guid id, UpdateArrivalRequest request, CancellationToken ct = default)
    {
        var arrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status != ArrivalStatus.Scheduled)
            return Result<ArrivalDto>.ValidationError("Can only update arrivals in Scheduled status");

        if (request.FlightNumber != null) arrival.FlightNumber = request.FlightNumber;
        if (request.AirportCode != null) arrival.AirportCode = request.AirportCode;
        if (request.AirportName != null) arrival.AirportName = request.AirportName;
        if (request.ScheduledArrivalDate.HasValue) arrival.ScheduledArrivalDate = request.ScheduledArrivalDate.Value;
        if (request.ScheduledArrivalTime.HasValue) arrival.ScheduledArrivalTime = request.ScheduledArrivalTime.Value;
        if (request.Notes != null) arrival.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Arrival {Code} updated", arrival.ArrivalCode);

        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> AssignDriverAsync(Guid tenantId, Guid id, AssignDriverRequest request, CancellationToken ct = default)
    {
        var arrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status == ArrivalStatus.Cancelled || arrival.Status == ArrivalStatus.NoShow)
            return Result<ArrivalDto>.ValidationError("Cannot assign driver to a cancelled or no-show arrival");

        arrival.DriverId = request.DriverId;
        arrival.DriverName = request.DriverName;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Driver {DriverName} assigned to arrival {Code}", request.DriverName, arrival.ArrivalCode);

        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ConfirmArrivalAsync(Guid tenantId, Guid id, ConfirmArrivalRequest request, CancellationToken ct = default)
    {
        var arrival = await GetArrivalForTransition(tenantId, id, ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status != ArrivalStatus.Scheduled && arrival.Status != ArrivalStatus.InTransit)
            return Result<ArrivalDto>.ValidationError("Arrival can only be confirmed from Scheduled or InTransit status");

        var now = _clock.UtcNow;
        var fromStatus = arrival.Status;
        arrival.Status = ArrivalStatus.Arrived;
        arrival.StatusChangedAt = now;
        arrival.ActualArrivalTime = request.ActualArrivalTime ?? now;

        AddStatusHistory(arrival, fromStatus, ArrivalStatus.Arrived, request.Notes);
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new ArrivalConfirmedEvent
        {
            TenantId = tenantId,
            ArrivalId = arrival.Id,
            PlacementId = arrival.PlacementId,
            WorkerId = arrival.WorkerId,
            ActualArrivalTime = arrival.ActualArrivalTime.Value,
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("Arrival {Code} confirmed", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ConfirmPickupAsync(Guid tenantId, Guid id, ConfirmPickupRequest request, CancellationToken ct = default)
    {
        var arrival = await GetArrivalForTransition(tenantId, id, ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status != ArrivalStatus.Arrived)
            return Result<ArrivalDto>.ValidationError("Pickup can only be confirmed when status is Arrived");

        var now = _clock.UtcNow;
        arrival.Status = ArrivalStatus.PickedUp;
        arrival.StatusChangedAt = now;
        arrival.DriverConfirmedPickupAt = now;

        AddStatusHistory(arrival, ArrivalStatus.Arrived, ArrivalStatus.PickedUp, request.Notes);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Pickup confirmed for arrival {Code}", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ConfirmAtAccommodationAsync(Guid tenantId, Guid id, ConfirmAccommodationRequest request, CancellationToken ct = default)
    {
        var arrival = await GetArrivalForTransition(tenantId, id, ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status != ArrivalStatus.PickedUp && arrival.Status != ArrivalStatus.Arrived)
            return Result<ArrivalDto>.ValidationError("Accommodation can only be confirmed from PickedUp or Arrived status");

        var now = _clock.UtcNow;
        var fromStatus = arrival.Status;
        arrival.Status = ArrivalStatus.AtAccommodation;
        arrival.StatusChangedAt = now;
        arrival.AccommodationConfirmedAt = now;
        arrival.AccommodationConfirmedBy = request.ConfirmedBy ?? _currentUser.UserId.ToString();

        AddStatusHistory(arrival, fromStatus, ArrivalStatus.AtAccommodation, request.Notes);
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new MaidAtAccommodationEvent
        {
            TenantId = tenantId,
            ArrivalId = arrival.Id,
            PlacementId = arrival.PlacementId,
            WorkerId = arrival.WorkerId,
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("Maid at accommodation confirmed for arrival {Code}", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ConfirmCustomerPickupAsync(Guid tenantId, Guid id, ConfirmCustomerPickupRequest request, CancellationToken ct = default)
    {
        var arrival = await GetArrivalForTransition(tenantId, id, ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status != ArrivalStatus.Arrived && arrival.Status != ArrivalStatus.PickedUp)
            return Result<ArrivalDto>.ValidationError("Customer pickup can only be confirmed from Arrived or PickedUp status");

        var now = _clock.UtcNow;
        var fromStatus = arrival.Status;
        arrival.CustomerPickedUp = true;
        arrival.CustomerPickupConfirmedAt = now;
        arrival.Status = ArrivalStatus.PickedUp;
        arrival.StatusChangedAt = now;

        AddStatusHistory(arrival, fromStatus, ArrivalStatus.PickedUp, request.Notes ?? "Customer picked up directly");
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Customer pickup confirmed for arrival {Code}", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result<ArrivalDto>> ReportNoShowAsync(Guid tenantId, Guid id, ReportNoShowRequest request, CancellationToken ct = default)
    {
        var arrival = await GetArrivalForTransition(tenantId, id, ct);
        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        if (arrival.Status == ArrivalStatus.AtAccommodation || arrival.Status == ArrivalStatus.NoShow || arrival.Status == ArrivalStatus.Cancelled)
            return Result<ArrivalDto>.ValidationError("Cannot report no-show for this arrival status");

        var now = _clock.UtcNow;
        var fromStatus = arrival.Status;
        arrival.Status = ArrivalStatus.NoShow;
        arrival.StatusChangedAt = now;

        AddStatusHistory(arrival, fromStatus, ArrivalStatus.NoShow, request.Notes, request.Reason);
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new MaidNoShowEvent
        {
            TenantId = tenantId,
            ArrivalId = arrival.Id,
            PlacementId = arrival.PlacementId,
            WorkerId = arrival.WorkerId,
            SupplierId = arrival.SupplierId,
            ScheduledArrivalDate = arrival.ScheduledArrivalDate,
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("No-show reported for arrival {Code}", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var arrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (arrival == null)
            return Result.NotFound("Arrival not found");

        arrival.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Arrival {Code} deleted", arrival.ArrivalCode);
        return Result.Success();
    }

    public async Task<Result<List<ArrivalStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var exists = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (!exists)
            return Result<List<ArrivalStatusHistoryDto>>.NotFound("Arrival not found");

        var history = await _db.Set<ArrivalStatusHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ArrivalId == id && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => MapToHistoryDto(x))
            .ToListAsync(ct);

        return Result<List<ArrivalStatusHistoryDto>>.Success(history);
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count(), ct);
    }

    public async Task<Result<ArrivalDto>> SetDriverPickupPhotoAsync(Guid tenantId, Guid id, string photoUrl, CancellationToken ct = default)
    {
        var arrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (arrival == null)
            return Result<ArrivalDto>.NotFound("Arrival not found");

        arrival.DriverPickupPhotoUrl = photoUrl;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Driver pickup photo uploaded for arrival {Code}", arrival.ArrivalCode);
        return Result<ArrivalDto>.Success(MapToDto(arrival));
    }

    // ── Helpers ──

    private async Task<Entities.Arrival?> GetArrivalForTransition(Guid tenantId, Guid id, CancellationToken ct)
    {
        // Use IgnoreQueryFilters() to bypass tenant/soft-delete filters (since we
        // filter manually), but do NOT Include navigations — adding child entities
        // to included collections causes EF to generate UPDATE instead of INSERT,
        // triggering DbUpdateConcurrencyException. Status history is added directly
        // via _db.Set<ArrivalStatusHistory>().Add() instead.
        return await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);
    }

    private void AddStatusHistory(Entities.Arrival arrival, ArrivalStatus fromStatus, ArrivalStatus toStatus, string? notes = null, string? reason = null)
    {
        // Add directly to DbSet instead of navigation collection to avoid
        // change tracker issues when parent was loaded with IgnoreQueryFilters.
        _db.Set<ArrivalStatusHistory>().Add(new ArrivalStatusHistory
        {
            ArrivalId = arrival.Id,
            TenantId = arrival.TenantId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedAt = _clock.UtcNow,
            ChangedBy = _currentUser.UserId.ToString(),
            Reason = reason,
            Notes = notes,
        });
    }

    // ── Mapping ──

    private static ArrivalDto MapToDto(Entities.Arrival x)
    {
        return new ArrivalDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ArrivalCode = x.ArrivalCode,
            Status = x.Status.ToString(),
            StatusChangedAt = x.StatusChangedAt,
            WorkerId = x.WorkerId,
            PlacementId = x.PlacementId,
            SupplierId = x.SupplierId,
            FlightNumber = x.FlightNumber,
            AirportCode = x.AirportCode,
            AirportName = x.AirportName,
            ScheduledArrivalDate = x.ScheduledArrivalDate,
            ScheduledArrivalTime = x.ScheduledArrivalTime,
            ActualArrivalTime = x.ActualArrivalTime,
            PreTravelPhotoUrl = x.PreTravelPhotoUrl,
            ArrivalPhotoUrl = x.ArrivalPhotoUrl,
            DriverPickupPhotoUrl = x.DriverPickupPhotoUrl,
            DriverId = x.DriverId,
            DriverName = x.DriverName,
            DriverConfirmedPickupAt = x.DriverConfirmedPickupAt,
            AccommodationConfirmedAt = x.AccommodationConfirmedAt,
            AccommodationConfirmedBy = x.AccommodationConfirmedBy,
            CustomerPickedUp = x.CustomerPickedUp,
            CustomerPickupConfirmedAt = x.CustomerPickupConfirmedAt,
            Notes = x.Notes,
            CreatedBy = x.CreatedBy,
            UpdatedBy = x.UpdatedBy,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            StatusHistory = x.StatusHistory?.OrderByDescending(h => h.ChangedAt)
                .Select(h => MapToHistoryDto(h)).ToList(),
        };
    }

    private static ArrivalListDto MapToListDto(Entities.Arrival x)
    {
        return new ArrivalListDto
        {
            Id = x.Id,
            ArrivalCode = x.ArrivalCode,
            Status = x.Status.ToString(),
            StatusChangedAt = x.StatusChangedAt,
            WorkerId = x.WorkerId,
            PlacementId = x.PlacementId,
            SupplierId = x.SupplierId,
            FlightNumber = x.FlightNumber,
            AirportCode = x.AirportCode,
            ScheduledArrivalDate = x.ScheduledArrivalDate,
            ScheduledArrivalTime = x.ScheduledArrivalTime,
            ActualArrivalTime = x.ActualArrivalTime,
            DriverId = x.DriverId,
            DriverName = x.DriverName,
            CustomerPickedUp = x.CustomerPickedUp,
            CreatedAt = x.CreatedAt,
        };
    }

    private static ArrivalStatusHistoryDto MapToHistoryDto(ArrivalStatusHistory h)
    {
        return new ArrivalStatusHistoryDto
        {
            Id = h.Id,
            ArrivalId = h.ArrivalId,
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedAt = h.ChangedAt,
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            Notes = h.Notes,
        };
    }
}
