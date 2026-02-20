import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QueryParams } from '@/shared/api/types';
import {
  listTenants,
  getTenant,
  createTenant,
  updateTenant,
  suspendTenant,
  reactivateTenant,
  deleteTenant,
  listTenantMembers,
  addTenantMember,
  updateMemberRole,
  removeTenantMember,
  listUsers,
  searchUsers,
} from './api';
import {
  CreateTenantRequest,
  UpdateTenantRequest,
  AddTenantMemberRequest,
  UpdateMemberRoleRequest,
} from './types';

// ============================================================================
// Query Keys
// ============================================================================

export const tenantKeys = {
  all: ['tenants'] as const,
  lists: () => [...tenantKeys.all, 'list'] as const,
  list: (params?: QueryParams) => [...tenantKeys.lists(), params] as const,
  details: () => [...tenantKeys.all, 'detail'] as const,
  detail: (id: string) => [...tenantKeys.details(), id] as const,
  members: (tenantId: string) => [...tenantKeys.detail(tenantId), 'members'] as const,
};

export const userKeys = {
  all: ['users'] as const,
  lists: () => [...userKeys.all, 'list'] as const,
  list: (params?: QueryParams) => [...userKeys.lists(), params] as const,
  search: (query: string) => [...userKeys.all, 'search', query] as const,
};

// ============================================================================
// Tenant Queries
// ============================================================================

export function useTenants(params?: QueryParams) {
  return useQuery({
    queryKey: tenantKeys.list(params),
    queryFn: () => listTenants(params),
  });
}

export function useTenant(tenantId: string) {
  return useQuery({
    queryKey: tenantKeys.detail(tenantId),
    queryFn: () => getTenant(tenantId),
    enabled: !!tenantId,
  });
}

// ============================================================================
// Tenant Mutations
// ============================================================================

export function useCreateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTenantRequest) => createTenant(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

export function useUpdateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, data }: { tenantId: string; data: UpdateTenantRequest }) =>
      updateTenant(tenantId, data),
    onSuccess: (_, { tenantId }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(tenantId) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

export function useSuspendTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (tenantId: string) => suspendTenant(tenantId),
    onSuccess: (_, tenantId) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(tenantId) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

export function useReactivateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (tenantId: string) => reactivateTenant(tenantId),
    onSuccess: (_, tenantId) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.detail(tenantId) });
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

export function useDeleteTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (tenantId: string) => deleteTenant(tenantId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.lists() });
    },
  });
}

// ============================================================================
// Tenant Members Queries & Mutations
// ============================================================================

export function useTenantMembers(tenantId: string, params?: QueryParams) {
  return useQuery({
    queryKey: tenantKeys.members(tenantId),
    queryFn: () => listTenantMembers(tenantId, params),
    enabled: !!tenantId,
  });
}

export function useAddTenantMember() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, data }: { tenantId: string; data: AddTenantMemberRequest }) =>
      addTenantMember(tenantId, data),
    onSuccess: (_, { tenantId }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.members(tenantId) });
    },
  });
}

export function useUpdateMemberRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      tenantId,
      userId,
      data,
    }: {
      tenantId: string;
      userId: string;
      data: UpdateMemberRoleRequest;
    }) => updateMemberRole(tenantId, userId, data),
    onSuccess: (_, { tenantId }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.members(tenantId) });
    },
  });
}

export function useRemoveTenantMember() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ tenantId, userId }: { tenantId: string; userId: string }) =>
      removeTenantMember(tenantId, userId),
    onSuccess: (_, { tenantId }) => {
      queryClient.invalidateQueries({ queryKey: tenantKeys.members(tenantId) });
    },
  });
}

// ============================================================================
// User Queries (for adding members)
// ============================================================================

export function useUsers(params?: QueryParams) {
  return useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => listUsers(params),
  });
}

export function useSearchUsers(query: string) {
  return useQuery({
    queryKey: userKeys.search(query),
    queryFn: () => searchUsers(query),
    enabled: query.length >= 2,
  });
}
