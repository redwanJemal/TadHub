import { useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../client';
import type { QueryParams } from '../types';
import type {
  AdminDashboard,
  DashboardStats,
  UserWithMemberships,
  TenantWithStats,
  TenantDetail,
  AuditLog,
  AuditSummary,
  Role,
} from '../types/admin';
import {
  createListQuery,
  createDetailQuery,
  createMutations,
  PaginatedQueryResult,
} from './use-query-factory';

// ═══════════════════════════════════════════════════════════════
// Dashboard
// ═══════════════════════════════════════════════════════════════

/**
 * Fetch full admin dashboard data
 */
export function useAdminDashboard() {
  return useQuery<AdminDashboard, ApiError>({
    queryKey: ['admin', 'dashboard'],
    queryFn: () => apiClient.get<AdminDashboard>('/admin/dashboard'),
    staleTime: 60 * 1000, // 1 minute
    refetchInterval: 5 * 60 * 1000, // Refresh every 5 minutes
  });
}

/**
 * Fetch dashboard stats only (lighter)
 */
export function useAdminDashboardStats() {
  return useQuery<DashboardStats, ApiError>({
    queryKey: ['admin', 'dashboard', 'stats'],
    queryFn: () => apiClient.get<DashboardStats>('/admin/dashboard/stats'),
    staleTime: 60 * 1000,
  });
}

// ═══════════════════════════════════════════════════════════════
// Users
// ═══════════════════════════════════════════════════════════════

export interface AdminUsersParams extends QueryParams {
  tenantId?: string;
  status?: 'active' | 'inactive';
  isEmailVerified?: boolean;
  roleSlug?: string;
  createdAfter?: string;
  createdBefore?: string;
}

/**
 * List all users (admin, cross-tenant)
 */
export const useAdminUsers = createListQuery<UserWithMemberships>(
  'admin-users',
  '/admin/users'
);

/**
 * Get single user details
 */
export const useAdminUser = createDetailQuery<UserWithMemberships>(
  'admin-users',
  (id) => `/admin/users/${id}`
);

/**
 * User mutations
 */
export const adminUserMutations = createMutations<
  { email: string; firstName?: string; lastName?: string; roleSlug?: string },
  { firstName?: string; lastName?: string; isActive?: boolean; roleSlug?: string },
  UserWithMemberships
>('admin-users', {
  create: '/admin/users',
  update: (id) => `/admin/users/${id}`,
  delete: (id) => `/admin/users/${id}`,
});

// ═══════════════════════════════════════════════════════════════
// Tenants
// ═══════════════════════════════════════════════════════════════

export interface AdminTenantsParams extends QueryParams {
  status?: 'active' | 'inactive';
  planSlug?: string;
  createdAfter?: string;
  createdBefore?: string;
}

/**
 * List all tenants
 */
export const useAdminTenants = createListQuery<TenantWithStats>(
  'admin-tenants',
  '/admin/tenants'
);

/**
 * Get single tenant details
 */
export const useAdminTenant = createDetailQuery<TenantDetail>(
  'admin-tenants',
  (id) => `/admin/tenants/${id}`
);

/**
 * List users in a specific tenant
 */
export function useAdminTenantUsers(
  tenantId: string | undefined,
  params?: QueryParams
): PaginatedQueryResult<UserWithMemberships> {
  const baseHook = createListQuery<UserWithMemberships>(
    'admin-tenant-users',
    `/admin/tenants/${tenantId}/users`
  );
  
  // Call the hook with params
  const result = baseHook(params, { enabled: !!tenantId });
  return result;
}

/**
 * Tenant mutations
 */
export const adminTenantMutations = createMutations<
  { name: string; slug: string; planId?: string },
  { name?: string; slug?: string; isActive?: boolean; planId?: string },
  TenantDetail
>('admin-tenants', {
  create: '/admin/tenants',
  update: (id) => `/admin/tenants/${id}`,
  delete: (id) => `/admin/tenants/${id}`,
});

// ═══════════════════════════════════════════════════════════════
// Audit Logs
// ═══════════════════════════════════════════════════════════════

export interface AdminAuditLogsParams extends QueryParams {
  tenantId?: string;
  actorId?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * List audit logs
 */
export const useAdminAuditLogs = createListQuery<AuditLog>(
  'admin-audit-logs',
  '/admin/audit-logs'
);

/**
 * Get audit summary
 */
export function useAdminAuditSummary(params?: {
  tenantId?: string;
  startDate?: string;
  endDate?: string;
}) {
  return useQuery<AuditSummary, ApiError>({
    queryKey: ['admin', 'audit-logs', 'summary', params],
    queryFn: () => apiClient.get<AuditSummary>('/admin/audit-logs/summary', params),
    staleTime: 5 * 60 * 1000,
  });
}

// ═══════════════════════════════════════════════════════════════
// Roles
// ═══════════════════════════════════════════════════════════════

export interface AdminRolesParams extends QueryParams {
  tenantId?: string;
  includeSystem?: boolean;
}

/**
 * List roles
 */
export const useAdminRoles = createListQuery<Role>(
  'admin-roles',
  '/admin/roles'
);

// ═══════════════════════════════════════════════════════════════
// Cache Invalidation Helpers
// ═══════════════════════════════════════════════════════════════

/**
 * Hook to get cache invalidation functions
 */
export function useAdminCacheInvalidation() {
  const queryClient = useQueryClient();

  return {
    invalidateDashboard: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] });
    },
    invalidateUsers: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
    },
    invalidateTenants: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-tenants'] });
    },
    invalidateAuditLogs: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-audit-logs'] });
    },
    invalidateAll: () => {
      queryClient.invalidateQueries({ queryKey: ['admin'] });
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      queryClient.invalidateQueries({ queryKey: ['admin-tenants'] });
      queryClient.invalidateQueries({ queryKey: ['admin-audit-logs'] });
    },
  };
}
