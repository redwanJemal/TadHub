using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Caching;
using TadHub.Infrastructure.Persistence;
using Subscription.Contracts;
using Subscription.Core.Entities;

namespace Subscription.Core.Services;

/// <summary>
/// Service for checking feature access based on tenant subscription.
/// Caches feature limits in Redis for performance.
/// </summary>
public class FeatureGateService : IFeatureGateService
{
    private readonly AppDbContext _db;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<FeatureGateService> _logger;

    private const string FeatureCacheKeyPrefix = "tenant:features";
    private static readonly TimeSpan FeatureCacheDuration = TimeSpan.FromMinutes(5);

    public FeatureGateService(AppDbContext db, IRedisCacheService cache, ILogger<FeatureGateService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<FeatureGateResult> CheckFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default)
    {
        var features = await GetTenantFeaturesAsync(tenantId, ct);

        if (!features.TryGetValue(featureKey, out var feature))
        {
            // Feature not defined - default to denied
            return FeatureGateResult.Denied(featureKey, "Feature not available on current plan");
        }

        // Boolean feature
        if (feature.ValueType == "boolean")
        {
            return feature.BooleanValue == true
                ? FeatureGateResult.Allowed(featureKey)
                : FeatureGateResult.Denied(featureKey, "Feature not available on current plan");
        }

        // Unlimited feature
        if (feature.IsUnlimited)
        {
            return FeatureGateResult.Allowed(featureKey);
        }

        // Numeric feature without current usage (just check if available)
        return FeatureGateResult.Allowed(featureKey);
    }

    public async Task<FeatureGateResult> CheckLimitAsync(Guid tenantId, string featureKey, long currentUsage, CancellationToken ct = default)
    {
        var features = await GetTenantFeaturesAsync(tenantId, ct);

        if (!features.TryGetValue(featureKey, out var feature))
        {
            return FeatureGateResult.Denied(featureKey, "Feature not available on current plan");
        }

        // Unlimited feature
        if (feature.IsUnlimited)
        {
            return FeatureGateResult.Allowed(featureKey);
        }

        // Non-numeric feature
        if (feature.ValueType != "number" || !feature.NumericValue.HasValue)
        {
            return FeatureGateResult.Denied(featureKey, "Feature does not have a numeric limit");
        }

        var limit = feature.NumericValue.Value;

        if (currentUsage >= limit)
        {
            return FeatureGateResult.LimitExceeded(featureKey, limit, currentUsage);
        }

        return new FeatureGateResult
        {
            IsAllowed = true,
            FeatureKey = featureKey,
            Limit = limit,
            CurrentUsage = currentUsage
        };
    }

    public async Task<Dictionary<string, FeatureGateResult>> CheckFeaturesAsync(
        Guid tenantId,
        IEnumerable<string> featureKeys,
        CancellationToken ct = default)
    {
        var results = new Dictionary<string, FeatureGateResult>();

        foreach (var key in featureKeys)
        {
            results[key] = await CheckFeatureAsync(tenantId, key, ct);
        }

        return results;
    }

    private async Task<Dictionary<string, PlanFeature>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct)
    {
        var cacheKey = $"{FeatureCacheKeyPrefix}:{tenantId}";

        var cached = await _cache.GetAsync<Dictionary<string, CachedFeature>>(cacheKey, ct);
        if (cached != null)
        {
            return cached.ToDictionary(x => x.Key, x => new PlanFeature
            {
                Key = x.Value.Key,
                ValueType = x.Value.ValueType,
                BooleanValue = x.Value.BooleanValue,
                NumericValue = x.Value.NumericValue,
                IsUnlimited = x.Value.IsUnlimited
            });
        }

        // Load from database
        var subscription = await _db.Set<TenantSubscription>()
            .AsNoTracking()
            .Include(x => x.Plan)
            .ThenInclude(x => x.Features)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        if (subscription is null)
        {
            _logger.LogWarning("No subscription found for tenant {TenantId}, using default plan features", tenantId);

            // Try to get default plan features
            var defaultPlan = await _db.Set<Plan>()
                .AsNoTracking()
                .Include(x => x.Features)
                .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive, ct);

            if (defaultPlan is null)
            {
                return new Dictionary<string, PlanFeature>();
            }

            var defaultFeatures = defaultPlan.Features.ToDictionary(f => f.Key);
            await CacheFeaturesAsync(cacheKey, defaultFeatures, ct);
            return defaultFeatures;
        }

        var features = subscription.Plan.Features.ToDictionary(f => f.Key);
        await CacheFeaturesAsync(cacheKey, features, ct);

        return features;
    }

    private async Task CacheFeaturesAsync(string cacheKey, Dictionary<string, PlanFeature> features, CancellationToken ct)
    {
        var cacheable = features.ToDictionary(x => x.Key, x => new CachedFeature
        {
            Key = x.Value.Key,
            ValueType = x.Value.ValueType,
            BooleanValue = x.Value.BooleanValue,
            NumericValue = x.Value.NumericValue,
            IsUnlimited = x.Value.IsUnlimited
        });

        await _cache.SetAsync(cacheKey, cacheable, FeatureCacheDuration, ct);
    }

    private record CachedFeature
    {
        public string Key { get; init; } = string.Empty;
        public string ValueType { get; init; } = "boolean";
        public bool? BooleanValue { get; init; }
        public long? NumericValue { get; init; }
        public bool IsUnlimited { get; init; }
    }
}
