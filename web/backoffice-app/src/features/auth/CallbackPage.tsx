import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { PageLoader } from '@/shared/components/ui/page-loader';

export function CallbackPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // If authentication is complete, redirect to dashboard
    if (!auth.isLoading) {
      if (auth.isAuthenticated) {
        navigate('/', { replace: true });
      } else if (auth.error) {
        console.error('Auth callback error:', auth.error);
        navigate('/login', { replace: true });
      }
    }
  }, [auth.isLoading, auth.isAuthenticated, auth.error, navigate]);

  // Show loading while processing callback
  return <PageLoader />;
}
