import { apiClient } from '@/shared/api/client';
import { QueryParams } from '@/shared/api/types';
import { PlatformStaffDto, CreatePlatformStaffRequest, UpdatePlatformStaffRequest } from './types';

/**
 * List all platform staff members
 */
export async function listPlatformStaff(params?: QueryParams): Promise<PlatformStaffDto[]> {
  return apiClient.get('/platform/staff', params);
}

/**
 * Get platform staff member by ID
 */
export async function getPlatformStaff(id: string): Promise<PlatformStaffDto> {
  return apiClient.get(`/platform/staff/${id}`);
}

/**
 * Get current user's platform staff record (if they are staff)
 */
export async function getCurrentPlatformStaff(): Promise<PlatformStaffDto> {
  return apiClient.get('/platform/staff/me');
}

/**
 * Create a new platform staff member
 */
export async function createPlatformStaff(data: CreatePlatformStaffRequest): Promise<PlatformStaffDto> {
  return apiClient.post('/platform/staff', data);
}

/**
 * Update a platform staff member
 */
export async function updatePlatformStaff(id: string, data: UpdatePlatformStaffRequest): Promise<PlatformStaffDto> {
  return apiClient.patch(`/platform/staff/${id}`, data);
}

/**
 * Remove platform staff status from a user
 */
export async function removePlatformStaff(id: string): Promise<void> {
  return apiClient.delete(`/platform/staff/${id}`);
}
