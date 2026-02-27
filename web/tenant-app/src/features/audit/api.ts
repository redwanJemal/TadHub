import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type { AuditEventDto, AuditLogDto } from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listAuditEvents(params?: QueryParams) {
  return apiClient.getPaged<AuditEventDto>(tenantPath('/audit/events'), params);
}

export function listAuditLogs(params?: QueryParams) {
  return apiClient.getPaged<AuditLogDto>(tenantPath('/audit/logs'), params);
}
