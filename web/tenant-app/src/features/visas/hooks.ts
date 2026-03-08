import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateVisaApplicationRequest,
  UpdateVisaApplicationRequest,
  TransitionVisaStatusRequest,
  UploadVisaDocumentRequest,
} from './types';

const VISAS_KEY = 'visa-applications';

export function useVisaApplications(params?: QueryParams) {
  return useQuery({
    queryKey: [VISAS_KEY, params],
    queryFn: () => api.listVisaApplications(params),
  });
}

export function useVisaApplication(id: string) {
  return useQuery({
    queryKey: [VISAS_KEY, id],
    queryFn: () => api.getVisaApplication(id, { include: 'statusHistory,documents' }),
    enabled: !!id,
  });
}

export function useVisaStatusHistory(id: string) {
  return useQuery({
    queryKey: [VISAS_KEY, id, 'status-history'],
    queryFn: () => api.getVisaStatusHistory(id),
    enabled: !!id,
  });
}

export function useCreateVisaApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateVisaApplicationRequest) => api.createVisaApplication(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VISAS_KEY] });
    },
  });
}

export function useUpdateVisaApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateVisaApplicationRequest }) =>
      api.updateVisaApplication(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VISAS_KEY] });
    },
  });
}

export function useTransitionVisaStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionVisaStatusRequest }) =>
      api.transitionVisaStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VISAS_KEY] });
    },
  });
}

export function useUploadVisaDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UploadVisaDocumentRequest }) =>
      api.uploadVisaDocument(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VISAS_KEY] });
    },
  });
}

export function useDeleteVisaApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteVisaApplication(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VISAS_KEY] });
    },
  });
}

export function useWorkerVisaApplications(workerId: string) {
  return useQuery({
    queryKey: [VISAS_KEY, 'worker', workerId],
    queryFn: () => api.getVisaApplicationsByWorker(workerId),
    enabled: !!workerId,
  });
}
