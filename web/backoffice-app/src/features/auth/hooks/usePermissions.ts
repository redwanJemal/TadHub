import { useMemo } from 'react';
import { useAuth } from 'react-oidc-context';

/**
 * Permission string format: "resource.action"
 * Examples: "workers.view", "workers.create", "clients.manage"
 */
export type Permission = string;

/**
 * Role with permissions
 */
export interface Role {
  id: string;
  name: string;
  permissions: Permission[];
}

/**
 * Extract permissions from OIDC token claims
 */
function extractPermissionsFromToken(user: unknown): Permission[] {
  if (!user || typeof user !== 'object') return [];
  
  const profile = (user as { profile?: Record<string, unknown> }).profile;
  if (!profile) return [];

  // Try different claim formats
  // 1. Direct permissions claim
  if (Array.isArray(profile.permissions)) {
    return profile.permissions as Permission[];
  }

  // 2. Realm roles from Keycloak
  const realmAccess = profile.realm_access as { roles?: string[] } | undefined;
  if (realmAccess?.roles) {
    return realmAccess.roles;
  }

  // 3. Resource access (client roles)
  const resourceAccess = profile.resource_access as Record<string, { roles?: string[] }> | undefined;
  if (resourceAccess) {
    const allRoles: string[] = [];
    for (const resource of Object.values(resourceAccess)) {
      if (resource.roles) {
        allRoles.push(...resource.roles);
      }
    }
    return allRoles;
  }

  return [];
}

/**
 * Hook for checking user permissions
 */
export function usePermissions() {
  const auth = useAuth();

  const permissions = useMemo(() => {
    if (!auth.isAuthenticated || !auth.user) return [];
    return extractPermissionsFromToken(auth.user);
  }, [auth.isAuthenticated, auth.user]);

  /**
   * Check if user has a specific permission
   */
  const hasPermission = (permission: Permission): boolean => {
    // Admin/owner has all permissions
    if (permissions.includes('*') || permissions.includes('admin')) {
      return true;
    }
    return permissions.includes(permission);
  };

  /**
   * Check if user has ALL of the specified permissions
   */
  const hasAllPermissions = (requiredPermissions: Permission[]): boolean => {
    return requiredPermissions.every(hasPermission);
  };

  /**
   * Check if user has ANY of the specified permissions
   */
  const hasAnyPermission = (requiredPermissions: Permission[]): boolean => {
    return requiredPermissions.some(hasPermission);
  };

  /**
   * Check if user can access a resource with given action
   */
  const can = (resource: string, action: string): boolean => {
    return hasPermission(`${resource}.${action}`);
  };

  return {
    permissions,
    hasPermission,
    hasAllPermissions,
    hasAnyPermission,
    can,
    
    // Convenience methods for common permissions
    canViewWorkers: hasPermission('workers.view'),
    canCreateWorkers: hasPermission('workers.create'),
    canManageWorkers: hasPermission('workers.manage'),
    
    canViewClients: hasPermission('clients.view'),
    canCreateClients: hasPermission('clients.create'),
    canManageClients: hasPermission('clients.manage'),
    
    canViewLeads: hasPermission('leads.view'),
    canCreateLeads: hasPermission('leads.create'),
    canManageLeads: hasPermission('leads.manage'),
    
    canManageTeam: hasPermission('tenant.members.manage'),
    canManageRoles: hasPermission('tenant.roles.manage'),
    canManageSettings: hasPermission('tenant.settings.manage'),
  };
}

/**
 * Utility function to check permission outside of React components
 */
export function checkPermission(permissions: Permission[], permission: Permission): boolean {
  if (permissions.includes('*') || permissions.includes('admin')) {
    return true;
  }
  return permissions.includes(permission);
}
