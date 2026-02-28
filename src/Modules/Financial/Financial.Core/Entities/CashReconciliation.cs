using TadHub.SharedKernel.Entities;

namespace Financial.Core.Entities;

public class CashReconciliation : SoftDeletableEntity, IAuditable
{
    public DateOnly ReportDate { get; set; }
    public Guid? CashierId { get; set; }
    public string? CashierName { get; set; }

    // Per-method totals
    public decimal CashTotal { get; set; }
    public decimal CardTotal { get; set; }
    public decimal BankTransferTotal { get; set; }
    public decimal ChequeTotal { get; set; }
    public decimal EDirhamTotal { get; set; }
    public decimal OnlineTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public int TransactionCount { get; set; }
    public string? Notes { get; set; }
    public bool IsClosed { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
