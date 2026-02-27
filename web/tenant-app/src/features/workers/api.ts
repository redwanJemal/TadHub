import { apiClient, API_BASE } from '@/shared/api/client';
import { getTenantId, getAccessToken } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  WorkerDto,
  WorkerListDto,
  WorkerStatusHistoryDto,
  UpdateWorkerRequest,
  TransitionWorkerStatusRequest,
  WorkerCvDto,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listWorkers(params?: QueryParams) {
  return apiClient.getPaged<WorkerListDto>(tenantPath('/workers'), params);
}

export function getWorker(id: string, params?: QueryParams) {
  return apiClient.get<WorkerDto>(tenantPath(`/workers/${id}`), params);
}

export function updateWorker(id: string, data: UpdateWorkerRequest) {
  return apiClient.patch<WorkerDto>(tenantPath(`/workers/${id}`), data);
}

export function transitionWorkerStatus(id: string, data: TransitionWorkerStatusRequest) {
  return apiClient.post<WorkerDto>(tenantPath(`/workers/${id}/status`), data);
}

export function getWorkerStatusHistory(id: string) {
  return apiClient.get<WorkerStatusHistoryDto[]>(tenantPath(`/workers/${id}/status-history`));
}

export function getWorkerCv(id: string) {
  return apiClient.get<WorkerCvDto>(tenantPath(`/workers/${id}/cv`));
}

export function deleteWorker(id: string) {
  return apiClient.delete<void>(tenantPath(`/workers/${id}`));
}

export async function downloadWorkerCvPdf(id: string): Promise<Blob> {
  const token = getAccessToken();
  const tenantId = getTenantId();
  const url = `${API_BASE}/tenants/${tenantId}/workers/${id}/cv/pdf`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(tenantId ? { 'X-Tenant-ID': tenantId } : {}),
    },
  });

  if (!response.ok) {
    throw new Error(`Failed to download PDF: ${response.status}`);
  }

  return response.blob();
}
