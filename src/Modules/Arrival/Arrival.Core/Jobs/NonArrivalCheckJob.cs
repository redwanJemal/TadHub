using Arrival.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Arrival.Core.Jobs;

/// <summary>
/// Hourly Hangfire job that checks for overdue arrivals (scheduled arrival date has passed
/// and status is still Scheduled or InTransit), and publishes MaidNoShowEvent notifications.
/// </summary>
public class NonArrivalCheckJob
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IClock _clock;
    private readonly ILogger<NonArrivalCheckJob> _logger;

    public NonArrivalCheckJob(
        AppDbContext db,
        IPublishEndpoint publisher,
        IClock clock,
        ILogger<NonArrivalCheckJob> logger)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.DateTime);
        var now = _clock.UtcNow;

        _logger.LogInformation("Running non-arrival check for {Date}", today);

        // Find arrivals that are overdue: scheduled date has passed and still Scheduled or InTransit
        var overdueArrivals = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted
                && (x.Status == ArrivalStatus.Scheduled || x.Status == ArrivalStatus.InTransit)
                && x.ScheduledArrivalDate < today)
            .ToListAsync(ct);

        foreach (var arrival in overdueArrivals)
        {
            await _publisher.Publish(new MaidNoShowEvent
            {
                TenantId = arrival.TenantId,
                ArrivalId = arrival.Id,
                PlacementId = arrival.PlacementId,
                WorkerId = arrival.WorkerId,
                SupplierId = arrival.SupplierId,
                ScheduledArrivalDate = arrival.ScheduledArrivalDate,
                OccurredAt = now,
            }, ct);
        }

        _logger.LogInformation(
            "Non-arrival check complete: {OverdueCount} overdue arrivals detected",
            overdueArrivals.Count);
    }
}
