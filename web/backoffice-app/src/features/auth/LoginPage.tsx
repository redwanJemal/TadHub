import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppAuth } from './AuthProvider';
import { PageLoader } from '@/shared/components/ui/page-loader';
import { Blocks, Zap, Shield, Sparkles } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';

export function LoginPage() {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading, login } = useAppAuth();

  useEffect(() => {
    // If already authenticated, redirect to dashboard
    if (isAuthenticated && !isLoading) {
      navigate('/', { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate]);

  // Show loading while checking auth status
  if (isLoading) {
    return <PageLoader />;
  }

  return (
    <div className="flex min-h-screen">
      {/* Left Column - Branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-primary via-primary/90 to-primary/80 p-12 flex-col justify-between text-primary-foreground">
        <div>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-white/20 font-bold text-xl">
              F
            </div>
            <span className="text-2xl font-bold">TadHub</span>
          </div>
        </div>
        
        <div className="space-y-8">
          <div>
            <h1 className="text-4xl font-bold mb-4">
              Build powerful backends without the hassle
            </h1>
            <p className="text-lg text-primary-foreground/80">
              The open-source backend platform that gives you everything you need to build modern applications.
            </p>
          </div>
          
          <div className="grid grid-cols-2 gap-6">
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Blocks className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Database</h3>
                <p className="text-sm text-primary-foreground/70">Visual schema builder with real-time sync</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Shield className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Authentication</h3>
                <p className="text-sm text-primary-foreground/70">Built-in auth with social providers</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Zap className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Functions</h3>
                <p className="text-sm text-primary-foreground/70">Edge functions with global deployment</p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Sparkles className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">AI Ready</h3>
                <p className="text-sm text-primary-foreground/70">Vector embeddings & AI integrations</p>
              </div>
            </div>
          </div>
        </div>
        
        <div className="text-sm text-primary-foreground/60">
          © 2024 TadHub. Open-source backend platform.
        </div>
      </div>

      {/* Right Column - Login Button */}
      <div className="flex w-full lg:w-1/2 items-center justify-center bg-background p-8">
        <div className="w-full max-w-md space-y-8 text-center">
          <div>
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-xl bg-primary text-primary-foreground font-bold text-2xl">
              F
            </div>
            <h2 className="text-3xl font-bold">Welcome back</h2>
            <p className="mt-2 text-muted-foreground">
              Sign in to access the admin panel
            </p>
          </div>

          <Button 
            onClick={login} 
            size="lg" 
            className="w-full text-lg py-6"
          >
            Sign in with SSO
          </Button>

          <p className="text-sm text-muted-foreground">
            You will be redirected to the authentication server
          </p>
        </div>
      </div>
    </div>
  );
}
