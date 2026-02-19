using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;
using Subscription.Contracts.DTOs;

namespace Subscription.Contracts;

/// <summary>
/// Service for managing subscription plans.
/// </summary>
public interface IPlanService
{
    Task<PagedList<PlanDto>> GetPlansAsync(QueryParameters qp, CancellationToken ct = default);
    Task<Result<PlanDto>> GetPlanByIdAsync(Guid planId, CancellationToken ct = default);
    Task<Result<PlanDto>> GetPlanBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<PlanDto>> GetDefaultPlanAsync(CancellationToken ct = default);
}

/// <summary>
/// Service for managing tenant subscriptions.
/// </summary>
public interface ISubscriptionService
{
    Task<Result<TenantSubscriptionDto>> GetSubscriptionAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(Guid tenantId, CreateCheckoutRequest request, CancellationToken ct = default);
    Task<Result<TenantSubscriptionDto>> CancelSubscriptionAsync(Guid tenantId, CancelSubscriptionRequest request, CancellationToken ct = default);
    Task<Result<TenantSubscriptionDto>> ResumeSubscriptionAsync(Guid tenantId, CancellationToken ct = default);
    Task HandleStripeWebhookAsync(string payload, string signature, CancellationToken ct = default);
}

/// <summary>
/// Service for checking feature access based on subscription.
/// </summary>
public interface IFeatureGateService
{
    Task<FeatureGateResult> CheckFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default);
    Task<FeatureGateResult> CheckLimitAsync(Guid tenantId, string featureKey, long currentUsage, CancellationToken ct = default);
    Task<Dictionary<string, FeatureGateResult>> CheckFeaturesAsync(Guid tenantId, IEnumerable<string> featureKeys, CancellationToken ct = default);
}

/// <summary>
/// Result of a feature gate check.
/// </summary>
public record FeatureGateResult
{
    public bool IsAllowed { get; init; }
    public string FeatureKey { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public long? Limit { get; init; }
    public long? CurrentUsage { get; init; }
    public long? Remaining => Limit.HasValue && CurrentUsage.HasValue ? Limit.Value - CurrentUsage.Value : null;

    public static FeatureGateResult Allowed(string featureKey) => new() { IsAllowed = true, FeatureKey = featureKey };
    public static FeatureGateResult Denied(string featureKey, string reason) => new() { IsAllowed = false, FeatureKey = featureKey, Reason = reason };
    public static FeatureGateResult LimitExceeded(string featureKey, long limit, long current) => new()
    {
        IsAllowed = false,
        FeatureKey = featureKey,
        Reason = $"Limit of {limit} exceeded",
        Limit = limit,
        CurrentUsage = current
    };
}

/// <summary>
/// Service for managing credit ledger.
/// </summary>
public interface ICreditService
{
    Task<CreditBalanceDto> GetBalanceAsync(Guid tenantId, CancellationToken ct = default);
    Task<PagedList<CreditDto>> GetHistoryAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<CreditDto>> AddCreditsAsync(Guid tenantId, AddCreditsRequest request, CancellationToken ct = default);
    Task<Result<CreditDto>> SpendCreditsAsync(Guid tenantId, SpendCreditsRequest request, CancellationToken ct = default);
    Task<bool> HasSufficientCreditsAsync(Guid tenantId, long amount, CancellationToken ct = default);
}
