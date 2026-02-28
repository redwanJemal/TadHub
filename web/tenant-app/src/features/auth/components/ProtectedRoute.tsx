import { ReactNode, useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { apiClient } from '../../../shared/api/client';
import { usePermissions, usePermissionsStore, Permission } from '../hooks/usePermissions';
import { useTenantStore } from '../hooks/useTenant';
import { setTenantId } from '../AuthProvider';
import { ForbiddenPage } from '../ForbiddenPage';

interface ProtectedRouteProps {
  children: ReactNode;
  /** Require user to be in onboarding state */
  requireOnboarding?: boolean;
  /** Required permission to access this route */
  requiredPermission?: Permission;
  /** Required permissions - user must have ALL */
  requiredPermissions?: Permission[];
  /** Required permissions - user must have ANY */
  anyPermission?: Permission[];
  /** Required role (Owner, Admin, Member) */
  requiredRole?: 'Owner' | 'Admin' | 'Member';
  /** Custom fallback for permission denied */
  fallback?: ReactNode;
}

interface UserStatus {
  id: string;
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  locale: string;
  isActive: boolean;
  defaultTenantId?: string;
  needsOnboarding: boolean;
  needsTenantSelection: boolean;
  tenants: Array<{ id: string; name: string; slug: string }>;
  permissions?: string[];
  roles?: string[];
  // Computed fields for compatibility
  status?: 'onboarding' | 'select_tenant' | 'active';
  tenant?: { id: string; name: string; slug: string };
  role?: 'Owner' | 'Admin' | 'Member';
}

function getRealmRoles(accessToken: string | null | undefined): string[] {
  if (!accessToken) return [];
  try {
    const payload = accessToken.split('.')[1] as string | undefined;
    if (!payload) return [];
    const decoded = JSON.parse(atob(payload));
    return decoded?.realm_access?.roles ?? [];
  } catch {
    return [];
  }
}

// Role hierarchy: Owner > Admin > Member
const ROLE_HIERARCHY: Record<string, number> = {
  'Member': 1,
  'Admin': 2,
  'Owner': 3,
};

export function ProtectedRoute({
  children,
  requireOnboarding = false,
  requiredPermission,
  requiredPermissions,
  anyPermission,
  requiredRole,
  fallback,
}: ProtectedRouteProps) {
  const auth = useAuth();
  const location = useLocation();
  const [userStatus, setUserStatus] = useState<UserStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const { hasPermission, hasAllPermissions, hasAnyPermission } = usePermissions();
  const { setCurrentTenant, setAvailableTenants } = useTenantStore();

  useEffect(() => {
    async function fetchUserStatus() {
      if (!auth.isAuthenticated) {
        setIsLoading(false);
        return;
      }

      try {
        const status = await apiClient.get<UserStatus>('/me');
        setUserStatus(status);

        // Set tenant from response
        let needsRefetch = false;
        if (status.tenants && status.tenants.length > 0) {
          setAvailableTenants(status.tenants);

          // Use the first tenant or the one marked as default
          const defaultTenant = status.tenant || status.tenants[0];
          if (defaultTenant) {
            setCurrentTenant(defaultTenant);
            setTenantId(defaultTenant.id);
            console.log('[Auth] Set tenant:', defaultTenant.id, defaultTenant.name);

            // If permissions are empty, re-fetch with tenant header now set
            if (!status.permissions || status.permissions.length === 0) {
              needsRefetch = true;
            }
          }
        }

        if (needsRefetch) {
          // Re-fetch with X-Tenant-ID header now set
          const refreshed = await apiClient.get<UserStatus>('/me');
          setUserStatus(refreshed);
          usePermissionsStore.getState().setPermissions(
            refreshed.permissions ?? [],
            refreshed.roles ?? []
          );
        } else {
          // Populate permissions store from /me response
          usePermissionsStore.getState().setPermissions(
            status.permissions ?? [],
            status.roles ?? []
          );
        }
      } catch (error) {
        console.error('Failed to fetch user status:', error);
      } finally {
        setIsLoading(false);
      }
    }

    fetchUserStatus();
  }, [auth.isAuthenticated, setCurrentTenant, setAvailableTenants]);

  // Still loading OIDC or user status
  if (auth.isLoading || isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  // Not authenticated - redirect to login via Keycloak
  if (!auth.isAuthenticated) {
    auth.signinRedirect({ state: { returnTo: location.pathname } });
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p>Redirecting to login...</p>
        </div>
      </div>
    );
  }

  // Block platform-admins with no tenant memberships
  if (userStatus) {
    const isPlatformAdmin = getRealmRoles(auth.user?.access_token).includes('platform-admin');

    if (isPlatformAdmin && userStatus.tenants.length === 0) {
      return (
        <ForbiddenPage
          message="This portal is for agency staff with tenant memberships. Your account is a platform administrator without tenant access. Please use the backoffice portal instead."
        />
      );
    }
  }

  // For onboarding route - only allow if user needs onboarding
  if (requireOnboarding) {
    const needsOnboarding = userStatus?.needsOnboarding || userStatus?.needsTenantSelection;
    if (!needsOnboarding) {
      return <Navigate to="/dashboard" replace />;
    }
    return <>{children}</>;
  }

  // For regular protected routes - redirect to onboarding if needed
  if (userStatus?.needsOnboarding || userStatus?.needsTenantSelection) {
    return <Navigate to="/onboarding" replace />;
  }

  // Check role-based access
  if (requiredRole && userStatus?.role) {
    const userRoleLevel = ROLE_HIERARCHY[userStatus.role] || 0;
    const requiredRoleLevel = ROLE_HIERARCHY[requiredRole] || 0;
    
    if (userRoleLevel < requiredRoleLevel) {
      if (fallback) return <>{fallback}</>;
      return (
        <ForbiddenPage 
          message={`This page requires ${requiredRole} role or higher.`}
        />
      );
    }
  }

  // Check single permission
  if (requiredPermission && !hasPermission(requiredPermission)) {
    if (fallback) return <>{fallback}</>;
    return (
      <ForbiddenPage 
        requiredPermission={requiredPermission}
      />
    );
  }

  // Check all permissions (AND)
  if (requiredPermissions && requiredPermissions.length > 0) {
    if (!hasAllPermissions(requiredPermissions)) {
      if (fallback) return <>{fallback}</>;
      return (
        <ForbiddenPage 
          requiredPermission={requiredPermissions.join(', ')}
          message="You need all of the required permissions to access this page."
        />
      );
    }
  }

  // Check any permission (OR)
  if (anyPermission && anyPermission.length > 0) {
    if (!hasAnyPermission(anyPermission)) {
      if (fallback) return <>{fallback}</>;
      return (
        <ForbiddenPage 
          requiredPermission={anyPermission.join(' or ')}
          message="You need at least one of the required permissions to access this page."
        />
      );
    }
  }

  return <>{children}</>;
}
