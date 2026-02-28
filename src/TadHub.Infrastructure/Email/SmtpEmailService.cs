using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Email;

/// <summary>
/// SMTP-based email service using MailKit.
/// Uses platform SMTP settings by default, with optional tenant config override.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        await SendWithTenantConfigAsync(message, null, ct);
    }

    public async Task SendWithTenantConfigAsync(
        EmailMessage message,
        EmailProviderConfig? tenantConfig,
        CancellationToken ct = default)
    {
        var host = tenantConfig?.SmtpHost ?? _settings.Host;
        var port = tenantConfig?.SmtpPort ?? _settings.Port;
        var username = tenantConfig?.SmtpUsername ?? _settings.Username;
        var password = tenantConfig?.SmtpPassword ?? _settings.Password;
        var useSsl = tenantConfig?.UseSsl ?? _settings.EnableSsl;
        var fromEmail = message.FromEmail ?? tenantConfig?.FromEmail ?? _settings.FromEmail;
        var fromName = message.FromName ?? tenantConfig?.FromName ?? _settings.FromName;

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message.HtmlBody };
        mimeMessage.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var secureSocketOptions = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureSocketOptions, ct);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password, ct);
            }

            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", message.To, message.Subject);
            throw;
        }
    }
}
