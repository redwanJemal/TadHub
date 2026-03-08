import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateReturneeCaseRequest,
  ApproveReturneeCaseRequest,
  RejectReturneeCaseRequest,
  SettleReturneeCaseRequest,
  CreateReturneeExpenseRequest,
} from './types';

const RETURNEES_KEY = 'returnee-cases';

export function useReturneeCases(params?: QueryParams) {
  return useQuery({
    queryKey: [RETURNEES_KEY, params],
    queryFn: () => api.listReturneeCases(params),
  });
}

export function useReturneeCase(id: string) {
  return useQuery({
    queryKey: [RETURNEES_KEY, id],
    queryFn: () => api.getReturneeCase(id, { include: 'statusHistory,expenses' }),
    enabled: !!id,
  });
}

export function useRefundCalculation(caseId: string, enabled: boolean) {
  return useQuery({
    queryKey: [RETURNEES_KEY, caseId, 'refund'],
    queryFn: () => api.getRefundCalculation(caseId),
    enabled: !!caseId && enabled,
  });
}

export function useCreateReturneeCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateReturneeCaseRequest) => api.createReturneeCase(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}

export function useApproveReturneeCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ApproveReturneeCaseRequest }) =>
      api.approveReturneeCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}

export function useRejectReturneeCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: RejectReturneeCaseRequest }) =>
      api.rejectReturneeCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}

export function useSettleReturneeCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SettleReturneeCaseRequest }) =>
      api.settleReturneeCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}

export function useAddReturneeExpense() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ caseId, data }: { caseId: string; data: CreateReturneeExpenseRequest }) =>
      api.addReturneeExpense(caseId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}

export function useDeleteReturneeCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteReturneeCase(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RETURNEES_KEY] });
    },
  });
}
