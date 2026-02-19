namespace TadHub.Infrastructure.Settings;

/// <summary>
/// Stripe payment gateway settings.
/// </summary>
public class StripeSettings
{
    public const string SectionName = "Stripe";

    /// <summary>
    /// Stripe publishable key (for client-side).
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>
    /// Stripe secret key (for server-side).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Webhook signing secret.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable Stripe integration.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
