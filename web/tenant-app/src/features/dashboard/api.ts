import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { DashboardSummary } from '@/shared/api/types/dashboard';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function getDashboardSummary() {
  return apiClient.get<DashboardSummary>(tenantPath('/dashboard'));
}
