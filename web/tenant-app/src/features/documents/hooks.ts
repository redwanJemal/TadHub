import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateWorkerDocumentRequest,
  UpdateWorkerDocumentRequest,
} from './types';

const DOCUMENTS_KEY = 'documents';
const WORKER_DOCUMENTS_KEY = 'workerDocuments';
const COMPLIANCE_KEY = 'compliance';

// ── Worker-scoped hooks ──

export function useWorkerDocuments(workerId: string, params?: QueryParams) {
  return useQuery({
    queryKey: [WORKER_DOCUMENTS_KEY, workerId, params],
    queryFn: () => api.listWorkerDocuments(workerId, params),
    enabled: !!workerId,
  });
}

export function useWorkerDocument(workerId: string, id: string) {
  return useQuery({
    queryKey: [WORKER_DOCUMENTS_KEY, workerId, id],
    queryFn: () => api.getWorkerDocument(workerId, id),
    enabled: !!workerId && !!id,
  });
}

export function useCreateWorkerDocument(workerId: string) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('documents');
  return useMutation({
    mutationFn: (data: Omit<CreateWorkerDocumentRequest, 'workerId'>) =>
      api.createWorkerDocument(workerId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKER_DOCUMENTS_KEY, workerId] });
      queryClient.invalidateQueries({ queryKey: [DOCUMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [COMPLIANCE_KEY] });
      toast.success(t('toast.createSuccess'));
    },
    onError: (error: Error) => {
      toast.error(error.message || t('toast.createError'));
    },
  });
}

export function useUpdateWorkerDocument(workerId: string) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('documents');
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateWorkerDocumentRequest }) =>
      api.updateWorkerDocument(workerId, id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKER_DOCUMENTS_KEY, workerId] });
      queryClient.invalidateQueries({ queryKey: [DOCUMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [COMPLIANCE_KEY] });
      toast.success(t('toast.updateSuccess'));
    },
    onError: (error: Error) => {
      toast.error(error.message || t('toast.updateError'));
    },
  });
}

export function useDeleteWorkerDocument(workerId: string) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('documents');
  return useMutation({
    mutationFn: (id: string) => api.deleteWorkerDocument(workerId, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKER_DOCUMENTS_KEY, workerId] });
      queryClient.invalidateQueries({ queryKey: [DOCUMENTS_KEY] });
      queryClient.invalidateQueries({ queryKey: [COMPLIANCE_KEY] });
      toast.success(t('toast.deleteSuccess'));
    },
    onError: (error: Error) => {
      toast.error(error.message || t('toast.deleteError'));
    },
  });
}

export function useUploadDocumentFile(workerId: string) {
  const queryClient = useQueryClient();
  const { t } = useTranslation('documents');
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      api.uploadDocumentFile(workerId, id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [WORKER_DOCUMENTS_KEY, workerId] });
      queryClient.invalidateQueries({ queryKey: [DOCUMENTS_KEY] });
      toast.success(t('toast.uploadSuccess'));
    },
    onError: (error: Error) => {
      toast.error(error.message || t('toast.uploadError'));
    },
  });
}

// ── Tenant-wide hooks ──

export function useAllDocuments(params?: QueryParams) {
  return useQuery({
    queryKey: [DOCUMENTS_KEY, params],
    queryFn: () => api.listAllDocuments(params),
  });
}

export function useExpiringDocuments(days: number = 30, params?: QueryParams) {
  return useQuery({
    queryKey: [DOCUMENTS_KEY, 'expiring', days, params],
    queryFn: () => api.getExpiringDocuments(days, params),
  });
}

export function useComplianceSummary() {
  return useQuery({
    queryKey: [COMPLIANCE_KEY],
    queryFn: () => api.getComplianceSummary(),
  });
}
