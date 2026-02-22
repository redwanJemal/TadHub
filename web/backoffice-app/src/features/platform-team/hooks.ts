import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QueryParams } from '@/shared/api/types';
import * as api from './api';
import { CreatePlatformStaffRequest, UpdatePlatformStaffRequest } from './types';

const QUERY_KEY = 'platform-staff';

/**
 * Hook to list all platform staff members
 */
export function usePlatformStaff(params?: QueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => api.listPlatformStaff(params),
  });
}

/**
 * Hook to get a single platform staff member
 */
export function usePlatformStaffMember(id: string) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => api.getPlatformStaff(id),
    enabled: !!id,
  });
}

/**
 * Hook to get current user's platform staff record
 */
export function useCurrentPlatformStaff() {
  return useQuery({
    queryKey: [QUERY_KEY, 'me'],
    queryFn: () => api.getCurrentPlatformStaff(),
    retry: false,
  });
}

/**
 * Hook to create a new platform staff member
 */
export function useCreatePlatformStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePlatformStaffRequest) => api.createPlatformStaff(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

/**
 * Hook to update a platform staff member
 */
export function useUpdatePlatformStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePlatformStaffRequest }) =>
      api.updatePlatformStaff(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

/**
 * Hook to remove platform staff status from a user
 */
export function useRemovePlatformStaff() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => api.removePlatformStaff(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}
