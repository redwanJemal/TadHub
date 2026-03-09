import { apiClient } from '@/shared/api/client';
import { getTenantId } from '@/features/auth/AuthProvider';
import type { QueryParams } from '@/shared/api/types/common';
import type {
  PlacementDto,
  PlacementListDto,
  PlacementStatusHistoryDto,
  PlacementCostItemDto,
  PlacementBoardDto,
  PlacementChecklistDto,
  CreatePlacementRequest,
  UpdatePlacementRequest,
  TransitionPlacementStatusRequest,
  AdvancePlacementStepRequest,
  CreatePlacementCostItemRequest,
  UpdatePlacementCostItemRequest,
} from './types';

function tenantPath(path: string) {
  const tenantId = getTenantId();
  return `/tenants/${tenantId}${path}`;
}

export function listPlacements(params?: QueryParams) {
  return apiClient.getPaged<PlacementListDto>(tenantPath('/placements'), params);
}

export function getPlacement(id: string, params?: QueryParams) {
  return apiClient.get<PlacementDto>(tenantPath(`/placements/${id}`), params);
}

export function createPlacement(data: CreatePlacementRequest) {
  return apiClient.post<PlacementDto>(tenantPath('/placements'), data);
}

export function updatePlacement(id: string, data: UpdatePlacementRequest) {
  return apiClient.patch<PlacementDto>(tenantPath(`/placements/${id}`), data);
}

export function transitionPlacementStatus(id: string, data: TransitionPlacementStatusRequest) {
  return apiClient.post<PlacementDto>(tenantPath(`/placements/${id}/transition`), data);
}

export function advancePlacementStep(id: string, data: AdvancePlacementStepRequest) {
  return apiClient.post<PlacementDto>(tenantPath(`/placements/${id}/advance-step`), data);
}

export function getPlacementChecklist(id: string) {
  return apiClient.get<PlacementChecklistDto>(tenantPath(`/placements/${id}/checklist`));
}

export function getPlacementStatusHistory(id: string) {
  return apiClient.get<PlacementStatusHistoryDto[]>(tenantPath(`/placements/${id}/status-history`));
}

export function getPlacementBoard() {
  return apiClient.get<PlacementBoardDto>(tenantPath('/placements/board'));
}

export function deletePlacement(id: string) {
  return apiClient.delete<void>(tenantPath(`/placements/${id}`));
}

// Cost items
export function addPlacementCostItem(placementId: string, data: CreatePlacementCostItemRequest) {
  return apiClient.post<PlacementCostItemDto>(tenantPath(`/placements/${placementId}/cost-items`), data);
}

export function updatePlacementCostItem(placementId: string, itemId: string, data: UpdatePlacementCostItemRequest) {
  return apiClient.patch<PlacementCostItemDto>(tenantPath(`/placements/${placementId}/cost-items/${itemId}`), data);
}

export function deletePlacementCostItem(placementId: string, itemId: string) {
  return apiClient.delete<void>(tenantPath(`/placements/${placementId}/cost-items/${itemId}`));
}
