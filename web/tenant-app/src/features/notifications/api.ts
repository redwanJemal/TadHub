import { apiClient } from '@/shared/api/client';
import type { NotificationDto, UserNotificationPreferenceDto, BulkUpdateUserPreferencesRequest } from './types';

const BASE = (tenantId: string) => `/tenants/${tenantId}/notifications`;
const PREFS_BASE = (tenantId: string) => `/tenants/${tenantId}/notification-preferences`;

export function listNotifications(tenantId: string, params?: Record<string, unknown>) {
  return apiClient.getPaged<NotificationDto>(BASE(tenantId), params);
}

export function getUnreadCount(tenantId: string) {
  return apiClient.get<{ count: number }>(`${BASE(tenantId)}/unread-count`);
}

export function markAsRead(tenantId: string, notificationId: string) {
  return apiClient.post<NotificationDto>(`${BASE(tenantId)}/${notificationId}/read`);
}

export function markAllAsRead(tenantId: string) {
  return apiClient.post<{ count: number }>(`${BASE(tenantId)}/read-all`);
}

export function deleteNotification(tenantId: string, notificationId: string) {
  return apiClient.delete<void>(`${BASE(tenantId)}/${notificationId}`);
}

export function getNotificationPreferences(tenantId: string) {
  return apiClient.get<UserNotificationPreferenceDto[]>(PREFS_BASE(tenantId));
}

export function updateNotificationPreferences(tenantId: string, request: BulkUpdateUserPreferencesRequest) {
  return apiClient.put<UserNotificationPreferenceDto[]>(PREFS_BASE(tenantId), request);
}
