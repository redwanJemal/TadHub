import { useQuery } from '@tanstack/react-query';
import { useAuth } from 'react-oidc-context';
import { apiClient } from '../../../shared/api/client';

/**
 * User profile from API
 */
export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  avatar?: string;
  phoneNumber?: string;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

/**
 * User's tenant membership
 */
export interface TenantMembership {
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  role: {
    id: string;
    name: string;
  };
  joinedAt: string;
}

/**
 * Full user status response
 */
export interface UserStatus {
  profile: UserProfile;
  currentTenant?: {
    id: string;
    name: string;
    slug: string;
    logo?: string;
    type?: string;
  };
  memberships: TenantMembership[];
  permissions: string[];
  status: 'onboarding' | 'select_tenant' | 'active';
  pendingInvitations?: Array<{
    id: string;
    tenantId: string;
    tenantName: string;
    invitedByName: string;
    expiresAt: string;
  }>;
}

/**
 * Fetch current user status
 */
async function fetchUserStatus(): Promise<UserStatus> {
  return apiClient.get<UserStatus>('/me');
}

/**
 * Hook to get current user profile and status
 */
export function useUser() {
  const auth = useAuth();

  const query = useQuery({
    queryKey: ['user', 'me'],
    queryFn: fetchUserStatus,
    enabled: auth.isAuthenticated,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 1,
  });

  const user = query.data?.profile;
  const currentTenant = query.data?.currentTenant;
  const memberships = query.data?.memberships ?? [];
  const permissions = query.data?.permissions ?? [];
  const status = query.data?.status;
  const pendingInvitations = query.data?.pendingInvitations ?? [];

  // Derived values
  const fullName = user ? `${user.firstName} ${user.lastName}`.trim() : '';
  const initials = user 
    ? `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase() 
    : '';
  const hasMultipleTenants = memberships.length > 1;
  const needsOnboarding = status === 'onboarding' || status === 'select_tenant';
  const hasPendingInvitations = pendingInvitations.length > 0;

  return {
    // Query state
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    refetch: query.refetch,

    // User data
    user,
    userId: user?.id,
    email: user?.email,
    fullName,
    initials,
    avatar: user?.avatar,

    // Tenant data
    currentTenant,
    tenantId: currentTenant?.id,
    tenantName: currentTenant?.name,
    memberships,
    hasMultipleTenants,

    // Status
    status,
    needsOnboarding,
    permissions,
    pendingInvitations,
    hasPendingInvitations,

    // Auth state from OIDC
    isAuthenticated: auth.isAuthenticated,
    isAuthLoading: auth.isLoading,
    accessToken: auth.user?.access_token,
  };
}
