using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class Invoice : SoftDeletableEntity, IAuditable
{
    public string InvoiceNumber { get; set; } = string.Empty;

    // Type & Status
    public InvoiceType Type { get; set; } = InvoiceType.Standard;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTimeOffset? StatusChangedAt { get; set; }

    // Cross-module references (no FK)
    public Guid ContractId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? WorkerId { get; set; }

    // Dates
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }

    // Financial amounts
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }

    // Currency & Tax info
    public string Currency { get; set; } = "AED";
    public string? TenantTrn { get; set; }
    public string? ClientTrn { get; set; }

    // Discount
    public Guid? DiscountProgramId { get; set; }
    public string? DiscountProgramName { get; set; }
    public string? DiscountCardNumber { get; set; }
    public decimal? DiscountPercentage { get; set; }

    // Milestone
    public MilestoneType? MilestoneType { get; set; }

    // Credit note
    public Guid? OriginalInvoiceId { get; set; }
    public string? CreditNoteReason { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
