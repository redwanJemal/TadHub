using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Placement.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Placement.Core.Consumers;

/// <summary>
/// Updates placement status when its associated contract is activated.
/// </summary>
public class PlacementContractStatusChangedConsumer : IConsumer<ContractStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<PlacementContractStatusChangedConsumer> _logger;

    public PlacementContractStatusChangedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<PlacementContractStatusChangedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContractStatusChangedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Only care about contract activation (Confirmed or Active)
        if (message.ToStatus is not ("Confirmed" or "Active"))
            return;

        // Find placement linked to this contract
        var placement = await _db.Set<Entities.Placement>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == message.TenantId
                && !x.IsDeleted
                && x.ContractId == message.ContractId, ct);

        if (placement is null)
            return;

        _logger.LogInformation(
            "Contract {ContractId} activated — placement {PlacementId} noted",
            message.ContractId, placement.Id);
    }
}
