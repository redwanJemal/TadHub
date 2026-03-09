import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type { CheckInRequest, CheckOutRequest, UpdateStayRequest } from './types';

const ACCOMMODATIONS_KEY = 'accommodations';

export function useAccommodations(params?: QueryParams) {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, 'list', params],
    queryFn: () => api.listAccommodations(params),
  });
}

export function useAccommodation(id: string) {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, id],
    queryFn: () => api.getAccommodation(id),
    enabled: !!id,
  });
}

export function useCurrentOccupants(params?: QueryParams) {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, 'current', params],
    queryFn: () => api.getCurrentOccupants(params),
  });
}

export function useDailyList(date: string, params?: QueryParams) {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, 'daily', date, params],
    queryFn: () => api.getDailyList(date, params),
    enabled: !!date,
  });
}

export function useWorkerStayHistory(workerId: string, params?: QueryParams) {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, 'history', workerId, params],
    queryFn: () => api.getWorkerHistory(workerId, params),
    enabled: !!workerId,
  });
}

export function useAccommodationCounts() {
  return useQuery({
    queryKey: [ACCOMMODATIONS_KEY, 'counts'],
    queryFn: () => api.getCounts(),
  });
}

export function useCheckIn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CheckInRequest) => api.checkIn(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ACCOMMODATIONS_KEY] });
    },
  });
}

export function useCheckOut() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CheckOutRequest }) =>
      api.checkOut(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ACCOMMODATIONS_KEY] });
    },
  });
}

export function useUpdateStay() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStayRequest }) =>
      api.updateStay(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ACCOMMODATIONS_KEY] });
    },
  });
}

export function useDeleteStay() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteStay(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [ACCOMMODATIONS_KEY] });
    },
  });
}
