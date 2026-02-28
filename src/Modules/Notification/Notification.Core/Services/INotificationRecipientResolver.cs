using Notification.Contracts.Channels;

namespace Notification.Core.Services;

/// <summary>
/// Resolves notification recipients for a tenant.
/// </summary>
public interface INotificationRecipientResolver
{
    /// <summary>
    /// Gets all active tenant members as notification recipients.
    /// </summary>
    Task<IReadOnlyList<RecipientInfo>> GetAllMembersAsync(Guid tenantId, CancellationToken ct = default);
}
