import { useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import { AuthProvider as OidcAuthProvider, useAuth } from 'react-oidc-context';
import { oidcConfig } from '../../lib/auth-config';
import { useTenantStore } from './hooks/useTenant';

// Store for managing auth state across the app (for non-React contexts like API client)
let accessToken: string | null = null;
let tenantId: string | null = null;

export function getAccessToken(): string | null {
  return accessToken;
}

export function getTenantId(): string | null {
  // First try the module-level variable, then fall back to store
  if (tenantId) return tenantId;
  
  // Get from zustand store (for SSR/hydration scenarios)
  const state = useTenantStore.getState();
  return state.currentTenant?.id ?? null;
}

export function setTenantId(id: string | null): void {
  tenantId = id;
}

// Inner component to sync auth state
function AuthSync({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const { setCurrentTenant, clearTenant } = useTenantStore();

  useEffect(() => {
    if (auth.isAuthenticated && auth.user?.access_token) {
      accessToken = auth.user.access_token;
      
      // Extract tenant_id from token claims if available
      const profile = auth.user.profile as Record<string, unknown>;
      const tokenTenantId = profile?.tenant_id as string | undefined;
      const tokenTenantName = profile?.tenant_name as string | undefined;
      const tokenTenantSlug = profile?.tenant_slug as string | undefined;
      
      if (tokenTenantId) {
        tenantId = tokenTenantId;
        setCurrentTenant({
          id: tokenTenantId,
          name: tokenTenantName ?? 'Tenant',
          slug: tokenTenantSlug ?? tokenTenantId,
        });
      }
    } else {
      accessToken = null;
      tenantId = null;
      clearTenant();
    }
  }, [auth.isAuthenticated, auth.user, setCurrentTenant, clearTenant]);

  // Handle 401 errors - redirect to login
  useEffect(() => {
    const handleUnauthorized = () => {
      if (auth.isAuthenticated) {
        auth.signinRedirect();
      }
    };

    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized);
  }, [auth]);

  return <>{children}</>;
}

// Handle auth callback
function onSigninCallback() {
  // Remove the code and state from the URL
  window.history.replaceState({}, document.title, window.location.pathname);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  return (
    <OidcAuthProvider {...oidcConfig} onSigninCallback={onSigninCallback}>
      <AuthSync>{children}</AuthSync>
    </OidcAuthProvider>
  );
}

// Custom hook for auth with additional utilities
export function useAppAuth() {
  const auth = useAuth();

  const login = useCallback(() => {
    auth.signinRedirect();
  }, [auth]);

  const logout = useCallback(() => {
    auth.signoutRedirect();
  }, [auth]);

  const register = useCallback(() => {
    // Redirect to Keycloak registration
    const registrationUrl = new URL(oidcConfig.authority + '/protocol/openid-connect/registrations');
    registrationUrl.searchParams.set('client_id', oidcConfig.client_id);
    registrationUrl.searchParams.set('response_type', 'code');
    registrationUrl.searchParams.set('scope', 'openid profile email');
    registrationUrl.searchParams.set('redirect_uri', oidcConfig.redirect_uri as string);
    window.location.href = registrationUrl.toString();
  }, []);

  return {
    ...auth,
    login,
    logout,
    register,
    accessToken: auth.user?.access_token ?? null,
    isAuthenticated: auth.isAuthenticated,
    isLoading: auth.isLoading,
    user: auth.user,
  };
}
