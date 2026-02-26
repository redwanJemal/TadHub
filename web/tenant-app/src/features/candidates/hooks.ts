import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { QueryParams } from '@/shared/api/types/common';
import * as api from './api';
import type {
  CreateCandidateRequest,
  UpdateCandidateRequest,
  TransitionStatusRequest,
} from './types';

const CANDIDATES_KEY = 'candidates';

export function useCandidates(params?: QueryParams) {
  return useQuery({
    queryKey: [CANDIDATES_KEY, params],
    queryFn: () => api.listCandidates({ ...params, include: 'supplier' }),
  });
}

export function useCandidate(id: string) {
  return useQuery({
    queryKey: [CANDIDATES_KEY, id],
    queryFn: () => api.getCandidate(id, { include: 'statusHistory,supplier' }),
    enabled: !!id,
  });
}

export function useStatusHistory(id: string) {
  return useQuery({
    queryKey: [CANDIDATES_KEY, id, 'status-history'],
    queryFn: () => api.getStatusHistory(id),
    enabled: !!id,
  });
}

export function useCreateCandidate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCandidateRequest) => api.createCandidate(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useUpdateCandidate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCandidateRequest }) =>
      api.updateCandidate(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useTransitionStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TransitionStatusRequest }) =>
      api.transitionStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useDeleteCandidate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteCandidate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useUploadPhoto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      api.uploadCandidatePhoto(id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useUploadVideo() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      api.uploadCandidateVideo(id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useUploadPassport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      api.uploadCandidatePassport(id, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [CANDIDATES_KEY] });
    },
  });
}

export function useUploadFile() {
  return useMutation({
    mutationFn: ({ file, fileType }: { file: File; fileType: string }) =>
      api.uploadFile(file, fileType),
  });
}
