import { apiClient } from '@/shared/api/client';
import { PaginatedData, QueryParams } from '@/shared/api/types';
import {
  NotificationDto,
  AdminSendNotificationRequest,
  AdminSendNotificationResponse,
  TenantNotificationSettings,
  UpdateNotificationSettingsRequest,
} from './types';

/**
 * List all notifications across tenants (admin view)
 */
export async function listNotifications(
  params?: QueryParams,
): Promise<PaginatedData<NotificationDto>> {
  return apiClient.get<PaginatedData<NotificationDto>>('/admin/notifications', params);
}

/**
 * Send notification to tenant members
 */
export async function sendNotification(
  data: AdminSendNotificationRequest,
): Promise<AdminSendNotificationResponse> {
  return apiClient.post<AdminSendNotificationResponse>('/admin/notifications/send', data);
}

/**
 * Get tenant notification settings
 */
export async function getTenantNotificationSettings(
  tenantId: string,
): Promise<TenantNotificationSettings> {
  return apiClient.get<TenantNotificationSettings>(
    `/admin/notifications/tenants/${tenantId}/settings`,
  );
}

/**
 * Update tenant notification settings
 */
export async function updateTenantNotificationSettings(
  tenantId: string,
  data: UpdateNotificationSettingsRequest,
): Promise<{ updated: boolean }> {
  return apiClient.put<{ updated: boolean }>(
    `/admin/notifications/tenants/${tenantId}/settings`,
    data,
  );
}
