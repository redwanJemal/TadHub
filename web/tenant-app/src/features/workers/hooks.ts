import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  UpdateWorkerRequest,
  TransitionWorkerStatusRequest,
} from './types';

const WORKERS_KEY = 'workers';

export function useWorkers(params?: QueryParams) {
  return useQuery({
    queryKey: [WORKERS_KEY, params],
    queryFn: () => api.listWorkers({ ...params, include: 'supplier' }),
  });
}

export function useWorker(id: string) {
  return useQuery({
    queryKey: [WORKERS_KEY, id],
    queryFn: () => api.getWorker(id, { include: 'statusHistory,supplier' }),
    enabled: !!id,
  });
}

export function useWorkerStatusHistory(id: string) {
  return useQuery({
    queryKey: [WORKERS_KEY, id, 'status-history'],
    queryFn: () => api.getWorkerStatusHistory(id),
    enabled: !!id,
  });
}

export function useUpdateWorker() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateWorkerRequest }) =>
      api.updateWorker(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKERS_KEY] });
    },
  });
}

export function useTransitionWorkerStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionWorkerStatusRequest }) =>
      api.transitionWorkerStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKERS_KEY] });
    },
  });
}

export function useDeleteWorker() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteWorker(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKERS_KEY] });
    },
  });
}
