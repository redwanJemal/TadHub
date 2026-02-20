import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workersApi, jobCategoriesApi } from '../api/workers-api';
import type {
  WorkerDto,
  CreateWorkerRequest,
  UpdateWorkerRequest,
  WorkerStateTransitionRequest,
  WorkerFilterParams,
} from '../types';

// Query Keys
export const workerKeys = {
  all: ['workers'] as const,
  lists: () => [...workerKeys.all, 'list'] as const,
  list: (filters: WorkerFilterParams) => [...workerKeys.lists(), filters] as const,
  details: () => [...workerKeys.all, 'detail'] as const,
  detail: (id: string) => [...workerKeys.details(), id] as const,
  history: (id: string) => [...workerKeys.detail(id), 'history'] as const,
  transitions: (id: string) => [...workerKeys.detail(id), 'transitions'] as const,
};

export const jobCategoryKeys = {
  all: ['jobCategories'] as const,
  list: () => [...jobCategoryKeys.all, 'list'] as const,
};

/**
 * Hook to fetch workers list
 */
export function useWorkers(params: WorkerFilterParams = {}) {
  return useQuery({
    queryKey: workerKeys.list(params),
    queryFn: () => workersApi.list(params),
  });
}

/**
 * Hook to fetch single worker
 */
export function useWorker(
  id: string,
  include?: ('skills' | 'languages' | 'media' | 'jobCategory')[]
) {
  return useQuery({
    queryKey: workerKeys.detail(id),
    queryFn: () => workersApi.getById(id, include),
    enabled: !!id,
  });
}

/**
 * Hook to fetch worker state history
 */
export function useWorkerHistory(id: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: [...workerKeys.history(id), page, pageSize],
    queryFn: () => workersApi.getHistory(id, page, pageSize),
    enabled: !!id,
  });
}

/**
 * Hook to fetch valid transitions
 */
export function useValidTransitions(id: string) {
  return useQuery({
    queryKey: workerKeys.transitions(id),
    queryFn: () => workersApi.getValidTransitions(id),
    enabled: !!id,
  });
}

/**
 * Hook to fetch job categories
 */
export function useJobCategories() {
  return useQuery({
    queryKey: jobCategoryKeys.list(),
    queryFn: () => jobCategoriesApi.list(),
    staleTime: 5 * 60 * 1000, // 5 minutes - categories don't change often
  });
}

/**
 * Hook to create worker
 */
export function useCreateWorker() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateWorkerRequest) => workersApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: workerKeys.lists() });
    },
  });
}

/**
 * Hook to update worker
 */
export function useUpdateWorker() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateWorkerRequest }) =>
      workersApi.update(id, data),
    onSuccess: (updatedWorker: WorkerDto) => {
      queryClient.setQueryData(workerKeys.detail(updatedWorker.id), updatedWorker);
      queryClient.invalidateQueries({ queryKey: workerKeys.lists() });
    },
  });
}

/**
 * Hook to delete worker
 */
export function useDeleteWorker() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => workersApi.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: workerKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: workerKeys.lists() });
    },
  });
}

/**
 * Hook to transition worker state
 */
export function useTransitionWorker() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: WorkerStateTransitionRequest }) =>
      workersApi.transition(id, data),
    onSuccess: (updatedWorker: WorkerDto) => {
      queryClient.setQueryData(workerKeys.detail(updatedWorker.id), updatedWorker);
      queryClient.invalidateQueries({ queryKey: workerKeys.lists() });
      queryClient.invalidateQueries({ queryKey: workerKeys.history(updatedWorker.id) });
      queryClient.invalidateQueries({ queryKey: workerKeys.transitions(updatedWorker.id) });
    },
  });
}

/**
 * Hook to add skill to worker
 */
export function useAddWorkerSkill() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      workerId,
      skill,
    }: {
      workerId: string;
      skill: { skillName: string; rating: number };
    }) => workersApi.addSkill(workerId, skill),
    onSuccess: (updatedWorker: WorkerDto) => {
      queryClient.setQueryData(workerKeys.detail(updatedWorker.id), updatedWorker);
    },
  });
}

/**
 * Hook to remove skill from worker
 */
export function useRemoveWorkerSkill() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ workerId, skillId }: { workerId: string; skillId: string }) =>
      workersApi.removeSkill(workerId, skillId),
    onSuccess: (_, { workerId }) => {
      queryClient.invalidateQueries({ queryKey: workerKeys.detail(workerId) });
    },
  });
}

/**
 * Hook to add language to worker
 */
export function useAddWorkerLanguage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      workerId,
      language,
    }: {
      workerId: string;
      language: { language: string; proficiency: string };
    }) => workersApi.addLanguage(workerId, language),
    onSuccess: (updatedWorker: WorkerDto) => {
      queryClient.setQueryData(workerKeys.detail(updatedWorker.id), updatedWorker);
    },
  });
}

/**
 * Hook to remove language from worker
 */
export function useRemoveWorkerLanguage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ workerId, languageId }: { workerId: string; languageId: string }) =>
      workersApi.removeLanguage(workerId, languageId),
    onSuccess: (_, { workerId }) => {
      queryClient.invalidateQueries({ queryKey: workerKeys.detail(workerId) });
    },
  });
}
