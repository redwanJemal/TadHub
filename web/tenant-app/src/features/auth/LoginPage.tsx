import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppAuth } from './AuthProvider';
import { Users, UserCheck, Target, FileText } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';

export function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAppAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      navigate('/dashboard', { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen">
      {/* Left Column - Branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-primary via-primary/90 to-primary/80 p-12 flex-col justify-between text-primary-foreground">
        <div>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-white/20 font-bold text-xl">
              T
            </div>
            <span className="text-2xl font-bold">Tadbeer</span>
          </div>
        </div>

        <div className="space-y-8">
          <div>
            <h1 className="text-4xl font-bold mb-4">
              Domestic worker recruitment, simplified
            </h1>
            <p className="text-lg text-primary-foreground/80">
              Manage your agency operations, workers, and clients all in one
              place.
            </p>
          </div>

          <div className="grid grid-cols-2 gap-6">
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Users className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Workers</h3>
                <p className="text-sm text-primary-foreground/70">
                  Track worker profiles, contracts & availability
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <UserCheck className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Clients</h3>
                <p className="text-sm text-primary-foreground/70">
                  Manage client relationships & requests
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <Target className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Leads</h3>
                <p className="text-sm text-primary-foreground/70">
                  Convert prospects into active clients
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/10">
                <FileText className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-semibold">Documents</h3>
                <p className="text-sm text-primary-foreground/70">
                  Generate contracts, visas & compliance docs
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="text-sm text-primary-foreground/60">
          Powered by TadHub Platform
        </div>
      </div>

      {/* Right Column - Login */}
      <div className="flex w-full lg:w-1/2 items-center justify-center bg-background p-8">
        <div className="w-full max-w-md space-y-8 text-center">
          <div>
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-xl bg-primary text-primary-foreground font-bold text-2xl">
              T
            </div>
            <h2 className="text-3xl font-bold">Login to your account</h2>
            <p className="mt-2 text-muted-foreground">
              Access your agency portal
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
