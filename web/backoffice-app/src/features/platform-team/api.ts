import { apiClient } from '@/shared/api/client';
import { QueryParams } from '@/shared/api/types';
import { AdminUserDto, CreateAdminUserRequest, UpdateAdminUserRequest } from './types';

/**
 * List all platform admin users
 */
export async function listAdminUsers(params?: QueryParams): Promise<AdminUserDto[]> {
  return apiClient.get('/admin/users', params);
}

/**
 * Get admin user by ID
 */
export async function getAdminUser(id: string): Promise<AdminUserDto> {
  return apiClient.get(`/admin/users/${id}`);
}

/**
 * Get current user's admin record (if they are an admin)
 */
export async function getCurrentAdminUser(): Promise<AdminUserDto> {
  return apiClient.get('/admin/users/me');
}

/**
 * Create a new admin user
 */
export async function createAdminUser(data: CreateAdminUserRequest): Promise<AdminUserDto> {
  return apiClient.post('/admin/users', data);
}

/**
 * Update an admin user
 */
export async function updateAdminUser(id: string, data: UpdateAdminUserRequest): Promise<AdminUserDto> {
  return apiClient.patch(`/admin/users/${id}`, data);
}

/**
 * Remove admin status from a user
 */
export async function removeAdminUser(id: string): Promise<void> {
  return apiClient.delete(`/admin/users/${id}`);
}
