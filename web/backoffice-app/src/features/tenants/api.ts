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
// Tenant CRUD (Admin endpoints - no tenant context required)
// ============================================================================

/**
 * List all tenants (admin view)
 */
export async function listTenants(params?: QueryParams): Promise<PaginatedData<TenantDto>> {
  return apiClient.get<PaginatedData<TenantDto>>('/admin/tenants', params);
}

/**
 * Get tenant by ID
 */
export async function getTenant(tenantId: string): Promise<TenantDto> {
  return apiClient.get<TenantDto>(`/admin/tenants/${tenantId}`);
}

/**
 * Get tenant by slug
 */
export async function getTenantBySlug(slug: string): Promise<TenantDto> {
  return apiClient.get<TenantDto>(`/admin/tenants/by-slug/${slug}`);
}

/**
 * Create a new tenant
 */
export async function createTenant(data: CreateTenantRequest): Promise<TenantDto> {
  return apiClient.post<TenantDto>('/admin/tenants', data);
}

/**
 * Update tenant
 */
export async function updateTenant(tenantId: string, data: UpdateTenantRequest): Promise<TenantDto> {
  return apiClient.patch<TenantDto>(`/admin/tenants/${tenantId}`, data);
}

/**
 * Suspend tenant
 */
export async function suspendTenant(tenantId: string): Promise<void> {
  return apiClient.post<void>(`/admin/tenants/${tenantId}/suspend`);
}

/**
 * Reactivate tenant
 */
export async function reactivateTenant(tenantId: string): Promise<void> {
  return apiClient.post<void>(`/admin/tenants/${tenantId}/reactivate`);
}

/**
 * Delete tenant
 */
export async function deleteTenant(tenantId: string): Promise<void> {
  return apiClient.delete<void>(`/admin/tenants/${tenantId}`);
}

// ============================================================================
// Tenant Members (Admin endpoints)
// ============================================================================

/**
 * List tenant members
 */
export async function listTenantMembers(
  tenantId: string,
  params?: QueryParams
): Promise<PaginatedData<TenantUserDto>> {
  return apiClient.get<PaginatedData<TenantUserDto>>(`/admin/tenants/${tenantId}/members`, params);
}

/**
 * Get tenant member
 */
export async function getTenantMember(tenantId: string, userId: string): Promise<TenantUserDto> {
  return apiClient.get<TenantUserDto>(`/admin/tenants/${tenantId}/members/${userId}`);
}

/**
 * Add member to tenant
 */
export async function addTenantMember(
  tenantId: string,
  data: AddTenantMemberRequest
): Promise<TenantUserDto> {
  return apiClient.post<TenantUserDto>(`/admin/tenants/${tenantId}/members`, data);
}

/**
 * Update member role
 */
export async function updateMemberRole(
  tenantId: string,
  userId: string,
  data: UpdateMemberRoleRequest
): Promise<TenantUserDto> {
  return apiClient.patch<TenantUserDto>(`/admin/tenants/${tenantId}/members/${userId}`, data);
}

/**
 * Remove member from tenant
 */
export async function removeTenantMember(tenantId: string, userId: string): Promise<void> {
  return apiClient.delete<void>(`/admin/tenants/${tenantId}/members/${userId}`);
}

// ============================================================================
// Tenant Invitations (Admin endpoints)
// ============================================================================

/**
 * List tenant invitations
 */
export async function listTenantInvitations(
  tenantId: string,
  params?: QueryParams
): Promise<PaginatedData<any>> {
  return apiClient.get<PaginatedData<any>>(`/admin/tenants/${tenantId}/invitations`, params);
}

/**
 * Create invitation
 */
export async function createTenantInvitation(
  tenantId: string,
  data: { email: string; role: string }
): Promise<any> {
  return apiClient.post<any>(`/admin/tenants/${tenantId}/invitations`, data);
}

/**
 * Revoke invitation
 */
export async function revokeTenantInvitation(tenantId: string, invitationId: string): Promise<void> {
  return apiClient.delete<void>(`/admin/tenants/${tenantId}/invitations/${invitationId}`);
}

// ============================================================================
// Users (for searching/adding to tenants)
// ============================================================================

/**
 * List all users (admin)
 */
export async function listUsers(params?: QueryParams): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get<PaginatedData<UserProfileDto>>('/admin/users', params);
}

/**
 * Search users by email or name
 */
export async function searchUsers(query: string): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get<PaginatedData<UserProfileDto>>('/admin/users', {
    q: query,
    perPage: 10,
  });
}

/**
 * Get user by ID
 */
export async function getUser(userId: string): Promise<UserProfileDto> {
  return apiClient.get<UserProfileDto>(`/admin/users/${userId}`);
}
