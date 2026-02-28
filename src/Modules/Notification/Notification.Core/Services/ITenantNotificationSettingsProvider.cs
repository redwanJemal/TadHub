using Notification.Contracts.Settings;

namespace Notification.Core.Services;

/// <summary>
/// Reads tenant notification settings from the Tenant.Settings JSONB field.
/// </summary>
public interface ITenantNotificationSettingsProvider
{
    Task<TenantNotificationSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
}
