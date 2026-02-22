import { apiClient } from '@/shared/api/client';
import { PaginatedData, QueryParams } from '@/shared/api/types';
import { UserProfileDto, CreateUserProfileRequest, UpdateUserProfileRequest } from './types';

/**
 * List all users (admin only)
 */
export async function listUsers(params?: QueryParams): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get('/users', params);
}

/**
 * Get user by ID (admin only)
 */
export async function getUser(userId: string): Promise<UserProfileDto> {
  return apiClient.get(`/users/${userId}`);
}

/**
 * Get current user profile
 */
export async function getCurrentUser(): Promise<UserProfileDto> {
  return apiClient.get('/users/me');
}

/**
 * Create a new user (platform-admin only)
 */
export async function createUser(data: CreateUserProfileRequest): Promise<UserProfileDto> {
  return apiClient.post('/users', data);
}

/**
 * Update user profile (admin only)
 */
export async function updateUser(userId: string, data: UpdateUserProfileRequest): Promise<UserProfileDto> {
  return apiClient.patch(`/users/${userId}`, data);
}

/**
 * Deactivate user (platform-admin only)
 */
export async function deactivateUser(userId: string): Promise<void> {
  return apiClient.post(`/users/${userId}/deactivate`);
}

/**
 * Reactivate user (platform-admin only)
 */
export async function reactivateUser(userId: string): Promise<void> {
  return apiClient.post(`/users/${userId}/reactivate`);
}

/**
 * Search users by query
 */
export async function searchUsers(query: string): Promise<PaginatedData<UserProfileDto>> {
  return apiClient.get('/users', { search: query });
}
