using Accommodation.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Accommodation.Core.Consumers;

public class ReturneeCaseApprovedConsumer : IConsumer<ReturneeCaseApprovedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<ReturneeCaseApprovedConsumer> _logger;

    public ReturneeCaseApprovedConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<ReturneeCaseApprovedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReturneeCaseApprovedEvent> context)
    {
        var evt = context.Message;

        var stay = await _db.Set<AccommodationStay>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == evt.TenantId
                && !x.IsDeleted
                && x.WorkerId == evt.WorkerId
                && x.Status == AccommodationStayStatus.CheckedIn);

        if (stay == null) return;

        var now = _clock.UtcNow;
        stay.Status = AccommodationStayStatus.CheckedOut;
        stay.StatusChangedAt = now;
        stay.CheckOutDate = now;
        stay.DepartureReason = DepartureReason.ReturnedToCountry;
        stay.DepartureNotes = $"Auto-checked out: returnee approved (CaseId: {evt.ReturneeCaseId})";
        stay.CheckedOutBy = "System (Returnee)";

        await _db.SaveChangesAsync();

        await _publisher.Publish(new AccommodationCheckOutEvent
        {
            TenantId = evt.TenantId,
            StayId = stay.Id,
            WorkerId = stay.WorkerId,
            DepartureReason = DepartureReason.ReturnedToCountry.ToString(),
            OccurredAt = now,
        });

        _logger.LogInformation("Auto check-out {Code} for worker {WorkerId} due to returnee approved",
            stay.StayCode, evt.WorkerId);
    }
}
