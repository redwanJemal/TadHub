using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaasKit.Infrastructure.Settings;
using Stripe;
using Stripe.Checkout;

namespace Subscription.Core.Gateways;

/// <summary>
/// Stripe implementation of IPaymentGateway.
/// </summary>
public class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(IOptions<StripeSettings> settings, ILogger<StripePaymentGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<string> CreateCustomerAsync(Guid tenantId, string email, string? name, CancellationToken ct = default)
    {
        var service = new CustomerService();
        var customer = await service.CreateAsync(new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId.ToString()
            }
        }, cancellationToken: ct);

        _logger.LogInformation("Created Stripe customer {CustomerId} for tenant {TenantId}", customer.Id, tenantId);
        return customer.Id;
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        string customerId,
        string priceId,
        string successUrl,
        string cancelUrl,
        int? trialDays = null,
        CancellationToken ct = default)
    {
        var service = new SessionService();

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = priceId,
                    Quantity = 1
                }
            }
        };

        if (trialDays.HasValue && trialDays.Value > 0)
        {
            options.SubscriptionData = new SessionSubscriptionDataOptions
            {
                TrialPeriodDays = trialDays.Value
            };
        }

        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation("Created checkout session {SessionId} for customer {CustomerId}", session.Id, customerId);

        return new CheckoutSessionResult
        {
            SessionId = session.Id,
            Url = session.Url,
            ExpiresAt = new DateTimeOffset(session.ExpiresAt, TimeSpan.Zero)
        };
    }

    public async Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl, CancellationToken ct = default)
    {
        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl
        }, cancellationToken: ct);

        return session.Url;
    }

    public async Task CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false, CancellationToken ct = default)
    {
        var service = new SubscriptionService();

        if (cancelImmediately)
        {
            await service.CancelAsync(subscriptionId, cancellationToken: ct);
            _logger.LogInformation("Canceled subscription {SubscriptionId} immediately", subscriptionId);
        }
        else
        {
            await service.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            }, cancellationToken: ct);
            _logger.LogInformation("Set subscription {SubscriptionId} to cancel at period end", subscriptionId);
        }
    }

    public async Task ResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        var service = new SubscriptionService();
        await service.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = false
        }, cancellationToken: ct);

        _logger.LogInformation("Resumed subscription {SubscriptionId}", subscriptionId);
    }

    public async Task<StripeSubscriptionInfo?> GetSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        try
        {
            var service = new SubscriptionService();
            var sub = await service.GetAsync(subscriptionId, cancellationToken: ct);

            return new StripeSubscriptionInfo
            {
                Id = sub.Id,
                Status = sub.Status,
                CustomerId = sub.CustomerId,
                PriceId = sub.Items.Data.FirstOrDefault()?.Price?.Id,
                CurrentPeriodStart = new DateTimeOffset(sub.CurrentPeriodStart, TimeSpan.Zero),
                CurrentPeriodEnd = new DateTimeOffset(sub.CurrentPeriodEnd, TimeSpan.Zero),
                TrialEnd = sub.TrialEnd.HasValue ? new DateTimeOffset(sub.TrialEnd.Value, TimeSpan.Zero) : null,
                CanceledAt = sub.CanceledAt.HasValue ? new DateTimeOffset(sub.CanceledAt.Value, TimeSpan.Zero) : null,
                CancelAtPeriodEnd = sub.CancelAtPeriodEnd
            };
        }
        catch (StripeException ex) when (ex.StripeError?.Code == "resource_missing")
        {
            return null;
        }
    }

    public StripeWebhookEvent? ValidateWebhook(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _settings.WebhookSecret);

            var result = new StripeWebhookEvent
            {
                Type = stripeEvent.Type,
                Data = stripeEvent.Data.Object
            };

            // Extract common fields based on event type
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    if (stripeEvent.Data.Object is Session session)
                    {
                        return result with
                        {
                            SessionId = session.Id,
                            CustomerId = session.CustomerId,
                            SubscriptionId = session.SubscriptionId
                        };
                    }
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                    if (stripeEvent.Data.Object is Stripe.Subscription stripeSub)
                    {
                        return result with
                        {
                            SubscriptionId = stripeSub.Id,
                            CustomerId = stripeSub.CustomerId
                        };
                    }
                    break;

                case "invoice.paid":
                case "invoice.payment_failed":
                    if (stripeEvent.Data.Object is Invoice invoice)
                    {
                        return result with
                        {
                            InvoiceId = invoice.Id,
                            CustomerId = invoice.CustomerId,
                            SubscriptionId = invoice.SubscriptionId
                        };
                    }
                    break;
            }

            return result;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Failed to validate Stripe webhook");
            return null;
        }
    }
}
