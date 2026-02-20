import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { keycloakUrls } from '../../lib/auth-config';

export function SignUpPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // If already authenticated, redirect to dashboard
    if (auth.isAuthenticated) {
      navigate('/dashboard', { replace: true });
      return;
    }

    // Redirect to Keycloak registration
    if (!auth.isLoading) {
      window.location.href = keycloakUrls.register;
    }
  }, [auth, navigate]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary/5 via-background to-primary/10 p-4">
      <div className="w-full max-w-md">
        <div className="rounded-2xl bg-card p-8 shadow-xl border border-border">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
            <p className="text-muted-foreground">Redirecting to registration...</p>
          </div>
        </div>
      </div>
    </div>
  );
}
