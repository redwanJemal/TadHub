namespace TadHub.SharedKernel.Events;

/// <summary>
/// Raised when a supplier is created and linked to a tenant.
/// </summary>
public sealed record SupplierCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid SupplierId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}
