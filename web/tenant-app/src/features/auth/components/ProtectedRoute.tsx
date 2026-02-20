import { ReactNode, useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { apiClient } from '../../../shared/api/client';

interface ProtectedRouteProps {
  children: ReactNode;
  requireOnboarding?: boolean;
}

interface UserStatus {
  status: 'onboarding' | 'select_tenant' | 'active';
  pendingInvitations?: Array<{ id: string; tenantName: string }>;
  canCreateTenant?: boolean;
  tenants?: Array<{ id: string; name: string; slug: string }>;
  profile?: {
    id: string;
    email: string;
    displayName: string;
  };
  tenant?: {
    id: string;
    name: string;
    slug: string;
  };
}

export function ProtectedRoute({ children, requireOnboarding = false }: ProtectedRouteProps) {
  const auth = useAuth();
  const location = useLocation();
  const [userStatus, setUserStatus] = useState<UserStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchUserStatus() {
      if (!auth.isAuthenticated) {
        setIsLoading(false);
        return;
      }

      try {
        const status = await apiClient.get<UserStatus>('/me');
        setUserStatus(status);
      } catch (error) {
        console.error('Failed to fetch user status:', error);
      } finally {
        setIsLoading(false);
      }
    }

    fetchUserStatus();
  }, [auth.isAuthenticated]);

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

  // For onboarding route - only allow if user needs onboarding
  if (requireOnboarding) {
    if (userStatus?.status !== 'onboarding' && userStatus?.status !== 'select_tenant') {
      return <Navigate to="/dashboard" replace />;
    }
    return <>{children}</>;
  }

  // For regular protected routes - redirect to onboarding if needed
  if (userStatus?.status === 'onboarding' || userStatus?.status === 'select_tenant') {
    return <Navigate to="/onboarding" replace />;
  }

  return <>{children}</>;
}
