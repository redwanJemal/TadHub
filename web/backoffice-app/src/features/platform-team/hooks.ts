import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QueryParams } from '@/shared/api/types';
import * as api from './api';
import { CreateAdminUserRequest, UpdateAdminUserRequest } from './types';

const QUERY_KEY = 'admin-users';

/**
 * Hook to list all platform admin users
 */
export function useAdminUsers(params?: QueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => api.listAdminUsers(params),
  });
}

/**
 * Hook to get a single admin user
 */
export function useAdminUser(id: string) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => api.getAdminUser(id),
    enabled: !!id,
  });
}

/**
 * Hook to get current user's admin record
 */
export function useCurrentAdminUser() {
  return useQuery({
    queryKey: [QUERY_KEY, 'me'],
    queryFn: () => api.getCurrentAdminUser(),
    retry: false, // Don't retry if user is not an admin
  });
}

/**
 * Hook to create a new admin user
 */
export function useCreateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAdminUserRequest) => api.createAdminUser(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

/**
 * Hook to update an admin user
 */
export function useUpdateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAdminUserRequest }) =>
      api.updateAdminUser(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

/**
 * Hook to remove admin status from a user
 */
export function useRemoveAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => api.removeAdminUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}
