import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  TenantSupplier,
  Supplier,
  CreateSupplierRequest,
  LinkSupplierRequest,
  UpdateTenantSupplierRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

// Tenant-scoped supplier relationships
export function listTenantSuppliers(params?: QueryParams) {
  return apiClient.getPaged<TenantSupplier>(tenantPath('/suppliers'), {
    ...params,
    include: 'supplier',
  });
}

export function getTenantSupplier(id: string) {
  return apiClient.get<TenantSupplier>(tenantPath(`/suppliers/${id}`), {
    include: 'supplier',
  });
}

export function linkSupplier(data: LinkSupplierRequest) {
  return apiClient.post<TenantSupplier>(tenantPath('/suppliers'), data);
}

export function updateTenantSupplier(id: string, data: UpdateTenantSupplierRequest) {
  return apiClient.patch<TenantSupplier>(tenantPath(`/suppliers/${id}`), data);
}

export function unlinkSupplier(id: string) {
  return apiClient.delete<void>(tenantPath(`/suppliers/${id}`));
}

// Create and link a supplier to the current tenant in one step
export function createAndLinkSupplier(data: CreateSupplierRequest) {
  return apiClient.post<TenantSupplier>(tenantPath('/suppliers/create'), data);
}

export function listSuppliers(params?: QueryParams) {
  return apiClient.getPaged<Supplier>('/suppliers', params);
}
