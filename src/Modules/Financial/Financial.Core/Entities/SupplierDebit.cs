using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class SupplierDebit : SoftDeletableEntity, IAuditable
{
    public string DebitNumber { get; set; } = string.Empty;
    public SupplierDebitStatus Status { get; set; } = SupplierDebitStatus.Outstanding;

    // References (no FK)
    public Guid SupplierId { get; set; }
    public Guid? WorkerId { get; set; }
    public Guid? ContractId { get; set; }

    // Case reference
    public SupplierDebitCaseType? CaseType { get; set; }
    public Guid? CaseId { get; set; }

    // Debit details
    public SupplierDebitType DebitType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";

    // Settlement
    public Guid? SettlementPaymentId { get; set; }
    public DateTimeOffset? SettledAt { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public enum SupplierDebitStatus
{
    Outstanding,
    PartiallyPaid,
    Settled,
    Waived,
    Cancelled,
}

public enum SupplierDebitType
{
    CommissionRefund,
    TicketCost,
    VisaCost,
    TransportationCost,
    MedicalCost,
    AccommodationCost,
    Other,
}

public enum SupplierDebitCaseType
{
    Returnee,
    Runaway,
}
