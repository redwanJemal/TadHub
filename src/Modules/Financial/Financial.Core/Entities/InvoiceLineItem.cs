using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class InvoiceLineItem : TenantScopedEntity
{
    public Guid InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? ItemCode { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
}
