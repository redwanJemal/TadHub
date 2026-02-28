using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Contracts.Settings;
using TadHub.Infrastructure.Persistence;

namespace Notification.Core.Services;

/// <summary>
/// Reads notification settings from the Tenant entity's Settings JSONB column.
/// Queries AppDbContext directly since TenantDto doesn't expose Settings.
/// </summary>
public sealed class TenantNotificationSettingsProvider : ITenantNotificationSettingsProvider
{
    private readonly AppDbContext _db;
    private readonly ILogger<TenantNotificationSettingsProvider> _logger;

    public TenantNotificationSettingsProvider(
        AppDbContext db,
        ILogger<TenantNotificationSettingsProvider> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TenantNotificationSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            // Query the settings JSON string directly from the tenant table
            var settingsJson = await _db.Database
                .SqlQuery<string?>($"SELECT settings FROM tenants WHERE id = {tenantId} LIMIT 1")
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrEmpty(settingsJson)) return null;

            var allSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(settingsJson);
            if (allSettings == null || !allSettings.TryGetValue("notifications", out var notificationsElement))
                return null;

            return JsonSerializer.Deserialize<TenantNotificationSettings>(notificationsElement.GetRawText());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read notification settings for tenant {TenantId}", tenantId);
            return null;
        }
    }
}
