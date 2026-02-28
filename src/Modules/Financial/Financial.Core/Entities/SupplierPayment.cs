using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class SupplierPayment : SoftDeletableEntity, IAuditable
{
    public string PaymentNumber { get; set; } = string.Empty;
    public SupplierPaymentStatus Status { get; set; } = SupplierPaymentStatus.Pending;

    // References (no FK)
    public Guid SupplierId { get; set; }
    public Guid? WorkerId { get; set; }
    public Guid? ContractId { get; set; }

    // Amount
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? ReferenceNumber { get; set; }
    public DateOnly PaymentDate { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
