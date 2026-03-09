namespace TadHub.SharedKernel.Events;

public sealed record PaymentReceivedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid? InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;

    public PaymentReceivedEvent() { }
}

public sealed record RefundProcessedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid RefundId { get; init; }
    public Guid? InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? Reason { get; init; }

    public RefundProcessedEvent() { }
}

public sealed record SupplierCommissionDueEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid SupplierId { get; init; }
    public string SupplierNameEn { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public SupplierCommissionDueEvent() { }
}

public sealed record OverduePaymentEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int DaysOverdue { get; init; }

    public OverduePaymentEvent() { }
}
