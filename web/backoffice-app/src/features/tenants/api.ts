import { apiClient } from '@/shared/api/client';
import { PaginatedData, QueryParams } from '@/shared/api/types';
import {
  TenantDto,
  CreateTenantRequest,
  UpdateTenantRequest,
  TenantUserDto,
  AddTenantMemberRequest,
  UpdateMemberRoleRequest,
  UserProfileDto,
} from './types';

// ============================================================================
// Tenant CRUD
// ============================================================================

/**
 * List all tenants (admin view)
 */
export async function listTenants(params?: QueryParams): Promise<PaginatedData<TenantDto>> {
  return apiClient.get<PaginatedData<TenantDto>>('/tenants', params);
}

/**
 * Get tenant by ID
 */
export async function getTenant(tenantId: string): Promise<TenantDto> {
  return apiClient.get<TenantDto>(`/tenants/${tenantId}`);
}

/**
 * Get tenant by slug
 */
export async function getTenantBySlug(slug: string): Promise<TenantDto> {
  return apiClient.get<TenantDto>(`/tenants/by-slug/${slug}`);
}

/**
 * Create a new tenant
 */
export async function createTenant(data: CreateTenantRequest): Promise<TenantDto> {
  return apiClient.post<TenantDto>('/tenants', data);
}

/**
 * Update tenant
 */
export async function updateTenant(tenantId: string, data: UpdateTenantRequest): Promise<TenantDto> {
  return apiClient.patch<TenantDto>(`/tenants/${tenantId}`, data);
}

/**
 * Suspend tenant
 */
export async function suspendTenant(tenantId: string): Promise<void> {
  return apiClient.post<void>(`/tenants/${tenantId}/suspend`);
}

/**
 * Reactivate tenant
 */
export async function reactivateTenant(tenantId: string): Promise<void> {
  return apiClient.post<void>(`/tenants/${tenantId}/reactivate`);
}

/**
 * Delete tenant
 */
export async function deleteTenant(tenantId: string): Promise<void> {
  return apiClient.delete<void>(`/tenants/${tenantId}`);
}

// ============================================================================
// Tenant Members
// ============================================================================

/**
 * List tenant members
 */
export async function listTenantMembers(
  tenantId: string,
  params?: QueryParams
): Promise<PaginatedData<TenantUserDto>> {
  return apiClient.get<PaginatedData<TenantUserDto>>(`/tenants/${tenantId}/members`, params);
}

/**
 * Get tenant member
 */
export async function getTenantMember(tenantId: string, userId: string): Promise<TenantUserDto> {
  return apiClient.get<TenantUserDto>(`/tenants/${tenantId}/members/${userId}`);
}

/**
 * Add member to tenant
 */
export async function addTenantMember(
  tenantId: string,
  data: AddTenantMemberRequest
): Promise<TenantUserDto> {
  return apiClient.post<TenantUserDto>(`/tenants/${tenantId}/members`, data);
}

/**
 * Update member role
 */
export async function updateMemberRole(
  tenantId: string,
  userId: string,
  data: UpdateMemberRoleRequest
): Promise<TenantUserDto> {
  return apiClient.patch<TenantUserDto>(`/tenants/${tenantId}/members/${userId}`, data);
}

/**
 * Remove member from tenant
 */
export async function removeTenantMember(tenantId: string, userId: string): Promise<void> {
  return apiClient.delete<void>(`/tenants/${tenantId}/members/${userId}`);
}

// ============================================================================
// Users (for searching/adding to tenants)
// ============================================================================

/**
 * List all users (admin)
 */
export async function listUsers(params?: QueryParams): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get<PaginatedData<UserProfileDto>>('/users', params);
}

/**
 * Search users by email or name
 */
export async function searchUsers(query: string): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get<PaginatedData<UserProfileDto>>('/users', {
    q: query,
    perPage: 10,
  });
}

/**
 * Get user by ID
 */
export async function getUser(userId: string): Promise<UserProfileDto> {
  return apiClient.get<UserProfileDto>(`/users/${userId}`);
}
