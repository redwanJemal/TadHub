import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { TenantNotificationSettings } from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function getNotificationSettings() {
  return apiClient.get<TenantNotificationSettings>(tenantPath('/settings/notifications'));
}

export function updateNotificationSettings(data: TenantNotificationSettings) {
  return apiClient.put<void>(tenantPath('/settings/notifications'), data);
}
