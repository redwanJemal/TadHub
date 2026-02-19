namespace TadHub.SharedKernel.Events.Tadbeer.Client;

/// <summary>
/// Published when a new client (employer) is registered.
/// </summary>
public record ClientRegisteredEvent : TadbeerEventBase
{
    public Guid ClientId { get; init; }
    public string EmiratesId { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string FullNameAr { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty; // Local, Expat, Investor, VIP
    public Guid RegisteredByUserId { get; init; }
}

/// <summary>
/// Published when a client's documents are verified.
/// This event unblocks contract creation for the client.
/// </summary>
public record ClientVerifiedEvent : TadbeerEventBase
{
    public Guid ClientId { get; init; }
    public Guid VerifiedByUserId { get; init; }
}

/// <summary>
/// Published when a client is blocked.
/// All pending contracts for this client should be paused.
/// </summary>
public record ClientBlockedEvent : TadbeerEventBase
{
    public Guid ClientId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid BlockedByUserId { get; init; }
}
