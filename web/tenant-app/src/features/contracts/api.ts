import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  ContractDto,
  ContractListDto,
  ContractStatusHistoryDto,
  CreateContractRequest,
  UpdateContractRequest,
  TransitionContractStatusRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listContracts(params?: QueryParams) {
  return apiClient.getPaged<ContractListDto>(tenantPath('/contracts'), params);
}

export function getContract(id: string, params?: QueryParams) {
  return apiClient.get<ContractDto>(tenantPath(`/contracts/${id}`), params);
}

export function createContract(data: CreateContractRequest) {
  return apiClient.post<ContractDto>(tenantPath('/contracts'), data);
}

export function updateContract(id: string, data: UpdateContractRequest) {
  return apiClient.patch<ContractDto>(tenantPath(`/contracts/${id}`), data);
}

export function transitionContractStatus(id: string, data: TransitionContractStatusRequest) {
  return apiClient.post<ContractDto>(tenantPath(`/contracts/${id}/status`), data);
}

export function getContractStatusHistory(id: string) {
  return apiClient.get<ContractStatusHistoryDto[]>(tenantPath(`/contracts/${id}/status-history`));
}

export function deleteContract(id: string) {
  return apiClient.delete<void>(tenantPath(`/contracts/${id}`));
}
