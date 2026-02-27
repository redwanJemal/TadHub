import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateContractRequest,
  UpdateContractRequest,
  TransitionContractStatusRequest,
} from './types';

const CONTRACTS_KEY = 'contracts';

export function useContracts(params?: QueryParams) {
  return useQuery({
    queryKey: [CONTRACTS_KEY, params],
    queryFn: () => api.listContracts(params),
  });
}

export function useContract(id: string) {
  return useQuery({
    queryKey: [CONTRACTS_KEY, id],
    queryFn: () => api.getContract(id, { include: 'statusHistory' }),
    enabled: !!id,
  });
}

export function useContractStatusHistory(id: string) {
  return useQuery({
    queryKey: [CONTRACTS_KEY, id, 'status-history'],
    queryFn: () => api.getContractStatusHistory(id),
    enabled: !!id,
  });
}

export function useCreateContract() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateContractRequest) => api.createContract(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CONTRACTS_KEY] });
    },
  });
}

export function useUpdateContract() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateContractRequest }) =>
      api.updateContract(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CONTRACTS_KEY] });
    },
  });
}

export function useTransitionContractStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionContractStatusRequest }) =>
      api.transitionContractStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CONTRACTS_KEY] });
    },
  });
}

export function useDeleteContract() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteContract(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CONTRACTS_KEY] });
    },
  });
}
