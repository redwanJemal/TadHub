import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  RunawayCaseDto,
  RunawayCaseListDto,
  RunawayCaseStatusHistoryDto,
  RunawayExpenseDto,
  ReportRunawayCaseRequest,
  UpdateRunawayCaseRequest,
  ConfirmRunawayCaseRequest,
  SettleRunawayCaseRequest,
  CloseRunawayCaseRequest,
  CreateRunawayExpenseRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listRunawayCases(params?: QueryParams) {
  return apiClient.getPaged<RunawayCaseListDto>(tenantPath('/runaway-cases'), params);
}

export function getRunawayCase(id: string, params?: QueryParams) {
  return apiClient.get<RunawayCaseDto>(tenantPath(`/runaway-cases/${id}`), params);
}

export function reportRunawayCase(data: ReportRunawayCaseRequest) {
  return apiClient.post<RunawayCaseDto>(tenantPath('/runaway-cases'), data);
}

export function updateRunawayCase(id: string, data: UpdateRunawayCaseRequest) {
  return apiClient.patch<RunawayCaseDto>(tenantPath(`/runaway-cases/${id}`), data);
}

export function confirmRunawayCase(id: string, data: ConfirmRunawayCaseRequest) {
  return apiClient.put<RunawayCaseDto>(tenantPath(`/runaway-cases/${id}/confirm`), data);
}

export function settleRunawayCase(id: string, data: SettleRunawayCaseRequest) {
  return apiClient.put<RunawayCaseDto>(tenantPath(`/runaway-cases/${id}/settle`), data);
}

export function closeRunawayCase(id: string, data: CloseRunawayCaseRequest) {
  return apiClient.put<RunawayCaseDto>(tenantPath(`/runaway-cases/${id}/close`), data);
}

export function addRunawayExpense(caseId: string, data: CreateRunawayExpenseRequest) {
  return apiClient.post<RunawayExpenseDto>(tenantPath(`/runaway-cases/${caseId}/expenses`), data);
}

export function getRunawayCaseStatusHistory(id: string) {
  return apiClient.get<RunawayCaseStatusHistoryDto[]>(tenantPath(`/runaway-cases/${id}/status-history`));
}

export function deleteRunawayCase(id: string) {
  return apiClient.delete<void>(tenantPath(`/runaway-cases/${id}`));
}
