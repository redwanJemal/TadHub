import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  SupplierUserDto,
  SupplierDashboardDto,
  SupplierCandidateListDto,
  SupplierWorkerListDto,
  SupplierCommissionDto,
  SupplierArrivalListDto,
  CreateSupplierUserRequest,
  UpdateSupplierUserRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function getSupplierProfile() {
  return apiClient.get<SupplierUserDto>(tenantPath('/supplier-portal/profile'));
}

export function getSupplierDashboard() {
  return apiClient.get<SupplierDashboardDto>(tenantPath('/supplier-portal/dashboard'));
}

export function listSupplierCandidates(params?: QueryParams) {
  return apiClient.getPaged<SupplierCandidateListDto>(tenantPath('/supplier-portal/candidates'), params);
}

export function listSupplierWorkers(params?: QueryParams) {
  return apiClient.getPaged<SupplierWorkerListDto>(tenantPath('/supplier-portal/workers'), params);
}

export function listSupplierCommissions(params?: QueryParams) {
  return apiClient.getPaged<SupplierCommissionDto>(tenantPath('/supplier-portal/commissions'), params);
}

export function listSupplierArrivals(params?: QueryParams) {
  return apiClient.getPaged<SupplierArrivalListDto>(tenantPath('/supplier-portal/arrivals'), params);
}

export function listSupplierUsers(params?: QueryParams) {
  return apiClient.getPaged<SupplierUserDto>(tenantPath('/supplier-portal/users'), params);
}

export function createSupplierUser(data: CreateSupplierUserRequest) {
  return apiClient.post<SupplierUserDto>(tenantPath('/supplier-portal/users'), data);
}

export function updateSupplierUser(id: string, data: UpdateSupplierUserRequest) {
  return apiClient.patch<SupplierUserDto>(tenantPath(`/supplier-portal/users/${id}`), data);
}
