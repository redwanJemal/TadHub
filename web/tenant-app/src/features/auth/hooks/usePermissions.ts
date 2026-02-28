import { create } from 'zustand';

/**
 * Permission string format: "resource.action"
 * Examples: "workers.view", "workers.create", "clients.manage"
 */
export type Permission = string;

interface PermissionsState {
  permissions: Permission[];
  roles: string[];
  isLoaded: boolean;
  setPermissions: (permissions: Permission[], roles: string[]) => void;
  clear: () => void;
}

/**
 * Zustand store for user permissions.
 * Populated from /me endpoint response via ProtectedRoute.
 */
export const usePermissionsStore = create<PermissionsState>((set) => ({
  permissions: [],
  roles: [],
  isLoaded: false,
  setPermissions: (permissions, roles) => set({ permissions, roles, isLoaded: true }),
  clear: () => set({ permissions: [], roles: [], isLoaded: false }),
}));

/**
 * Hook for checking user permissions
 */
export function usePermissions() {
  const { permissions, roles, isLoaded } = usePermissionsStore();

  /**
   * Check if user has a specific permission
   */
  const hasPermission = (permission: Permission): boolean => {
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
    roles,
    isLoaded,
    hasPermission,
    hasAllPermissions,
    hasAnyPermission,
    can,

    // Convenience methods for common permissions
    canViewMembers: hasPermission('members.view'),
    canInviteMembers: hasPermission('members.invite'),
    canManageMembers: hasPermission('members.manage'),
    canRemoveMembers: hasPermission('members.remove'),

    canViewRoles: hasPermission('roles.view'),
    canManageRoles: hasPermission('roles.manage'),
    canManageSettings: hasPermission('settings.manage'),
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
