namespace TadHub.SharedKernel.Events.Tadbeer.Financial;

/// <summary>
/// Published when an invoice is generated.
/// </summary>
public record InvoiceGeneratedEvent : TadbeerEventBase
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string InvoiceType { get; init; } = string.Empty; // Proforma, TaxInvoice
    public Guid? ContractId { get; init; }
    public Guid ClientId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal VatAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTimeOffset DueDate { get; init; }
}

/// <summary>
/// Published when a payment is received.
/// </summary>
public record PaymentReceivedEvent : TadbeerEventBase
{
    public Guid PaymentId { get; init; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty; // Cash, Card, BankTransfer, Cheque, EDirham
    public string? ReferenceNumber { get; init; }
    public Guid ProcessedByUserId { get; init; }
}

/// <summary>
/// Published when a payment becomes overdue.
/// </summary>
public record PaymentOverdueEvent : TadbeerEventBase
{
    public Guid InvoiceId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? ContractId { get; init; }
    public decimal AmountDue { get; init; }
    public int DaysOverdue { get; init; }
}

/// <summary>
/// Published when a refund is processed.
/// </summary>
public record RefundProcessedEvent : TadbeerEventBase
{
    public Guid RefundId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
    public Guid ProcessedByUserId { get; init; }
}

/// <summary>
/// Published when a credit note is issued.
/// </summary>
public record CreditNoteIssuedEvent : TadbeerEventBase
{
    public Guid CreditNoteId { get; init; }
    public string CreditNoteNumber { get; init; } = string.Empty;
    public Guid OriginalInvoiceId { get; init; }
    public Guid ClientId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
