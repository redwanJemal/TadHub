import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  ReportRunawayCaseRequest,
  UpdateRunawayCaseRequest,
  ConfirmRunawayCaseRequest,
  SettleRunawayCaseRequest,
  CloseRunawayCaseRequest,
  CreateRunawayExpenseRequest,
} from './types';

const RUNAWAYS_KEY = 'runaway-cases';

export function useRunawayCases(params?: QueryParams) {
  return useQuery({
    queryKey: [RUNAWAYS_KEY, params],
    queryFn: () => api.listRunawayCases(params),
  });
}

export function useRunawayCase(id: string) {
  return useQuery({
    queryKey: [RUNAWAYS_KEY, id],
    queryFn: () => api.getRunawayCase(id, { include: 'statusHistory,expenses' }),
    enabled: !!id,
  });
}

export function useReportRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ReportRunawayCaseRequest) => api.reportRunawayCase(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useUpdateRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRunawayCaseRequest }) =>
      api.updateRunawayCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useConfirmRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ConfirmRunawayCaseRequest }) =>
      api.confirmRunawayCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useSettleRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SettleRunawayCaseRequest }) =>
      api.settleRunawayCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useCloseRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CloseRunawayCaseRequest }) =>
      api.closeRunawayCase(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useAddRunawayExpense() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ caseId, data }: { caseId: string; data: CreateRunawayExpenseRequest }) =>
      api.addRunawayExpense(caseId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}

export function useDeleteRunawayCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteRunawayCase(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [RUNAWAYS_KEY] });
    },
  });
}
