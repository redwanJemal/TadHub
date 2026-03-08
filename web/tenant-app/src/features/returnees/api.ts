import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  ReturneeCaseDto,
  ReturneeCaseListDto,
  ReturneeCaseStatusHistoryDto,
  ReturneeExpenseDto,
  RefundCalculationDto,
  CreateReturneeCaseRequest,
  ApproveReturneeCaseRequest,
  RejectReturneeCaseRequest,
  SettleReturneeCaseRequest,
  CreateReturneeExpenseRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listReturneeCases(params?: QueryParams) {
  return apiClient.getPaged<ReturneeCaseListDto>(tenantPath('/returnee-cases'), params);
}

export function getReturneeCase(id: string, params?: QueryParams) {
  return apiClient.get<ReturneeCaseDto>(tenantPath(`/returnee-cases/${id}`), params);
}

export function createReturneeCase(data: CreateReturneeCaseRequest) {
  return apiClient.post<ReturneeCaseDto>(tenantPath('/returnee-cases'), data);
}

export function approveReturneeCase(id: string, data: ApproveReturneeCaseRequest) {
  return apiClient.put<ReturneeCaseDto>(tenantPath(`/returnee-cases/${id}/approve`), data);
}

export function rejectReturneeCase(id: string, data: RejectReturneeCaseRequest) {
  return apiClient.put<ReturneeCaseDto>(tenantPath(`/returnee-cases/${id}/reject`), data);
}

export function settleReturneeCase(id: string, data: SettleReturneeCaseRequest) {
  return apiClient.put<ReturneeCaseDto>(tenantPath(`/returnee-cases/${id}/settle`), data);
}

export function addReturneeExpense(caseId: string, data: CreateReturneeExpenseRequest) {
  return apiClient.post<ReturneeExpenseDto>(tenantPath(`/returnee-cases/${caseId}/expenses`), data);
}

export function getRefundCalculation(caseId: string) {
  return apiClient.get<RefundCalculationDto>(tenantPath(`/returnee-cases/${caseId}/refund-calculation`));
}

export function getReturneeCaseStatusHistory(id: string) {
  return apiClient.get<ReturneeCaseStatusHistoryDto[]>(tenantPath(`/returnee-cases/${id}/status-history`));
}

export function deleteReturneeCase(id: string) {
  return apiClient.delete<void>(tenantPath(`/returnee-cases/${id}`));
}
