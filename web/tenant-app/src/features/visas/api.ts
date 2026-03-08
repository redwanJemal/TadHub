import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  VisaApplicationDto,
  VisaApplicationListDto,
  VisaApplicationStatusHistoryDto,
  VisaApplicationDocumentDto,
  CreateVisaApplicationRequest,
  UpdateVisaApplicationRequest,
  TransitionVisaStatusRequest,
  UploadVisaDocumentRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listVisaApplications(params?: QueryParams) {
  return apiClient.getPaged<VisaApplicationListDto>(tenantPath('/visa-applications'), params);
}

export function getVisaApplication(id: string, params?: QueryParams) {
  return apiClient.get<VisaApplicationDto>(tenantPath(`/visa-applications/${id}`), params);
}

export function createVisaApplication(data: CreateVisaApplicationRequest) {
  return apiClient.post<VisaApplicationDto>(tenantPath('/visa-applications'), data);
}

export function updateVisaApplication(id: string, data: UpdateVisaApplicationRequest) {
  return apiClient.patch<VisaApplicationDto>(tenantPath(`/visa-applications/${id}`), data);
}

export function transitionVisaStatus(id: string, data: TransitionVisaStatusRequest) {
  return apiClient.post<VisaApplicationDto>(tenantPath(`/visa-applications/${id}/transition`), data);
}

export function getVisaStatusHistory(id: string) {
  return apiClient.get<VisaApplicationStatusHistoryDto[]>(tenantPath(`/visa-applications/${id}/status-history`));
}

export function uploadVisaDocument(id: string, data: UploadVisaDocumentRequest) {
  return apiClient.post<VisaApplicationDocumentDto>(tenantPath(`/visa-applications/${id}/documents`), data);
}

export function deleteVisaApplication(id: string) {
  return apiClient.delete<void>(tenantPath(`/visa-applications/${id}`));
}

export function getVisaApplicationsByWorker(workerId: string) {
  return apiClient.get<VisaApplicationListDto[]>(tenantPath(`/visa-applications/by-worker/${workerId}`));
}
