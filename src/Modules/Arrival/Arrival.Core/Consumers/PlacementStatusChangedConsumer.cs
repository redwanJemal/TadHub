using Arrival.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Arrival.Core.Consumers;

public class PlacementStatusChangedConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<PlacementStatusChangedConsumer> _logger;

    public PlacementStatusChangedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<PlacementStatusChangedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementStatusChangedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Only react to TicketArranged status
        if (message.ToStatus != "TicketArranged")
            return;

        // Check if arrival already exists for this placement
        var existingArrival = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == message.TenantId
                && !x.IsDeleted
                && x.PlacementId == message.PlacementId
                && x.Status != ArrivalStatus.NoShow
                && x.Status != ArrivalStatus.Cancelled, ct);

        if (existingArrival)
        {
            _logger.LogInformation("Arrival already exists for placement {PlacementId}, skipping auto-creation", message.PlacementId);
            return;
        }

        // Get placement flight details via raw SQL to avoid cross-module dependency
        var placements = await _db.Database.SqlQueryRaw<PlacementFlightInfo>(
            @"SELECT flight_details AS ""FlightDetails"", expected_arrival_date AS ""ExpectedArrivalDate"",
                     candidate_id AS ""CandidateId""
              FROM placements
              WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
            message.PlacementId, message.TenantId)
            .ToListAsync(ct);

        var placement = placements.FirstOrDefault();
        if (placement == null)
        {
            _logger.LogWarning("Placement {PlacementId} not found for auto-creating arrival", message.PlacementId);
            return;
        }

        // Get supplier ID from candidate via raw SQL
        Guid? supplierId = null;
        var candidates = await _db.Database.SqlQueryRaw<CandidateSupplierInfo>(
            @"SELECT supplier_id AS ""SupplierId""
              FROM candidates
              WHERE id = {0} AND tenant_id = {1} AND is_deleted = false",
            message.CandidateId, message.TenantId)
            .ToListAsync(ct);

        var candidate = candidates.FirstOrDefault();
        if (candidate != null)
        {
            supplierId = candidate.SupplierId;
        }

        var now = _clock.UtcNow;
        var arrivalDate = placement.ExpectedArrivalDate ?? DateOnly.FromDateTime(now.DateTime.AddDays(7));

        // Generate arrival code
        var maxCode = await _db.Set<Entities.Arrival>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == message.TenantId)
            .OrderByDescending(x => x.ArrivalCode)
            .Select(x => x.ArrivalCode)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (maxCode != null && maxCode.StartsWith("ARR-") && int.TryParse(maxCode[4..], out var current))
        {
            nextNumber = current + 1;
        }

        var workerId = message.WorkerId ?? Guid.Empty;

        var arrival = new Entities.Arrival
        {
            TenantId = message.TenantId,
            ArrivalCode = $"ARR-{nextNumber:D6}",
            Status = ArrivalStatus.Scheduled,
            StatusChangedAt = now,
            WorkerId = workerId,
            PlacementId = message.PlacementId,
            SupplierId = supplierId,
            FlightNumber = placement.FlightDetails,
            ScheduledArrivalDate = arrivalDate,
            Notes = "Auto-created from placement ticket arrangement",
        };

        arrival.StatusHistory.Add(new ArrivalStatusHistory
        {
            TenantId = message.TenantId,
            FromStatus = null,
            ToStatus = ArrivalStatus.Scheduled,
            ChangedAt = now,
            Notes = "Auto-scheduled from placement ticket arrangement",
        });

        _db.Set<Entities.Arrival>().Add(arrival);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Auto-created arrival {Code} for placement {PlacementId}", arrival.ArrivalCode, message.PlacementId);
    }

    private sealed record PlacementFlightInfo
    {
        public string? FlightDetails { get; init; }
        public DateOnly? ExpectedArrivalDate { get; init; }
        public Guid CandidateId { get; init; }
    }

    private sealed record CandidateSupplierInfo
    {
        public Guid? SupplierId { get; init; }
    }
}
