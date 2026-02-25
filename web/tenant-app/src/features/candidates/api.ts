import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  CandidateDto,
  CandidateListDto,
  CandidateStatusHistoryDto,
  CreateCandidateRequest,
  UpdateCandidateRequest,
  TransitionStatusRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listCandidates(params?: QueryParams) {
  return apiClient.getPaged<CandidateListDto>(tenantPath('/candidates'), params);
}

export function getCandidate(id: string, params?: QueryParams) {
  return apiClient.get<CandidateDto>(tenantPath(`/candidates/${id}`), params);
}

export function createCandidate(data: CreateCandidateRequest) {
  return apiClient.post<CandidateDto>(tenantPath('/candidates'), data);
}

export function updateCandidate(id: string, data: UpdateCandidateRequest) {
  return apiClient.patch<CandidateDto>(tenantPath(`/candidates/${id}`), data);
}

export function transitionStatus(id: string, data: TransitionStatusRequest) {
  return apiClient.post<CandidateDto>(tenantPath(`/candidates/${id}/status`), data);
}

export function getStatusHistory(id: string) {
  return apiClient.get<CandidateStatusHistoryDto[]>(tenantPath(`/candidates/${id}/status-history`));
}

export function deleteCandidate(id: string) {
  return apiClient.delete<void>(tenantPath(`/candidates/${id}`));
}
