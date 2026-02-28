import { apiClient, API_BASE } from '@/shared/api/client';
import { getTenantId, getAccessToken } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  WorkerDocumentDto,
  WorkerDocumentListDto,
  CreateWorkerDocumentRequest,
  UpdateWorkerDocumentRequest,
  ComplianceSummaryDto,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

// ── Worker-scoped document endpoints ──

export function listWorkerDocuments(workerId: string, params?: QueryParams) {
  return apiClient.getPaged<WorkerDocumentListDto>(tenantPath(`/workers/${workerId}/documents`), params);
}

export function getWorkerDocument(workerId: string, id: string) {
  return apiClient.get<WorkerDocumentDto>(tenantPath(`/workers/${workerId}/documents/${id}`));
}

export function createWorkerDocument(workerId: string, data: Omit<CreateWorkerDocumentRequest, 'workerId'>) {
  return apiClient.post<WorkerDocumentDto>(tenantPath(`/workers/${workerId}/documents`), {
    ...data,
    workerId,
  });
}

export function updateWorkerDocument(workerId: string, id: string, data: UpdateWorkerDocumentRequest) {
  return apiClient.patch<WorkerDocumentDto>(tenantPath(`/workers/${workerId}/documents/${id}`), data);
}

export function deleteWorkerDocument(workerId: string, id: string) {
  return apiClient.delete<void>(tenantPath(`/workers/${workerId}/documents/${id}`));
}

export async function uploadDocumentFile(workerId: string, id: string, file: File): Promise<WorkerDocumentDto> {
  const token = getAccessToken();
  const tenantId = getTenantId();
  const url = `${API_BASE}/tenants/${tenantId}/workers/${workerId}/documents/${id}/file`;

  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(url, {
    method: 'POST',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(tenantId ? { 'X-Tenant-ID': tenantId } : {}),
    },
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Failed to upload file: ${response.status}`);
  }

  const json = await response.json();
  return json.data ?? json;
}

// ── Tenant-wide document endpoints ──

export function listAllDocuments(params?: QueryParams) {
  return apiClient.getPaged<WorkerDocumentListDto>(tenantPath('/documents'), params);
}

export function getExpiringDocuments(days: number = 30, params?: QueryParams) {
  return apiClient.getPaged<WorkerDocumentListDto>(tenantPath('/documents/expiring'), {
    ...params,
    days,
  });
}

export function getComplianceSummary() {
  return apiClient.get<ComplianceSummaryDto>(tenantPath('/documents/compliance'));
}
