using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using Notification.Core.Services;
using TadHub.Infrastructure.Email;
using TadHub.Infrastructure.Email.Templates;

namespace Notification.Core.Channels;

/// <summary>
/// Email notification channel. Sends emails using tenant config or platform defaults.
/// </summary>
public sealed class EmailNotificationChannel : INotificationChannel
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ITenantNotificationSettingsProvider _settingsProvider;
    private readonly ILogger<EmailNotificationChannel> _logger;

    public ChannelType ChannelType => ChannelType.Email;

    public EmailNotificationChannel(
        IEmailService emailService,
        IEmailTemplateRenderer templateRenderer,
        ITenantNotificationSettingsProvider settingsProvider,
        ILogger<EmailNotificationChannel> logger)
    {
        _emailService = emailService;
        _templateRenderer = templateRenderer;
        _settingsProvider = settingsProvider;
        _logger = logger;
    }

    public async Task SendAsync(NotificationContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(context.RecipientEmail))
        {
            _logger.LogDebug("Skipping email for user {UserId} — no email address", context.RecipientUserId);
            return;
        }

        var settings = await _settingsProvider.GetSettingsAsync(context.TenantId, ct);

        // Determine template name from event type
        var templateName = context.EventType switch
        {
            "document.expiring" => "DocumentExpiring",
            "document.expired" => "DocumentExpired",
            _ => "GenericNotification"
        };

        // Build template data
        var data = new Dictionary<string, string>(context.TemplateData)
        {
            ["Title"] = context.Title,
            ["Body"] = context.Body,
            ["Link"] = context.Link ?? "#"
        };

        var htmlBody = _templateRenderer.Render(templateName, data);

        // Build tenant email provider config
        EmailProviderConfig? tenantConfig = null;
        if (settings?.Email is { Enabled: true })
        {
            tenantConfig = new EmailProviderConfig
            {
                Provider = settings.Email.Provider,
                SmtpHost = settings.Email.SmtpHost,
                SmtpPort = settings.Email.SmtpPort,
                SmtpUsername = settings.Email.SmtpUsername,
                SmtpPassword = settings.Email.SmtpPassword,
                UseSsl = settings.Email.UseSsl,
                SendGridApiKey = settings.Email.SendGridApiKey,
                FromEmail = settings.Email.FromEmail,
                FromName = settings.Email.FromName
            };
        }

        var message = new EmailMessage
        {
            To = context.RecipientEmail,
            Subject = context.Title,
            HtmlBody = htmlBody
        };

        await _emailService.SendWithTenantConfigAsync(message, tenantConfig, ct);
    }

    public async Task<bool> IsAvailableAsync(Guid tenantId, CancellationToken ct = default)
    {
        var settings = await _settingsProvider.GetSettingsAsync(tenantId, ct);
        // Available if tenant has email enabled, or we fall back to platform defaults
        return settings?.Email.Enabled == true || true; // Always available via platform fallback
    }
}
