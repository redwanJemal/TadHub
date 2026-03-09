import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  ArrivalDto,
  ArrivalListDto,
  ArrivalStatusHistoryDto,
  ScheduleArrivalRequest,
  UpdateArrivalRequest,
  AssignDriverRequest,
  ConfirmArrivalRequest,
  ConfirmPickupRequest,
  ConfirmAccommodationRequest,
  ConfirmCustomerPickupRequest,
  ReportNoShowRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listArrivals(params?: QueryParams) {
  return apiClient.getPaged<ArrivalListDto>(tenantPath('/arrivals'), params);
}

export function getArrival(id: string, params?: QueryParams) {
  return apiClient.get<ArrivalDto>(tenantPath(`/arrivals/${id}`), params);
}

export function scheduleArrival(data: ScheduleArrivalRequest) {
  return apiClient.post<ArrivalDto>(tenantPath('/arrivals'), data);
}

export function updateArrival(id: string, data: UpdateArrivalRequest) {
  return apiClient.patch<ArrivalDto>(tenantPath(`/arrivals/${id}`), data);
}

export function assignDriver(id: string, data: AssignDriverRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/assign-driver`), data);
}

export function confirmArrival(id: string, data: ConfirmArrivalRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/confirm-arrival`), data);
}

export function confirmPickup(id: string, data: ConfirmPickupRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/confirm-pickup`), data);
}

export function confirmAccommodation(id: string, data: ConfirmAccommodationRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/confirm-accommodation`), data);
}

export function confirmCustomerPickup(id: string, data: ConfirmCustomerPickupRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/confirm-customer-pickup`), data);
}

export function reportNoShow(id: string, data: ReportNoShowRequest) {
  return apiClient.put<ArrivalDto>(tenantPath(`/arrivals/${id}/report-no-show`), data);
}

export function deleteArrival(id: string) {
  return apiClient.delete<void>(tenantPath(`/arrivals/${id}`));
}

export function getArrivalStatusHistory(id: string) {
  return apiClient.get<ArrivalStatusHistoryDto[]>(tenantPath(`/arrivals/${id}/status-history`));
}

// Driver-scoped endpoints
export function listMyPickups(params?: QueryParams) {
  return apiClient.getPaged<ArrivalListDto>(tenantPath('/arrivals/my-pickups'), params);
}

export function uploadPickupPhoto(id: string, file: File) {
  return apiClient.uploadFile<ArrivalDto>(tenantPath(`/arrivals/${id}/upload-pickup-photo`), file);
}
