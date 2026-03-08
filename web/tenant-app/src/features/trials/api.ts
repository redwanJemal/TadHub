import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  TrialDto,
  TrialListDto,
  TrialStatusHistoryDto,
  CreateTrialRequest,
  CompleteTrialRequest,
  CancelTrialRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listTrials(params?: QueryParams) {
  return apiClient.getPaged<TrialListDto>(tenantPath('/trials'), params);
}

export function getTrial(id: string, params?: QueryParams) {
  return apiClient.get<TrialDto>(tenantPath(`/trials/${id}`), params);
}

export function createTrial(data: CreateTrialRequest) {
  return apiClient.post<TrialDto>(tenantPath('/trials'), data);
}

export function completeTrial(id: string, data: CompleteTrialRequest) {
  return apiClient.put<TrialDto>(tenantPath(`/trials/${id}/complete`), data);
}

export function cancelTrial(id: string, data: CancelTrialRequest) {
  return apiClient.put<TrialDto>(tenantPath(`/trials/${id}/cancel`), data);
}

export function getTrialStatusHistory(id: string) {
  return apiClient.get<TrialStatusHistoryDto[]>(tenantPath(`/trials/${id}/status-history`));
}

export function deleteTrial(id: string) {
  return apiClient.delete<void>(tenantPath(`/trials/${id}`));
}
