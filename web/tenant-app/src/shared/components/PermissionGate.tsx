import type { ReactNode } from 'react';
import { usePermissions, type Permission } from '@/features/auth/hooks/usePermissions';

interface PermissionGateProps {
  /** Single permission required */
  permission?: Permission;
  /** All of these permissions required */
  allOf?: Permission[];
  /** Any of these permissions required */
  anyOf?: Permission[];
  /** Content to render when authorized */
  children: ReactNode;
  /** Content to render when denied (default: null) */
  fallback?: ReactNode;
}

/**
 * Conditionally renders children based on user permissions.
 * Returns null while permissions are loading (prevents flash).
 */
export function PermissionGate({
  permission,
  allOf,
  anyOf,
  children,
  fallback = null,
}: PermissionGateProps) {
  const { hasPermission, hasAllPermissions, hasAnyPermission, isLoaded } = usePermissions();

  if (!isLoaded) return null;

  if (permission && !hasPermission(permission)) {
    return <>{fallback}</>;
  }

  if (allOf && allOf.length > 0 && !hasAllPermissions(allOf)) {
    return <>{fallback}</>;
  }

  if (anyOf && anyOf.length > 0 && !hasAnyPermission(anyOf)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
