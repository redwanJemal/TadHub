namespace TadHub.Infrastructure.Settings;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "noreply@saaskit.dev";
    public string FromName { get; init; } = "SaaS Kit";
    public bool EnableSsl { get; init; } = false;
}
