import { apiClient, API_BASE } from '@/shared/api/client';
import { getTenantId, getAccessToken } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  CandidateDto,
  CandidateListDto,
  CandidateStatusHistoryDto,
  CreateCandidateRequest,
  UpdateCandidateRequest,
  TransitionStatusRequest,
  TenantFileDto,
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

async function authenticatedUpload<T>(path: string, file: File, errorMsg: string): Promise<T> {
  const tenantId = getTenantId();
  const formData = new FormData();
  formData.append('file', file);

  const token = getAccessToken();
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      'X-Tenant-ID': tenantId || '',
    },
    body: formData,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || errorMsg);
  }

  const json = await res.json();
  return json.data ?? json;
}

export function uploadCandidatePhoto(id: string, file: File): Promise<CandidateDto> {
  return authenticatedUpload(tenantPath(`/candidates/${id}/photo`), file, 'Photo upload failed');
}

export function uploadCandidateVideo(id: string, file: File): Promise<CandidateDto> {
  return authenticatedUpload(tenantPath(`/candidates/${id}/video`), file, 'Video upload failed');
}

export function uploadCandidatePassport(id: string, file: File): Promise<CandidateDto> {
  return authenticatedUpload(tenantPath(`/candidates/${id}/passport`), file, 'Passport upload failed');
}

export function uploadFile(file: File, fileType: string): Promise<TenantFileDto> {
  return authenticatedUpload(tenantPath(`/files?fileType=${fileType}`), file, 'File upload failed');
}
