import { useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import { AuthProvider as OidcAuthProvider, useAuth } from 'react-oidc-context';
import { oidcConfig } from '@/lib/auth-config';

// Store for managing auth state across the app
let accessToken: string | null = null;

export function getAccessToken(): string | null {
  return accessToken;
}

// Inner component to sync auth state
function AuthSync({ children }: { children: ReactNode }) {
  const auth = useAuth();

  useEffect(() => {
    if (auth.isAuthenticated && auth.user?.access_token) {
      accessToken = auth.user.access_token;
      // Store in localStorage for API client
      localStorage.setItem('tadhub_admin_token', auth.user.access_token);
    } else {
      accessToken = null;
      localStorage.removeItem('tadhub_admin_token');
    }
  }, [auth.isAuthenticated, auth.user]);

  // Handle 401 errors - redirect to login
  useEffect(() => {
    const handleUnauthorized = () => {
      if (!auth.isAuthenticated) {
        auth.signinRedirect();
      }
    };

    window.addEventListener('auth:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized);
  }, [auth]);

  return <>{children}</>;
}

// Handle auth callback - clean up URL
function onSigninCallback() {
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
    localStorage.removeItem('tadhub_admin_token');
    localStorage.removeItem('tadhub_admin_employee');
    auth.signoutRedirect();
  }, [auth]);

  return {
    ...auth,
    login,
    logout,
    accessToken: auth.user?.access_token ?? null,
    isAuthenticated: auth.isAuthenticated,
    isLoading: auth.isLoading,
    user: auth.user,
    employee: auth.user?.profile ? {
      id: auth.user.profile.sub ?? '',
      email: auth.user.profile.email ?? '',
      firstName: auth.user.profile.given_name ?? '',
      lastName: auth.user.profile.family_name ?? '',
      role: 'platform-admin',
      avatar: (auth.user.profile as Record<string, unknown>).picture as string | undefined,
    } : null,
  };
}
