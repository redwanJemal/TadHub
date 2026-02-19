namespace Subscription.Contracts.DTOs;

/// <summary>
/// DTO for tenant subscription data.
/// </summary>
public record TenantSubscriptionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public string PlanSlug { get; init; } = string.Empty;
    public Guid PlanPriceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CurrentPeriodStart { get; init; }
    public DateTimeOffset CurrentPeriodEnd { get; init; }
    public DateTimeOffset? TrialEnd { get; init; }
    public DateTimeOffset? CanceledAt { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Whether the subscription is in a trial period.
    /// </summary>
    public bool IsTrialing => Status == "trialing";

    /// <summary>
    /// Whether the subscription is active (active or trialing).
    /// </summary>
    public bool IsActive => Status == "active" || Status == "trialing";
}

/// <summary>
/// Request to create a checkout session.
/// </summary>
public record CreateCheckoutRequest
{
    public Guid PlanId { get; init; }
    public Guid PlanPriceId { get; init; }
    public string? SuccessUrl { get; init; }
    public string? CancelUrl { get; init; }
}

/// <summary>
/// Response from checkout session creation.
/// </summary>
public record CheckoutSessionDto
{
    public Guid Id { get; init; }
    public string StripeSessionId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Url { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Request to cancel subscription.
/// </summary>
public record CancelSubscriptionRequest
{
    public bool CancelImmediately { get; init; } = false;
}
