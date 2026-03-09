using Accommodation.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Accommodation.Core.Consumers;

public class MaidAtAccommodationConsumer : IConsumer<MaidAtAccommodationEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<MaidAtAccommodationConsumer> _logger;

    public MaidAtAccommodationConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<MaidAtAccommodationConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MaidAtAccommodationEvent> context)
    {
        var evt = context.Message;

        // Check if worker is already checked in
        var alreadyCheckedIn = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == evt.TenantId
                && !x.IsDeleted
                && x.WorkerId == evt.WorkerId
                && x.Status == AccommodationStayStatus.CheckedIn);

        if (alreadyCheckedIn)
        {
            _logger.LogInformation("Worker {WorkerId} already checked in, skipping auto check-in from arrival {ArrivalId}",
                evt.WorkerId, evt.ArrivalId);
            return;
        }

        // Generate stay code
        var maxCode = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == evt.TenantId)
            .OrderByDescending(x => x.StayCode)
            .Select(x => x.StayCode)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (maxCode != null && maxCode.StartsWith("STAY-") && int.TryParse(maxCode[5..], out var current))
        {
            nextNumber = current + 1;
        }

        var now = _clock.UtcNow;
        var stay = new AccommodationStay
        {
            TenantId = evt.TenantId,
            StayCode = $"STAY-{nextNumber:D6}",
            Status = AccommodationStayStatus.CheckedIn,
            StatusChangedAt = now,
            WorkerId = evt.WorkerId,
            PlacementId = evt.PlacementId,
            ArrivalId = evt.ArrivalId,
            CheckInDate = now,
            CheckedInBy = "System (Arrival)",
        };

        _db.Set<AccommodationStay>().Add(stay);
        await _db.SaveChangesAsync();

        await _publisher.Publish(new AccommodationCheckInEvent
        {
            TenantId = evt.TenantId,
            StayId = stay.Id,
            WorkerId = stay.WorkerId,
            PlacementId = stay.PlacementId,
            ArrivalId = stay.ArrivalId,
            OccurredAt = now,
        });

        _logger.LogInformation("Auto check-in {Code} created for worker {WorkerId} from arrival {ArrivalId}",
            stay.StayCode, evt.WorkerId, evt.ArrivalId);
    }
}
