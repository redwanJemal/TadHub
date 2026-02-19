namespace Subscription.Core.Gateways;

/// <summary>
/// Abstraction for payment gateway operations.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Creates a Stripe customer for a tenant.
    /// </summary>
    Task<string> CreateCustomerAsync(Guid tenantId, string email, string? name, CancellationToken ct = default);

    /// <summary>
    /// Creates a checkout session for subscription.
    /// </summary>
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        string customerId,
        string priceId,
        string successUrl,
        string cancelUrl,
        int? trialDays = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a billing portal session.
    /// </summary>
    Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl, CancellationToken ct = default);

    /// <summary>
    /// Cancels a subscription.
    /// </summary>
    Task CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false, CancellationToken ct = default);

    /// <summary>
    /// Resumes a canceled subscription.
    /// </summary>
    Task ResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Gets subscription details.
    /// </summary>
    Task<StripeSubscriptionInfo?> GetSubscriptionAsync(string subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Validates a webhook signature and parses the event.
    /// </summary>
    StripeWebhookEvent? ValidateWebhook(string payload, string signature);
}

/// <summary>
/// Result of checkout session creation.
/// </summary>
public record CheckoutSessionResult
{
    public string SessionId { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Stripe subscription info.
/// </summary>
public record StripeSubscriptionInfo
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string? PriceId { get; init; }
    public DateTimeOffset CurrentPeriodStart { get; init; }
    public DateTimeOffset CurrentPeriodEnd { get; init; }
    public DateTimeOffset? TrialEnd { get; init; }
    public DateTimeOffset? CanceledAt { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
}

/// <summary>
/// Parsed Stripe webhook event.
/// </summary>
public record StripeWebhookEvent
{
    public string Type { get; init; } = string.Empty;
    public string? SubscriptionId { get; init; }
    public string? CustomerId { get; init; }
    public string? SessionId { get; init; }
    public string? InvoiceId { get; init; }
    public object? Data { get; init; }
}
