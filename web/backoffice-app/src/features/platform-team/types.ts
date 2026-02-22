import { UserProfileDto } from '../users/types';

/**
 * Platform staff DTO - represents a platform team member
 */
export interface PlatformStaffDto {
  id: string;
  userId: string;
  user: UserProfileDto;
  role: string;
  department?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Platform staff roles
 */
export type PlatformStaffRole = 'super-admin' | 'admin' | 'finance' | 'sales' | 'support';

/**
 * Request to create a new platform staff member
 */
export interface CreatePlatformStaffRequest {
  email: string;
  role: PlatformStaffRole;
  department?: string;
}

/**
 * Request to update a platform staff member
 */
export interface UpdatePlatformStaffRequest {
  role?: PlatformStaffRole;
  department?: string;
}
