using Microsoft.Extensions.Logging;
using Notification.Contracts.Channels;
using TadHub.SharedKernel.Api;
using Tenancy.Contracts;

namespace Notification.Core.Services;

/// <summary>
/// Resolves notification recipients by querying tenant members.
/// Initial implementation: all active members. Future: filter by notification permissions.
/// </summary>
public sealed class NotificationRecipientResolver : INotificationRecipientResolver
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<NotificationRecipientResolver> _logger;

    public NotificationRecipientResolver(
        ITenantService tenantService,
        ILogger<NotificationRecipientResolver> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RecipientInfo>> GetAllMembersAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var members = await _tenantService.GetMembersAsync(
                tenantId,
                new QueryParameters { PageSize = 500 },
                ct);

            return members.Items
                .Select(m => new RecipientInfo
                {
                    UserId = m.UserId,
                    Email = m.Email
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve notification recipients for tenant {TenantId}", tenantId);
            return Array.Empty<RecipientInfo>();
        }
    }
}
