import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateTrialRequest,
  CompleteTrialRequest,
  CancelTrialRequest,
} from './types';

const TRIALS_KEY = 'trials';

export function useTrials(params?: QueryParams) {
  return useQuery({
    queryKey: [TRIALS_KEY, params],
    queryFn: () => api.listTrials(params),
  });
}

export function useTrial(id: string) {
  return useQuery({
    queryKey: [TRIALS_KEY, id],
    queryFn: () => api.getTrial(id, { include: 'statusHistory' }),
    enabled: !!id,
  });
}

export function useTrialStatusHistory(id: string) {
  return useQuery({
    queryKey: [TRIALS_KEY, id, 'status-history'],
    queryFn: () => api.getTrialStatusHistory(id),
    enabled: !!id,
  });
}

export function useCreateTrial() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTrialRequest) => api.createTrial(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TRIALS_KEY] });
    },
  });
}

export function useCompleteTrial() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CompleteTrialRequest }) =>
      api.completeTrial(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TRIALS_KEY] });
    },
  });
}

export function useCancelTrial() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CancelTrialRequest }) =>
      api.cancelTrial(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TRIALS_KEY] });
    },
  });
}

export function useDeleteTrial() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteTrial(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [TRIALS_KEY] });
    },
  });
}
