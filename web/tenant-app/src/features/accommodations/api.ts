import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  AccommodationStayDto,
  AccommodationStayListDto,
  CheckInRequest,
  CheckOutRequest,
  UpdateStayRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listAccommodations(params?: QueryParams) {
  return apiClient.getPaged<AccommodationStayListDto>(tenantPath('/accommodations'), params);
}

export function getAccommodation(id: string) {
  return apiClient.get<AccommodationStayDto>(tenantPath(`/accommodations/${id}`));
}

export function checkIn(data: CheckInRequest) {
  return apiClient.post<AccommodationStayDto>(tenantPath('/accommodations/check-in'), data);
}

export function checkOut(id: string, data: CheckOutRequest) {
  return apiClient.put<AccommodationStayDto>(tenantPath(`/accommodations/${id}/check-out`), data);
}

export function updateStay(id: string, data: UpdateStayRequest) {
  return apiClient.patch<AccommodationStayDto>(tenantPath(`/accommodations/${id}`), data);
}

export function deleteStay(id: string) {
  return apiClient.delete<void>(tenantPath(`/accommodations/${id}`));
}

export function getCurrentOccupants(params?: QueryParams) {
  return apiClient.getPaged<AccommodationStayListDto>(tenantPath('/accommodations/current'), params);
}

export function getDailyList(date: string, params?: QueryParams) {
  return apiClient.getPaged<AccommodationStayListDto>(tenantPath(`/accommodations/daily-list?date=${date}`), params);
}

export function getWorkerHistory(workerId: string, params?: QueryParams) {
  return apiClient.getPaged<AccommodationStayListDto>(tenantPath(`/accommodations/history?workerId=${workerId}`), params);
}

export function getCounts() {
  return apiClient.get<Record<string, number>>(tenantPath('/accommodations/counts'));
}
