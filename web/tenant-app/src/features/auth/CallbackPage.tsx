import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';

export function CallbackPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (auth.isAuthenticated) {
      // Get the return URL from state or default to dashboard
      const returnTo = (auth.user?.state as { returnTo?: string })?.returnTo || '/dashboard';
      navigate(returnTo, { replace: true });
    }
  }, [auth.isAuthenticated, auth.user, navigate]);

  if (auth.error) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary/5 via-background to-primary/10 p-4">
        <div className="w-full max-w-md">
          <div className="rounded-2xl bg-card p-8 shadow-xl border border-border">
            <div className="text-center">
              <h2 className="text-xl font-semibold text-destructive mb-2">Authentication Error</h2>
              <p className="text-muted-foreground mb-4">{auth.error.message}</p>
              <button
                onClick={() => auth.signinRedirect()}
                className="rounded-lg bg-primary px-6 py-2 font-medium text-primary-foreground hover:bg-primary/90 transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary/5 via-background to-primary/10 p-4">
      <div className="w-full max-w-md">
        <div className="rounded-2xl bg-card p-8 shadow-xl border border-border">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
            <p className="text-muted-foreground">Completing authentication...</p>
          </div>
        </div>
      </div>
    </div>
  );
}
