using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;
using Subscription.Core.Entities;
using Subscription.Core.Gateways;

namespace Subscription.Core.Services;

/// <summary>
/// Service for managing tenant subscriptions.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        AppDbContext db,
        IPaymentGateway paymentGateway,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<SubscriptionService> logger)
    {
        _db = db;
        _paymentGateway = paymentGateway;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<TenantSubscriptionDto>> GetSubscriptionAsync(Guid tenantId, CancellationToken ct = default)
    {
        var subscription = await _db.Set<TenantSubscription>()
            .AsNoTracking()
            .Include(x => x.Plan)
            .Include(x => x.PlanPrice)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        if (subscription is null)
            return Result<TenantSubscriptionDto>.NotFound("No subscription found for this tenant");

        return Result<TenantSubscriptionDto>.Success(MapToDto(subscription));
    }

    public async Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(
        Guid tenantId,
        CreateCheckoutRequest request,
        CancellationToken ct = default)
    {
        // Get plan and price
        var planPrice = await _db.Set<PlanPrice>()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == request.PlanPriceId && x.PlanId == request.PlanId, ct);

        if (planPrice is null)
            return Result<CheckoutSessionDto>.NotFound("Plan or price not found");

        if (planPrice.StripePriceId is null)
            return Result<CheckoutSessionDto>.ValidationError("Plan price is not configured for Stripe");

        // Get or create Stripe customer
        var subscription = await _db.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        string customerId;
        if (subscription?.StripeCustomerId is not null)
        {
            customerId = subscription.StripeCustomerId;
        }
        else
        {
            // Create new customer (in production, get email from tenant/user)
            customerId = await _paymentGateway.CreateCustomerAsync(
                tenantId,
                $"tenant-{tenantId}@example.com",
                null,
                ct);
        }

        // Create checkout session
        var successUrl = request.SuccessUrl ?? "https://app.example.com/settings/billing?success=true";
        var cancelUrl = request.CancelUrl ?? "https://app.example.com/settings/billing?canceled=true";

        var sessionResult = await _paymentGateway.CreateCheckoutSessionAsync(
            customerId,
            planPrice.StripePriceId,
            successUrl,
            cancelUrl,
            planPrice.TrialDays > 0 ? planPrice.TrialDays : null,
            ct);

        // Store checkout session
        var checkoutSession = new CheckoutSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StripeSessionId = sessionResult.SessionId,
            PlanId = request.PlanId,
            PlanPriceId = request.PlanPriceId,
            Status = "pending",
            Url = sessionResult.Url,
            ExpiresAt = sessionResult.ExpiresAt,
            InitiatedByUserId = _currentUser.UserId
        };

        _db.Set<CheckoutSession>().Add(checkoutSession);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created checkout session {SessionId} for tenant {TenantId}, plan {PlanId}",
            checkoutSession.Id, tenantId, request.PlanId);

        return Result<CheckoutSessionDto>.Success(new CheckoutSessionDto
        {
            Id = checkoutSession.Id,
            StripeSessionId = sessionResult.SessionId,
            Status = "pending",
            Url = sessionResult.Url,
            ExpiresAt = sessionResult.ExpiresAt
        });
    }

    public async Task<Result<TenantSubscriptionDto>> CancelSubscriptionAsync(
        Guid tenantId,
        CancelSubscriptionRequest request,
        CancellationToken ct = default)
    {
        var subscription = await _db.Set<TenantSubscription>()
            .Include(x => x.Plan)
            .Include(x => x.PlanPrice)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        if (subscription is null)
            return Result<TenantSubscriptionDto>.NotFound("No subscription found");

        if (subscription.StripeSubscriptionId is null)
            return Result<TenantSubscriptionDto>.ValidationError("Subscription is not managed by Stripe");

        await _paymentGateway.CancelSubscriptionAsync(
            subscription.StripeSubscriptionId,
            request.CancelImmediately,
            ct);

        if (request.CancelImmediately)
        {
            subscription.Status = "canceled";
            subscription.CanceledAt = _clock.UtcNow;
        }
        else
        {
            subscription.CancelAtPeriodEnd = true;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Canceled subscription for tenant {TenantId}. Immediate: {Immediate}",
            tenantId, request.CancelImmediately);

        return Result<TenantSubscriptionDto>.Success(MapToDto(subscription));
    }

    public async Task<Result<TenantSubscriptionDto>> ResumeSubscriptionAsync(Guid tenantId, CancellationToken ct = default)
    {
        var subscription = await _db.Set<TenantSubscription>()
            .Include(x => x.Plan)
            .Include(x => x.PlanPrice)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        if (subscription is null)
            return Result<TenantSubscriptionDto>.NotFound("No subscription found");

        if (!subscription.CancelAtPeriodEnd)
            return Result<TenantSubscriptionDto>.ValidationError("Subscription is not scheduled for cancellation");

        if (subscription.StripeSubscriptionId is not null)
        {
            await _paymentGateway.ResumeSubscriptionAsync(subscription.StripeSubscriptionId, ct);
        }

        subscription.CancelAtPeriodEnd = false;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Resumed subscription for tenant {TenantId}", tenantId);

        return Result<TenantSubscriptionDto>.Success(MapToDto(subscription));
    }

    public async Task HandleStripeWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        var webhookEvent = _paymentGateway.ValidateWebhook(payload, signature);
        if (webhookEvent is null)
        {
            _logger.LogWarning("Invalid webhook signature");
            return;
        }

        _logger.LogInformation("Processing Stripe webhook: {EventType}", webhookEvent.Type);

        switch (webhookEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync(webhookEvent, ct);
                break;

            case "customer.subscription.updated":
                await HandleSubscriptionUpdatedAsync(webhookEvent, ct);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(webhookEvent, ct);
                break;

            case "invoice.paid":
                _logger.LogInformation("Invoice paid for subscription {SubscriptionId}", webhookEvent.SubscriptionId);
                break;

            case "invoice.payment_failed":
                await HandlePaymentFailedAsync(webhookEvent, ct);
                break;
        }
    }

    private async Task HandleCheckoutCompletedAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (webhookEvent.SessionId is null) return;

        var checkoutSession = await _db.Set<CheckoutSession>()
            .FirstOrDefaultAsync(x => x.StripeSessionId == webhookEvent.SessionId, ct);

        if (checkoutSession is null)
        {
            _logger.LogWarning("Checkout session not found: {SessionId}", webhookEvent.SessionId);
            return;
        }

        checkoutSession.Status = "completed";
        checkoutSession.CompletedAt = _clock.UtcNow;

        // Get or create subscription
        var subscription = await _db.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.TenantId == checkoutSession.TenantId, ct);

        var planPrice = await _db.Set<PlanPrice>()
            .FirstOrDefaultAsync(x => x.Id == checkoutSession.PlanPriceId, ct);

        if (subscription is null)
        {
            subscription = new TenantSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = checkoutSession.TenantId,
                PlanId = checkoutSession.PlanId,
                PlanPriceId = checkoutSession.PlanPriceId,
                Status = "active",
                StripeSubscriptionId = webhookEvent.SubscriptionId,
                StripeCustomerId = webhookEvent.CustomerId,
                CurrentPeriodStart = _clock.UtcNow,
                CurrentPeriodEnd = _clock.UtcNow.AddMonths(planPrice?.Interval == "year" ? 12 : 1)
            };
            _db.Set<TenantSubscription>().Add(subscription);
        }
        else
        {
            subscription.PlanId = checkoutSession.PlanId;
            subscription.PlanPriceId = checkoutSession.PlanPriceId;
            subscription.Status = "active";
            subscription.StripeSubscriptionId = webhookEvent.SubscriptionId;
            subscription.StripeCustomerId = webhookEvent.CustomerId;
            subscription.CancelAtPeriodEnd = false;
            subscription.CanceledAt = null;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Checkout completed for tenant {TenantId}, subscription {SubscriptionId}",
            checkoutSession.TenantId, webhookEvent.SubscriptionId);
    }

    private async Task HandleSubscriptionUpdatedAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (webhookEvent.SubscriptionId is null) return;

        var stripeSubscription = await _paymentGateway.GetSubscriptionAsync(webhookEvent.SubscriptionId, ct);
        if (stripeSubscription is null) return;

        var subscription = await _db.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == webhookEvent.SubscriptionId, ct);

        if (subscription is null) return;

        subscription.Status = stripeSubscription.Status;
        subscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
        subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
        subscription.TrialEnd = stripeSubscription.TrialEnd;
        subscription.CanceledAt = stripeSubscription.CanceledAt;
        subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Updated subscription {SubscriptionId} for tenant {TenantId}. Status: {Status}",
            webhookEvent.SubscriptionId, subscription.TenantId, stripeSubscription.Status);
    }

    private async Task HandleSubscriptionDeletedAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (webhookEvent.SubscriptionId is null) return;

        var subscription = await _db.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == webhookEvent.SubscriptionId, ct);

        if (subscription is null) return;

        subscription.Status = "canceled";
        subscription.CanceledAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Subscription {SubscriptionId} deleted for tenant {TenantId}",
            webhookEvent.SubscriptionId, subscription.TenantId);
    }

    private async Task HandlePaymentFailedAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (webhookEvent.SubscriptionId is null) return;

        var subscription = await _db.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == webhookEvent.SubscriptionId, ct);

        if (subscription is null) return;

        subscription.Status = "past_due";
        await _db.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Payment failed for subscription {SubscriptionId}, tenant {TenantId}",
            webhookEvent.SubscriptionId, subscription.TenantId);

        // TODO: Send notification to tenant about payment failure
    }

    private static TenantSubscriptionDto MapToDto(TenantSubscription s) => new()
    {
        Id = s.Id,
        TenantId = s.TenantId,
        PlanId = s.PlanId,
        PlanName = s.Plan.Name,
        PlanSlug = s.Plan.Slug,
        PlanPriceId = s.PlanPriceId,
        Status = s.Status,
        CurrentPeriodStart = s.CurrentPeriodStart,
        CurrentPeriodEnd = s.CurrentPeriodEnd,
        TrialEnd = s.TrialEnd,
        CanceledAt = s.CanceledAt,
        CancelAtPeriodEnd = s.CancelAtPeriodEnd,
        CreatedAt = s.CreatedAt
    };
}
