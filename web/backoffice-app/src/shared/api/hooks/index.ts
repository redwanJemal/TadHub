// Query factories
export {
  createListQuery,
  createDetailQuery,
  createMutations,
  createPrefetch,
  type PaginatedQueryResult,
  type ListQueryOptions,
  type DetailQueryOptions,
} from './use-query-factory';

// Admin hooks
export {
  // Dashboard
  useAdminDashboard,
  useAdminDashboardStats,
  // Users
  useAdminUsers,
  useAdminUser,
  adminUserMutations,
  type AdminUsersParams,
  // Tenants
  useAdminTenants,
  useAdminTenant,
  useAdminTenantUsers,
  adminTenantMutations,
  type AdminTenantsParams,
  // Audit
  useAdminAuditLogs,
  useAdminAuditSummary,
  type AdminAuditLogsParams,
  // Roles
  useAdminRoles,
  type AdminRolesParams,
  // Cache
  useAdminCacheInvalidation,
} from './use-admin';

// Pagination
export {
  usePagination,
  useDebouncedSearch,
  type PaginationState,
  type FilterState,
  type SearchState,
  type QueryState,
  type PaginationControls,
  type UsePaginationOptions,
  type SortOrder,
} from './use-pagination';
