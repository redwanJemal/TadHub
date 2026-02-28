using System.Text.Json.Serialization;

namespace Notification.Contracts.Settings;

/// <summary>
/// Tenant-level notification settings, stored in Tenant.Settings JSONB under "notifications" key.
/// </summary>
public sealed class TenantNotificationSettings
{
    [JsonPropertyName("email")]
    public EmailChannelSettings Email { get; set; } = new();

    [JsonPropertyName("whatsapp")]
    public WhatsAppChannelSettings WhatsApp { get; set; } = new();

    [JsonPropertyName("telegram")]
    public TelegramChannelSettings Telegram { get; set; } = new();

    [JsonPropertyName("eventPreferences")]
    public Dictionary<string, EventNotificationPreference> EventPreferences { get; set; } = new();
}

/// <summary>
/// Email channel configuration.
/// </summary>
public sealed class EmailChannelSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "smtp";

    [JsonPropertyName("smtpHost")]
    public string? SmtpHost { get; set; }

    [JsonPropertyName("smtpPort")]
    public int SmtpPort { get; set; } = 587;

    [JsonPropertyName("smtpUsername")]
    public string? SmtpUsername { get; set; }

    [JsonPropertyName("smtpPassword")]
    public string? SmtpPassword { get; set; }

    [JsonPropertyName("useSsl")]
    public bool UseSsl { get; set; } = true;

    [JsonPropertyName("sendGridApiKey")]
    public string? SendGridApiKey { get; set; }

    [JsonPropertyName("fromEmail")]
    public string? FromEmail { get; set; }

    [JsonPropertyName("fromName")]
    public string? FromName { get; set; }
}

/// <summary>
/// WhatsApp channel configuration (future).
/// </summary>
public sealed class WhatsAppChannelSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("apiToken")]
    public string? ApiToken { get; set; }
}

/// <summary>
/// Telegram channel configuration (future).
/// </summary>
public sealed class TelegramChannelSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("apiToken")]
    public string? ApiToken { get; set; }
}

/// <summary>
/// Per-event-type notification preferences.
/// </summary>
public sealed class EventNotificationPreference
{
    [JsonPropertyName("channels")]
    public List<string> Channels { get; set; } = new() { "in_app" };

    [JsonPropertyName("muted")]
    public bool Muted { get; set; }
}
