using Notification.Contracts.DTOs;
using TadHub.SharedKernel.Models;

namespace Notification.Contracts;

public interface IUserNotificationPreferenceService
{
    Task<IReadOnlyList<UserNotificationPreferenceDto>> GetPreferencesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<UserNotificationPreferenceDto>>> BulkUpdateAsync(
        Guid tenantId, Guid userId, BulkUpdateUserPreferencesRequest request, CancellationToken ct = default);

    Task<bool> IsEventMutedForUserAsync(
        Guid tenantId, Guid userId, string eventType, CancellationToken ct = default);
}
