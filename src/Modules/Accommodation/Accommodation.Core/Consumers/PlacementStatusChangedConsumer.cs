using Accommodation.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Accommodation.Core.Consumers;

public class PlacementStatusChangedConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<PlacementStatusChangedConsumer> _logger;

    public PlacementStatusChangedConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<PlacementStatusChangedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementStatusChangedEvent> context)
    {
        var evt = context.Message;

        // When placement is "Placed" (deployed to customer), auto-checkout
        if (evt.ToStatus != "Placed") return;

        var workerId = evt.WorkerId;
        if (workerId == null || workerId == Guid.Empty) return;

        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == evt.TenantId
                && !x.IsDeleted
                && x.WorkerId == workerId
                && x.Status == AccommodationStayStatus.CheckedIn);

        if (stay == null) return;

        var now = _clock.UtcNow;
        stay.Status = AccommodationStayStatus.CheckedOut;
        stay.StatusChangedAt = now;
        stay.CheckOutDate = now;
        stay.DepartureReason = DepartureReason.DeployedToCustomer;
        stay.DepartureNotes = $"Auto-checked out: placement deployed (PlacementId: {evt.PlacementId})";
        stay.CheckedOutBy = "System (Placement)";

        await _db.SaveChangesAsync();

        await _publisher.Publish(new AccommodationCheckOutEvent
        {
            TenantId = evt.TenantId,
            StayId = stay.Id,
            WorkerId = stay.WorkerId,
            DepartureReason = DepartureReason.DeployedToCustomer.ToString(),
            OccurredAt = now,
        });

        _logger.LogInformation("Auto check-out {Code} for worker {WorkerId} due to placement deployed",
            stay.StayCode, workerId);
    }
}
