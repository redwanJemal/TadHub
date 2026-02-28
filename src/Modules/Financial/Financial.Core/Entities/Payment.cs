using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class Payment : SoftDeletableEntity, IAuditable
{
    public string PaymentNumber { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // References
    public Guid InvoiceId { get; set; }
    public Guid ClientId { get; set; }

    // Amount
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public DateOnly PaymentDate { get; set; }

    // Gateway future-proofing
    public string? GatewayProvider { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayStatus { get; set; }
    public string? GatewayResponseJson { get; set; }

    // Refund
    public Guid? RefundedPaymentId { get; set; }
    public decimal? RefundAmount { get; set; }

    // Cashier (for X-Report)
    public Guid? CashierId { get; set; }
    public string? CashierName { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
}
