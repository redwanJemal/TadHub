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
/// When a returnee case is approved:
/// 1. Auto-generates a CreditNote invoice for the customer refund (if refund amount > 0)
/// 2. Auto-generates supplier debits if within guarantee period
/// </summary>
public class ReturneeCaseApprovedConsumer : IConsumer<ReturneeCaseApprovedEvent>
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<ReturneeCaseApprovedConsumer> _logger;

    public ReturneeCaseApprovedConsumer(
        AppDbContext db,
        IClock clock,
        ILogger<ReturneeCaseApprovedConsumer> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReturneeCaseApprovedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation("Processing returnee case approval for case {CaseId}, worker {WorkerId}", evt.ReturneeCaseId, evt.WorkerId);

        // Auto-generate CreditNote if refund amount > 0
        if (evt.RefundAmount.HasValue && evt.RefundAmount > 0)
        {
            await GenerateCreditNoteAsync(evt, context.CancellationToken);
        }

        // Auto-generate supplier debits if within guarantee period
        if (evt.IsWithinGuarantee && evt.SupplierId.HasValue)
        {
            await GenerateSupplierDebitsAsync(evt, context.CancellationToken);
        }
    }

    private async Task GenerateCreditNoteAsync(ReturneeCaseApprovedEvent evt, CancellationToken ct)
    {
        // Find the original invoice for this contract
        var originalInvoice = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == evt.TenantId
                && x.ContractId == evt.ContractId
                && x.Type == InvoiceType.Standard
                && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        // Generate invoice number
        var lastInvNumber = await _db.Set<Invoice>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == evt.TenantId)
            .OrderByDescending(x => x.InvoiceNumber)
            .Select(x => x.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastInvNumber is not null)
        {
            var dashIndex = lastInvNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastInvNumber[(dashIndex + 1)..], out var parsed))
                nextNumber = parsed + 1;
        }

        var creditNote = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            InvoiceNumber = $"INV-{nextNumber:D6}",
            Type = InvoiceType.CreditNote,
            Status = InvoiceStatus.Issued,
            ContractId = evt.ContractId,
            ClientId = evt.ClientId,
            WorkerId = evt.WorkerId,
            OriginalInvoiceId = originalInvoice?.Id,
            CreditNoteReason = $"Returnee case refund (Case #{evt.ReturneeCaseId})",
            IssueDate = DateOnly.FromDateTime(_clock.UtcNow.DateTime),
            DueDate = DateOnly.FromDateTime(_clock.UtcNow.AddDays(30).DateTime),
            Subtotal = evt.RefundAmount!.Value,
            TotalAmount = evt.RefundAmount!.Value,
            BalanceDue = evt.RefundAmount!.Value,
            Currency = "AED",
        };

        _db.Set<Invoice>().Add(creditNote);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Generated credit note {InvoiceNumber} for returnee case {CaseId}, amount={Amount}",
            creditNote.InvoiceNumber, evt.ReturneeCaseId, evt.RefundAmount);
    }

    private async Task GenerateSupplierDebitsAsync(ReturneeCaseApprovedEvent evt, CancellationToken ct)
    {
        // Load settings
        var settingsJson = await _db.Database
            .SqlQueryRaw<string>(
                "SELECT settings_json AS \"Value\" FROM tenants WHERE id = {0} AND is_deleted = false LIMIT 1",
                evt.TenantId)
            .FirstOrDefaultAsync(ct);

        var guaranteeSettings = new GuaranteeSettings();
        if (!string.IsNullOrWhiteSpace(settingsJson))
        {
            try
            {
                var root = JsonNode.Parse(settingsJson);
                var financialNode = root?["financial"];
                if (financialNode is not null)
                {
                    var settings = financialNode.Deserialize<TenantFinancialSettings>();
                    if (settings?.GuaranteeSettings is not null)
                        guaranteeSettings = settings.GuaranteeSettings;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse financial settings for tenant {TenantId}", evt.TenantId);
            }
        }

        if (!guaranteeSettings.AutoGenerateDebits)
        {
            _logger.LogInformation("Auto-generate debits disabled for tenant {TenantId}, skipping", evt.TenantId);
            return;
        }

        // Generate debit number base
        var lastDebitNumber = await _db.Set<SupplierDebit>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == evt.TenantId)
            .OrderByDescending(x => x.DebitNumber)
            .Select(x => x.DebitNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastDebitNumber is not null)
        {
            var dashIndex = lastDebitNumber.LastIndexOf('-');
            if (dashIndex >= 0 && int.TryParse(lastDebitNumber[(dashIndex + 1)..], out var parsed))
                nextNumber = parsed + 1;
        }

        // Get placement costs via raw SQL to determine amounts for debits
        var placementCosts = await _db.Database
            .SqlQueryRaw<PlacementCostRaw>(
                "SELECT cost_type AS \"CostType\", COALESCE(SUM(amount), 0)::numeric AS \"Amount\" " +
                "FROM placement_cost_items pci " +
                "JOIN placements p ON p.id = pci.placement_id " +
                "WHERE p.tenant_id = {0} AND pci.tenant_id = {0} " +
                "AND p.is_deleted = false AND pci.is_deleted = false " +
                "AND EXISTS (SELECT 1 FROM contracts c WHERE c.placement_id = p.id AND c.id = {1} AND c.tenant_id = {0} AND c.is_deleted = false) " +
                "GROUP BY cost_type",
                evt.TenantId, evt.ContractId)
            .ToListAsync(ct);

        var debitsToCreate = new List<SupplierDebit>();

        // Always add commission refund debit
        var commissionPayment = await _db.Set<SupplierPayment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == evt.TenantId
                && x.SupplierId == evt.SupplierId!.Value
                && x.PaymentType == SupplierPaymentType.Commission
                && x.ContractId == evt.ContractId
                && !x.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (commissionPayment is not null)
        {
            debitsToCreate.Add(new SupplierDebit
            {
                Id = Guid.NewGuid(),
                TenantId = evt.TenantId,
                DebitNumber = $"SDBT-{nextNumber++:D6}",
                Status = SupplierDebitStatus.Outstanding,
                SupplierId = evt.SupplierId!.Value,
                WorkerId = evt.WorkerId,
                ContractId = evt.ContractId,
                CaseType = SupplierDebitCaseType.Returnee,
                CaseId = evt.ReturneeCaseId,
                DebitType = SupplierDebitType.CommissionRefund,
                Description = "Commission refund for returnee within guarantee",
                Amount = commissionPayment.Amount,
                Currency = "AED",
            });
        }

        // Add ticket cost debit (from Flight cost)
        var flightCost = placementCosts.FirstOrDefault(c => c.CostType == "Flight");
        if (flightCost is not null && flightCost.Amount > 0)
        {
            debitsToCreate.Add(new SupplierDebit
            {
                Id = Guid.NewGuid(),
                TenantId = evt.TenantId,
                DebitNumber = $"SDBT-{nextNumber++:D6}",
                Status = SupplierDebitStatus.Outstanding,
                SupplierId = evt.SupplierId!.Value,
                WorkerId = evt.WorkerId,
                ContractId = evt.ContractId,
                CaseType = SupplierDebitCaseType.Returnee,
                CaseId = evt.ReturneeCaseId,
                DebitType = SupplierDebitType.TicketCost,
                Description = "Ticket cost recovery for returnee within guarantee",
                Amount = flightCost.Amount,
                Currency = "AED",
            });
        }

        // Optional cost debits based on settings
        if (guaranteeSettings.IncludeVisaCost)
        {
            var visaCost = placementCosts.FirstOrDefault(c => c.CostType == "Visa");
            if (visaCost is not null && visaCost.Amount > 0)
            {
                debitsToCreate.Add(CreateCostDebit(evt, ref nextNumber, SupplierDebitType.VisaCost, "Visa cost recovery", visaCost.Amount));
            }
        }

        if (guaranteeSettings.IncludeTransportationCost)
        {
            var transportCost = placementCosts.FirstOrDefault(c => c.CostType == "Accommodation");
            if (transportCost is not null && transportCost.Amount > 0)
            {
                debitsToCreate.Add(CreateCostDebit(evt, ref nextNumber, SupplierDebitType.TransportationCost, "Transportation cost recovery", transportCost.Amount));
            }
        }

        if (guaranteeSettings.IncludeMedicalCost)
        {
            var medicalCost = placementCosts.FirstOrDefault(c => c.CostType == "Medical");
            if (medicalCost is not null && medicalCost.Amount > 0)
            {
                debitsToCreate.Add(CreateCostDebit(evt, ref nextNumber, SupplierDebitType.MedicalCost, "Medical cost recovery", medicalCost.Amount));
            }
        }

        if (debitsToCreate.Count > 0)
        {
            _db.Set<SupplierDebit>().AddRange(debitsToCreate);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Generated {Count} supplier debits for returnee case {CaseId}, supplier {SupplierId}",
                debitsToCreate.Count, evt.ReturneeCaseId, evt.SupplierId);
        }
    }

    private static SupplierDebit CreateCostDebit(
        ReturneeCaseApprovedEvent evt,
        ref int nextNumber,
        SupplierDebitType debitType,
        string description,
        decimal amount)
    {
        return new SupplierDebit
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            DebitNumber = $"SDBT-{nextNumber++:D6}",
            Status = SupplierDebitStatus.Outstanding,
            SupplierId = evt.SupplierId!.Value,
            WorkerId = evt.WorkerId,
            ContractId = evt.ContractId,
            CaseType = SupplierDebitCaseType.Returnee,
            CaseId = evt.ReturneeCaseId,
            DebitType = debitType,
            Description = description,
            Amount = amount,
            Currency = "AED",
        };
    }

    private sealed class PlacementCostRaw
    {
        public string CostType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
