using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Contract.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Contract.Core.Consumers;

public class TrialCompletedConsumer : IConsumer<TrialCompletedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<TrialCompletedConsumer> _logger;

    public TrialCompletedConsumer(
        AppDbContext db,
        IClock clock,
        IPublishEndpoint publisher,
        ILogger<TrialCompletedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrialCompletedEvent> context)
    {
        var evt = context.Message;

        // Only auto-create contract when trial outcome is ProceedToContract
        if (!string.Equals(evt.Outcome, "ProceedToContract", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Trial {TrialId} completed with outcome {Outcome}, skipping contract auto-creation",
                evt.TrialId, evt.Outcome);
            return;
        }

        // Check no existing active contract for this worker
        var hasActiveContract = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == evt.TenantId
                && !x.IsDeleted
                && x.WorkerId == evt.WorkerId
                && x.Status != ContractStatus.Closed
                && x.Status != ContractStatus.Cancelled
                && x.Status != ContractStatus.Terminated);

        if (hasActiveContract)
        {
            _logger.LogWarning("Worker {WorkerId} already has an active contract, skipping auto-creation from trial {TrialId}",
                evt.WorkerId, evt.TrialId);
            return;
        }

        // Generate contract code
        var lastCode = await _db.Set<Entities.Contract>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == evt.TenantId)
            .OrderByDescending(x => x.ContractCode)
            .Select(x => x.ContractCode)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("CTR-") && int.TryParse(lastCode[4..], out var lastNumber))
            nextNumber = lastNumber + 1;

        var contractCode = $"CTR-{nextNumber:D6}";
        var now = _clock.UtcNow;
        var startDate = DateOnly.FromDateTime(now.DateTime);

        var contract = new Entities.Contract
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            ContractCode = contractCode,
            Type = ContractType.TwoYearEmployment,
            Status = ContractStatus.Draft,
            StatusChangedAt = now,
            WorkerId = evt.WorkerId,
            ClientId = evt.ClientId,
            StartDate = startDate,
            EndDate = startDate.AddYears(2),
            GuaranteePeriodType = GuaranteePeriod.OneYear,
            GuaranteeEndDate = startDate.AddYears(1),
            Rate = 0, // To be filled in by staff
            RatePeriod = RatePeriod.Monthly,
            Currency = "AED",
            Notes = $"Auto-created from successful trial (Trial ID: {evt.TrialId})",
        };

        var history = new ContractStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            ContractId = contract.Id,
            FromStatus = null,
            ToStatus = ContractStatus.Draft,
            ChangedAt = now,
            Notes = "Auto-created from successful trial",
        };

        _db.Set<Entities.Contract>().Add(contract);
        _db.Set<ContractStatusHistory>().Add(history);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Auto-created contract {ContractCode} for worker {WorkerId} from successful trial {TrialId}",
            contractCode, evt.WorkerId, evt.TrialId);

        await _publisher.Publish(new ContractStatusChangedEvent
        {
            OccurredAt = now,
            TenantId = evt.TenantId,
            ContractId = contract.Id,
            WorkerId = evt.WorkerId,
            FromStatus = string.Empty,
            ToStatus = ContractStatus.Draft.ToString(),
        });
    }
}
