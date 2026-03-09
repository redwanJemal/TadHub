using System.Linq.Expressions;
using Accommodation.Contracts;
using Accommodation.Contracts.DTOs;
using Accommodation.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Accommodation.Core.Services;

public class AccommodationService : IAccommodationService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<AccommodationService> _logger;

    public AccommodationService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        IPublishEndpoint publisher,
        ILogger<AccommodationService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _publisher = publisher;
        _logger = logger;
    }

    private static readonly Dictionary<string, Expression<Func<AccommodationStay, object>>> FilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["workerId"] = x => x.WorkerId,
        ["room"] = x => x.Room!,
        ["departureReason"] = x => x.DepartureReason!,
    };

    private static readonly Dictionary<string, Expression<Func<AccommodationStay, object>>> SortableFields = new()
    {
        ["stayCode"] = x => x.StayCode,
        ["checkInDate"] = x => x.CheckInDate,
        ["checkOutDate"] = x => x.CheckOutDate!,
        ["createdAt"] = x => x.CreatedAt,
        ["status"] = x => x.Status,
    };

    public async Task<PagedList<AccommodationStayListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        query = query.ApplyFilters(qp.Filters, FilterableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x =>
                x.StayCode.ToLower().Contains(search) ||
                (x.Room != null && x.Room.ToLower().Contains(search)) ||
                (x.Location != null && x.Location.ToLower().Contains(search)));
        }

        query = query.ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<AccommodationStayDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (stay == null)
            return Result<AccommodationStayDto>.NotFound("Accommodation stay not found");

        return Result<AccommodationStayDto>.Success(MapToDto(stay));
    }

    public async Task<Result<AccommodationStayDto>> CheckInAsync(Guid tenantId, CheckInRequest request, CancellationToken ct = default)
    {
        // Check if worker is already checked in
        var alreadyCheckedIn = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.WorkerId == request.WorkerId
                && x.Status == AccommodationStayStatus.CheckedIn, ct);

        if (alreadyCheckedIn)
            return Result<AccommodationStayDto>.Conflict("Worker is already checked in to accommodation");

        // Generate stay code
        var maxCode = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StayCode)
            .Select(x => x.StayCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (maxCode != null && maxCode.StartsWith("STAY-") && int.TryParse(maxCode[5..], out var current))
        {
            nextNumber = current + 1;
        }

        var now = _clock.UtcNow;
        var stay = new AccommodationStay
        {
            TenantId = tenantId,
            StayCode = $"STAY-{nextNumber:D6}",
            Status = AccommodationStayStatus.CheckedIn,
            StatusChangedAt = now,
            WorkerId = request.WorkerId,
            PlacementId = request.PlacementId,
            ArrivalId = request.ArrivalId,
            CheckInDate = now,
            Room = request.Room,
            Location = request.Location,
            CheckedInBy = _currentUser.UserId.ToString(),
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<AccommodationStay>().Add(stay);
        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new AccommodationCheckInEvent
        {
            TenantId = tenantId,
            StayId = stay.Id,
            WorkerId = stay.WorkerId,
            PlacementId = stay.PlacementId,
            ArrivalId = stay.ArrivalId,
            Room = stay.Room,
            Location = stay.Location,
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("Check-in {Code} created for worker {WorkerId}", stay.StayCode, stay.WorkerId);

        return Result<AccommodationStayDto>.Success(MapToDto(stay));
    }

    public async Task<Result<AccommodationStayDto>> CheckOutAsync(Guid tenantId, Guid id, CheckOutRequest request, CancellationToken ct = default)
    {
        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (stay == null)
            return Result<AccommodationStayDto>.NotFound("Accommodation stay not found");

        if (stay.Status != AccommodationStayStatus.CheckedIn)
            return Result<AccommodationStayDto>.ValidationError("Can only check out from CheckedIn status");

        if (!Enum.TryParse<DepartureReason>(request.DepartureReason, ignoreCase: true, out var reason))
            return Result<AccommodationStayDto>.ValidationError($"Invalid departure reason '{request.DepartureReason}'");

        var now = _clock.UtcNow;
        stay.Status = AccommodationStayStatus.CheckedOut;
        stay.StatusChangedAt = now;
        stay.CheckOutDate = now;
        stay.DepartureReason = reason;
        stay.DepartureNotes = request.DepartureNotes;
        stay.CheckedOutBy = _currentUser.UserId.ToString();
        stay.UpdatedBy = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        await _publisher.Publish(new AccommodationCheckOutEvent
        {
            TenantId = tenantId,
            StayId = stay.Id,
            WorkerId = stay.WorkerId,
            DepartureReason = reason.ToString(),
            OccurredAt = now,
        }, ct);

        _logger.LogInformation("Check-out {Code} with reason {Reason}", stay.StayCode, reason);

        return Result<AccommodationStayDto>.Success(MapToDto(stay));
    }

    public async Task<Result<AccommodationStayDto>> UpdateAsync(Guid tenantId, Guid id, UpdateStayRequest request, CancellationToken ct = default)
    {
        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (stay == null)
            return Result<AccommodationStayDto>.NotFound("Accommodation stay not found");

        if (request.Room != null) stay.Room = request.Room;
        if (request.Location != null) stay.Location = request.Location;
        stay.UpdatedBy = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Stay {Code} updated", stay.StayCode);

        return Result<AccommodationStayDto>.Success(MapToDto(stay));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == id, ct);

        if (stay == null)
            return Result.NotFound("Accommodation stay not found");

        stay.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Stay {Code} deleted", stay.StayCode);
        return Result.Success();
    }

    public async Task<PagedList<AccommodationStayListDto>> GetCurrentOccupantsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == AccommodationStayStatus.CheckedIn);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x =>
                x.StayCode.ToLower().Contains(search) ||
                (x.Room != null && x.Room.ToLower().Contains(search)) ||
                (x.Location != null && x.Location.ToLower().Contains(search)));
        }

        query = query.ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<AccommodationStayListDto>> GetDailyListAsync(Guid tenantId, DateOnly date, QueryParameters qp, CancellationToken ct = default)
    {
        var dateStart = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dateEnd = dateStart.AddDays(1);

        var query = _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.CheckInDate < dateEnd
                && (x.CheckOutDate == null || x.CheckOutDate >= dateStart));

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x =>
                x.StayCode.ToLower().Contains(search) ||
                (x.Room != null && x.Room.ToLower().Contains(search)));
        }

        query = query.ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<AccommodationStayListDto>> GetStayHistoryByWorkerAsync(Guid tenantId, Guid workerId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.WorkerId == workerId)
            .OrderByDescending(x => x.CheckInDate);

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count(), ct);
    }

    // ── Mapping ──

    private static AccommodationStayDto MapToDto(AccommodationStay x)
    {
        return new AccommodationStayDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            StayCode = x.StayCode,
            Status = x.Status.ToString(),
            StatusChangedAt = x.StatusChangedAt,
            WorkerId = x.WorkerId,
            PlacementId = x.PlacementId,
            ArrivalId = x.ArrivalId,
            CheckInDate = x.CheckInDate,
            CheckOutDate = x.CheckOutDate,
            Room = x.Room,
            Location = x.Location,
            DepartureReason = x.DepartureReason?.ToString(),
            DepartureNotes = x.DepartureNotes,
            CheckedInBy = x.CheckedInBy,
            CheckedOutBy = x.CheckedOutBy,
            CreatedBy = x.CreatedBy,
            UpdatedBy = x.UpdatedBy,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        };
    }

    private static AccommodationStayListDto MapToListDto(AccommodationStay x)
    {
        return new AccommodationStayListDto
        {
            Id = x.Id,
            StayCode = x.StayCode,
            Status = x.Status.ToString(),
            WorkerId = x.WorkerId,
            CheckInDate = x.CheckInDate,
            CheckOutDate = x.CheckOutDate,
            Room = x.Room,
            Location = x.Location,
            DepartureReason = x.DepartureReason?.ToString(),
            CheckedInBy = x.CheckedInBy,
            CreatedAt = x.CreatedAt,
        };
    }
}
