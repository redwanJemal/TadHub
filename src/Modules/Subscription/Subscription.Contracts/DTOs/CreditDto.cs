namespace Subscription.Contracts.DTOs;

/// <summary>
/// DTO for credit transaction data.
/// </summary>
public record CreditDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Type { get; init; } = string.Empty;
    public long Amount { get; init; }
    public long Balance { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? UserId { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Credit balance summary.
/// </summary>
public record CreditBalanceDto
{
    public Guid TenantId { get; init; }
    public long Balance { get; init; }
    public long ExpiringWithin30Days { get; init; }
    public DateTimeOffset? NextExpiration { get; init; }
}

/// <summary>
/// Request to add credits.
/// </summary>
public record AddCreditsRequest
{
    public long Amount { get; init; }
    public string Type { get; init; } = "bonus";
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Request to spend credits.
/// </summary>
public record SpendCreditsRequest
{
    public long Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
}
