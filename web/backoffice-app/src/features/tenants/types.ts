/**
 * Tenant status enum
 */
export type TenantStatus = 'Active' | 'Suspended' | 'Deleted';

/**
 * Tenant DTO from API
 */
export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  status: TenantStatus;
  tenantTypeId?: string;
  tenantTypeName?: string;
  logoUrl?: string;
  description?: string;
  website?: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Create tenant request
 */
export interface CreateTenantRequest {
  name: string;
  slug?: string;
  tenantTypeId?: string;
  logoUrl?: string;
  description?: string;
  website?: string;
}

/**
 * Update tenant request
 */
export interface UpdateTenantRequest {
  name?: string;
  tenantTypeId?: string;
  logoUrl?: string;
  description?: string;
  website?: string;
}

/**
 * Tenant member/user DTO
 */
export type TenantRole = 'Member' | 'Admin' | 'Owner';

export interface TenantUserDto {
  id: string;
  tenantId: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  role: TenantRole;
  joinedAt: string;
}

/**
 * Add member to tenant request
 */
export interface AddTenantMemberRequest {
  userId: string;
  role: TenantRole;
}

/**
 * Update member role request
 */
export interface UpdateMemberRoleRequest {
  role: TenantRole;
}

/**
 * User profile for searching/selecting users
 */
export interface UserProfileDto {
  id: string;
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  isActive: boolean;
  createdAt: string;
}
