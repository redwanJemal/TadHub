namespace TadHub.Infrastructure.Email;

/// <summary>
/// Email sending service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email using platform default SMTP settings.
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);

    /// <summary>
    /// Sends an email using tenant-specific SMTP config, falling back to platform defaults.
    /// </summary>
    Task SendWithTenantConfigAsync(EmailMessage message, EmailProviderConfig? tenantConfig, CancellationToken ct = default);
}

/// <summary>
/// Email message model.
/// </summary>
public sealed record EmailMessage
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
}

/// <summary>
/// Tenant-specific email provider configuration.
/// </summary>
public sealed record EmailProviderConfig
{
    public string Provider { get; init; } = "smtp";
    public string? SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public bool UseSsl { get; init; } = true;
    public string? SendGridApiKey { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
}

