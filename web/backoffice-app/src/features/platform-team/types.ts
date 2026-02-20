import { UserProfileDto } from '../users/types';

/**
 * Admin user DTO - represents a platform team member
 */
export interface AdminUserDto {
  id: string;
  userId: string;
  user: UserProfileDto;
  isSuperAdmin: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Request to create a new admin user
 */
export interface CreateAdminUserRequest {
  email: string;
  isSuperAdmin: boolean;
}

/**
 * Request to update an admin user
 */
export interface UpdateAdminUserRequest {
  isSuperAdmin?: boolean;
}
