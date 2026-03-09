import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreatePlacementRequest,
  UpdatePlacementRequest,
  TransitionPlacementStatusRequest,
  AdvancePlacementStepRequest,
  CreatePlacementCostItemRequest,
  UpdatePlacementCostItemRequest,
} from './types';

const PLACEMENTS_KEY = 'placements';

export function usePlacements(params?: QueryParams) {
  return useQuery({
    queryKey: [PLACEMENTS_KEY, params],
    queryFn: () => api.listPlacements(params),
  });
}

export function usePlacement(id: string) {
  return useQuery({
    queryKey: [PLACEMENTS_KEY, id],
    queryFn: () => api.getPlacement(id, { include: 'statusHistory,costItems,checklist' }),
    enabled: !!id,
  });
}

export function usePlacementBoard() {
  return useQuery({
    queryKey: [PLACEMENTS_KEY, 'board'],
    queryFn: () => api.getPlacementBoard(),
    refetchInterval: 30000, // Refresh board every 30 seconds
  });
}

export function usePlacementChecklist(id: string) {
  return useQuery({
    queryKey: [PLACEMENTS_KEY, id, 'checklist'],
    queryFn: () => api.getPlacementChecklist(id),
    enabled: !!id,
  });
}

export function usePlacementStatusHistory(id: string) {
  return useQuery({
    queryKey: [PLACEMENTS_KEY, id, 'status-history'],
    queryFn: () => api.getPlacementStatusHistory(id),
    enabled: !!id,
  });
}

export function useCreatePlacement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreatePlacementRequest) => api.createPlacement(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useUpdatePlacement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePlacementRequest }) =>
      api.updatePlacement(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useTransitionPlacementStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionPlacementStatusRequest }) =>
      api.transitionPlacementStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useAdvancePlacementStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AdvancePlacementStepRequest }) =>
      api.advancePlacementStep(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useDeletePlacement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deletePlacement(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

// Cost items
export function useAddPlacementCostItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ placementId, data }: { placementId: string; data: CreatePlacementCostItemRequest }) =>
      api.addPlacementCostItem(placementId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useUpdatePlacementCostItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ placementId, itemId, data }: { placementId: string; itemId: string; data: UpdatePlacementCostItemRequest }) =>
      api.updatePlacementCostItem(placementId, itemId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}

export function useDeletePlacementCostItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ placementId, itemId }: { placementId: string; itemId: string }) =>
      api.deletePlacementCostItem(placementId, itemId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [PLACEMENTS_KEY] });
    },
  });
}
