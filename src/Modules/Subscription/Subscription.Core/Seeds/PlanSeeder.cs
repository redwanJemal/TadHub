using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using Subscription.Core.Entities;

namespace Subscription.Core.Seeds;

/// <summary>
/// Seeds default subscription plans on application startup.
/// </summary>
public class PlanSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlanSeeder> _logger;

    public PlanSeeder(IServiceProvider serviceProvider, ILogger<PlanSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedPlansAsync(db, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedPlansAsync(AppDbContext db, CancellationToken ct)
    {
        var existingPlans = await db.Set<Plan>().AnyAsync(ct);
        if (existingPlans)
        {
            _logger.LogDebug("Plans already seeded, skipping");
            return;
        }

        var plans = GetDefaultPlans();

        foreach (var plan in plans)
        {
            db.Set<Plan>().Add(plan);
            _logger.LogInformation("Seeding plan: {PlanName}", plan.Name);
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Plan seeding completed - {Count} plans created", plans.Count);
    }

    private static List<Plan> GetDefaultPlans()
    {
        var freePlanId = Guid.NewGuid();
        var proPlanId = Guid.NewGuid();
        var enterprisePlanId = Guid.NewGuid();

        return new List<Plan>
        {
            // Free Plan
            new()
            {
                Id = freePlanId,
                Name = "Free",
                Slug = "free",
                Description = "Perfect for getting started",
                IsActive = true,
                IsDefault = true,
                DisplayOrder = 1,
                Prices = new List<PlanPrice>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PlanId = freePlanId,
                        Amount = 0,
                        Currency = "usd",
                        Interval = "month",
                        IntervalCount = 1,
                        TrialDays = 0,
                        IsActive = true
                    }
                },
                Features = new List<PlanFeature>
                {
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "max_users", Name = "Team Members", ValueType = "number", NumericValue = 3, DisplayOrder = 1 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "max_projects", Name = "Projects", ValueType = "number", NumericValue = 5, DisplayOrder = 2 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "storage_gb", Name = "Storage", ValueType = "number", NumericValue = 1, DisplayOrder = 3 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "api_calls_per_month", Name = "API Calls/Month", ValueType = "number", NumericValue = 1000, DisplayOrder = 4 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "custom_domain", Name = "Custom Domain", ValueType = "boolean", BooleanValue = false, DisplayOrder = 5 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "priority_support", Name = "Priority Support", ValueType = "boolean", BooleanValue = false, DisplayOrder = 6 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "analytics", Name = "Analytics", ValueType = "boolean", BooleanValue = false, DisplayOrder = 7 },
                    new() { Id = Guid.NewGuid(), PlanId = freePlanId, Key = "audit_logs", Name = "Audit Logs", ValueType = "boolean", BooleanValue = false, DisplayOrder = 8 }
                }
            },

            // Pro Plan
            new()
            {
                Id = proPlanId,
                Name = "Pro",
                Slug = "pro",
                Description = "For growing teams",
                IsActive = true,
                IsDefault = false,
                DisplayOrder = 2,
                Prices = new List<PlanPrice>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PlanId = proPlanId,
                        Amount = 2900, // $29/month
                        Currency = "usd",
                        Interval = "month",
                        IntervalCount = 1,
                        TrialDays = 14,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PlanId = proPlanId,
                        Amount = 29000, // $290/year (2 months free)
                        Currency = "usd",
                        Interval = "year",
                        IntervalCount = 1,
                        TrialDays = 14,
                        IsActive = true
                    }
                },
                Features = new List<PlanFeature>
                {
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "max_users", Name = "Team Members", ValueType = "number", NumericValue = 25, DisplayOrder = 1 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "max_projects", Name = "Projects", ValueType = "number", NumericValue = 50, DisplayOrder = 2 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "storage_gb", Name = "Storage", ValueType = "number", NumericValue = 50, DisplayOrder = 3 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "api_calls_per_month", Name = "API Calls/Month", ValueType = "number", NumericValue = 100000, DisplayOrder = 4 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "custom_domain", Name = "Custom Domain", ValueType = "boolean", BooleanValue = true, DisplayOrder = 5 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "priority_support", Name = "Priority Support", ValueType = "boolean", BooleanValue = true, DisplayOrder = 6 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "analytics", Name = "Analytics", ValueType = "boolean", BooleanValue = true, DisplayOrder = 7 },
                    new() { Id = Guid.NewGuid(), PlanId = proPlanId, Key = "audit_logs", Name = "Audit Logs", ValueType = "boolean", BooleanValue = false, DisplayOrder = 8 }
                }
            },

            // Enterprise Plan
            new()
            {
                Id = enterprisePlanId,
                Name = "Enterprise",
                Slug = "enterprise",
                Description = "For large organizations",
                IsActive = true,
                IsDefault = false,
                DisplayOrder = 3,
                Prices = new List<PlanPrice>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PlanId = enterprisePlanId,
                        Amount = 9900, // $99/month
                        Currency = "usd",
                        Interval = "month",
                        IntervalCount = 1,
                        TrialDays = 30,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PlanId = enterprisePlanId,
                        Amount = 99000, // $990/year (2 months free)
                        Currency = "usd",
                        Interval = "year",
                        IntervalCount = 1,
                        TrialDays = 30,
                        IsActive = true
                    }
                },
                Features = new List<PlanFeature>
                {
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "max_users", Name = "Team Members", ValueType = "unlimited", IsUnlimited = true, DisplayOrder = 1 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "max_projects", Name = "Projects", ValueType = "unlimited", IsUnlimited = true, DisplayOrder = 2 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "storage_gb", Name = "Storage", ValueType = "number", NumericValue = 500, DisplayOrder = 3 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "api_calls_per_month", Name = "API Calls/Month", ValueType = "unlimited", IsUnlimited = true, DisplayOrder = 4 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "custom_domain", Name = "Custom Domain", ValueType = "boolean", BooleanValue = true, DisplayOrder = 5 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "priority_support", Name = "Priority Support", ValueType = "boolean", BooleanValue = true, DisplayOrder = 6 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "analytics", Name = "Analytics", ValueType = "boolean", BooleanValue = true, DisplayOrder = 7 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "audit_logs", Name = "Audit Logs", ValueType = "boolean", BooleanValue = true, DisplayOrder = 8 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "sso", Name = "Single Sign-On (SSO)", ValueType = "boolean", BooleanValue = true, DisplayOrder = 9 },
                    new() { Id = Guid.NewGuid(), PlanId = enterprisePlanId, Key = "dedicated_support", Name = "Dedicated Support", ValueType = "boolean", BooleanValue = true, DisplayOrder = 10 }
                }
            }
        };
    }
}
