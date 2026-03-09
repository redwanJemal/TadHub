using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Placement.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Placement.Core.Consumers;

/// <summary>
/// Handles trial completion for inside-country placements.
/// If trial succeeds → advance to TrialSuccessful.
/// If trial fails → cancel placement and return worker to inventory.
/// </summary>
public class PlacementTrialCompletedConsumer : IConsumer<TrialCompletedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<PlacementTrialCompletedConsumer> _logger;

    public PlacementTrialCompletedConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<PlacementTrialCompletedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrialCompletedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Find the placement linked to this trial
        Entities.Placement? placement = null;

        if (message.PlacementId.HasValue)
        {
            placement = await _db.Set<Entities.Placement>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == message.PlacementId.Value
                    && x.TenantId == message.TenantId
                    && !x.IsDeleted, ct);
        }

        // Also try finding by trial ID
        placement ??= await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TrialId == message.TrialId
                && x.TenantId == message.TenantId
                && !x.IsDeleted, ct);

        if (placement is null)
        {
            _logger.LogInformation("No placement found for trial {TrialId} — ignoring", message.TrialId);
            return;
        }

        // Only process if placement is in InTrial status
        if (placement.Status != PlacementStatus.InTrial)
        {
            _logger.LogInformation("Placement {PlacementId} is in {Status}, not InTrial — ignoring trial completion",
                placement.Id, placement.Status);
            return;
        }

        var now = _clock.UtcNow;

        // Link trial if not already linked
        if (placement.TrialId is null)
            placement.TrialId = message.TrialId;

        if (message.Outcome == "ProceedToContract")
        {
            // Trial successful → advance to TrialSuccessful
            placement.Status = PlacementStatus.TrialSuccessful;
            placement.StatusChangedAt = now;
            placement.TrialSucceededAt = now;

            _db.Set<PlacementStatusHistory>().Add(new PlacementStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = placement.TenantId,
                PlacementId = placement.Id,
                FromStatus = PlacementStatus.InTrial,
                ToStatus = PlacementStatus.TrialSuccessful,
                ChangedAt = now,
                ChangedBy = "system",
                Notes = $"Trial completed successfully. {message.OutcomeNotes}".Trim(),
            });

            await _db.SaveChangesAsync(ct);

            await _publisher.Publish(new PlacementStatusChangedEvent
            {
                OccurredAt = now,
                TenantId = placement.TenantId,
                PlacementId = placement.Id,
                CandidateId = placement.CandidateId,
                WorkerId = placement.WorkerId,
                FromStatus = PlacementStatus.InTrial.ToString(),
                ToStatus = PlacementStatus.TrialSuccessful.ToString(),
            }, ct);

            _logger.LogInformation("Trial {TrialId} succeeded — placement {PlacementId} advanced to TrialSuccessful",
                message.TrialId, placement.Id);
        }
        else
        {
            // Trial failed → cancel placement
            placement.Status = PlacementStatus.Cancelled;
            placement.StatusChangedAt = now;
            placement.CancelledAt = now;
            placement.CancellationReason = $"Trial failed: {message.OutcomeNotes ?? "returned to inventory"}";

            _db.Set<PlacementStatusHistory>().Add(new PlacementStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = placement.TenantId,
                PlacementId = placement.Id,
                FromStatus = PlacementStatus.InTrial,
                ToStatus = PlacementStatus.Cancelled,
                ChangedAt = now,
                ChangedBy = "system",
                Reason = placement.CancellationReason,
                Notes = "Trial failed — worker returned to inventory",
            });

            await _db.SaveChangesAsync(ct);

            await _publisher.Publish(new PlacementStatusChangedEvent
            {
                OccurredAt = now,
                TenantId = placement.TenantId,
                PlacementId = placement.Id,
                CandidateId = placement.CandidateId,
                WorkerId = placement.WorkerId,
                FromStatus = PlacementStatus.InTrial.ToString(),
                ToStatus = PlacementStatus.Cancelled.ToString(),
                Reason = placement.CancellationReason,
            }, ct);

            _logger.LogInformation("Trial {TrialId} failed — placement {PlacementId} cancelled",
                message.TrialId, placement.Id);
        }
    }
}
