/**
 * User Profile DTO matching API response
 */
export interface UserProfileDto {
  id: string;
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  phone?: string;
  locale: string;
  defaultTenantId?: string;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Create user request (admin only)
 */
export interface CreateUserProfileRequest {
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  phone?: string;
  locale?: string;
  defaultTenantId?: string;
}

/**
 * Update user profile request
 */
export interface UpdateUserProfileRequest {
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  phone?: string;
  locale?: string;
  defaultTenantId?: string;
}

/**
 * User reference DTO (minimal info)
 */
export interface UserRefDto {
  id: string;
  name: string;
  email?: string;
}
