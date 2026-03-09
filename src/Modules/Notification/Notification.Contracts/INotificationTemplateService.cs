using Notification.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Notification.Contracts;

public interface INotificationTemplateService
{
    Task<PagedList<NotificationTemplateListDto>> ListAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<NotificationTemplateDto>> GetByIdAsync(
        Guid tenantId, Guid templateId, CancellationToken ct = default);

    Task<Result<NotificationTemplateDto>> GetByEventTypeAsync(
        Guid tenantId, string eventType, CancellationToken ct = default);

    Task<Result<NotificationTemplateDto>> CreateAsync(
        Guid tenantId, CreateNotificationTemplateRequest request, CancellationToken ct = default);

    Task<Result<NotificationTemplateDto>> UpdateAsync(
        Guid tenantId, Guid templateId, UpdateNotificationTemplateRequest request, CancellationToken ct = default);

    Task<Result<bool>> DeleteAsync(
        Guid tenantId, Guid templateId, CancellationToken ct = default);
}
