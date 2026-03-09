using System.Text.Json;
using System.Text.Json.Nodes;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Financial.Contracts.Settings;
using Financial.Core.Entities;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;
using TadHub.SharedKernel.Interfaces;

namespace Financial.Core.Consumers;

/// <summary>
/// Auto-creates a supplier commission payment when a placement reaches "Placed" status.
/// </summary>
public class PlacementDeployedCommissionConsumer : IConsumer<PlacementStatusChangedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<PlacementDeployedCommissionConsumer> _logger;

    public PlacementDeployedCommissionConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<PlacementDeployedCommissionConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlacementStatusChangedEvent> context)
    {
        var evt = context.Message;

        if (evt.ToStatus != "Placed")
            return;

        _logger.LogInformation("Processing commission for placement {PlacementId} (status → Placed)", evt.PlacementId);

        // Load tenant financial settings via raw SQL
        var settingsJson = await _db.Database
            .SqlQueryRaw<string>(
                "SELECT settings_json AS \"Value\" FROM tenants WHERE id = {0} AND is_deleted = false LIMIT 1",
                evt.TenantId)
            .FirstOrDefaultAsync(context.CancellationToken);

        var commissionSettings = new CommissionSettings();
        if (!string.IsNullOrWhiteSpace(settingsJson))
        {
            try
            {
                var root = JsonNode.Parse(settingsJson);
                var financialNode = root?["financial"];
                if (financialNode is not null)
                {
                    var settings = financialNode.Deserialize<TenantFinancialSettings>();
                    if (settings?.CommissionSettings is not null)
                        commissionSettings = settings.CommissionSettings;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse tenant financial settings for tenant {TenantId}", evt.TenantId);
            }
        }

        if (!commissionSettings.AutoCalculateOnDeployment)
        {
            _logger.LogInformation("Auto-commission disabled for tenant {TenantId}, skipping", evt.TenantId);
            return;
        }

        // Get supplier ID from candidate via raw SQL
        var supplierId = await _db.Database
            .SqlQueryRaw<Guid?>(
                "SELECT supplier_id AS \"Value\" FROM candidates WHERE id = {0} AND tenant_id = {1} AND is_deleted = false LIMIT 1",
                evt.CandidateId, evt.TenantId)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (supplierId is null || supplierId == Guid.Empty)
        {
            _logger.LogInformation("No supplier for candidate {CandidateId}, skipping commission", evt.CandidateId);
            return;
        }

        // Check if commission already exists for this placement
        var existingCommission = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == evt.TenantId
                && x.PlacementId == evt.PlacementId
                && x.PaymentType == SupplierPaymentType.Commission
                && !x.IsDeleted,
                context.CancellationToken);

        if (existingCommission)
        {
            _logger.LogInformation("Commission already exists for placement {PlacementId}, skipping", evt.PlacementId);
            return;
        }

        // Calculate commission amount
        decimal commissionAmount;
        switch (commissionSettings.CalculationType)
        {
            case "Percentage":
                var contractValue = await _db.Database
                    .SqlQueryRaw<decimal>(
                        "SELECT COALESCE(total_amount, 0)::numeric AS \"Value\" FROM contracts WHERE placement_id = {0} AND tenant_id = {1} AND is_deleted = false LIMIT 1",
                        evt.PlacementId, evt.TenantId)
                    .FirstOrDefaultAsync(context.CancellationToken);
                commissionAmount = Math.Round(contractValue * commissionSettings.Percentage / 100m, 2);
                break;
            default: // Fixed or Custom
                commissionAmount = commissionSettings.FixedAmount;
                break;
        }

        if (commissionAmount <= 0)
        {
            _logger.LogInformation("Calculated commission is zero for placement {PlacementId}, skipping", evt.PlacementId);
            return;
        }

        // Generate payment number
        var lastNumber = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == evt.TenantId)
            .OrderByDescending(x => x.PaymentNumber)
            .Select(x => x.PaymentNumber)
            .FirstOrDefaultAsync(context.CancellationToken);

        var nextNumber = 1;
        if (lastNumber is not null)
        {
            var dashIndex = lastNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastNumber[(dashIndex + 1)..], out var parsed))
                nextNumber = parsed + 1;
        }

        var payment = new SupplierPayment
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            PaymentNumber = $"SPAY-{nextNumber:D6}",
            Status = SupplierPaymentStatus.Pending,
            PaymentType = SupplierPaymentType.Commission,
            SupplierId = supplierId.Value,
            WorkerId = evt.WorkerId,
            PlacementId = evt.PlacementId,
            Amount = commissionAmount,
            Currency = "AED",
            Method = PaymentMethod.BankTransfer,
            PaymentDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime),
            Notes = $"Auto-calculated commission ({commissionSettings.CalculationType}) on deployment",
        };

        _db.Set<SupplierPayment>().Add(payment);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created commission payment {PaymentNumber} for supplier {SupplierId}, amount={Amount} (placement {PlacementId})",
            payment.PaymentNumber, supplierId, commissionAmount, evt.PlacementId);
    }
}
