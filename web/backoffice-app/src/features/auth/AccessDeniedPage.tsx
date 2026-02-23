import { ShieldX, LogOut } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useAppAuth } from './AuthProvider';

export function AccessDeniedPage() {
  const { logout } = useAppAuth();

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="text-center max-w-md">
        <div className="mx-auto mb-6 w-20 h-20 rounded-full bg-destructive/10 flex items-center justify-center">
          <ShieldX className="h-10 w-10 text-destructive" />
        </div>

        <h1 className="text-3xl font-bold mb-2">Access Denied</h1>

        <p className="text-muted-foreground mb-8">
          This portal is for platform administrators only. If you believe this
          is an error, please contact your system administrator.
        </p>

        <Button onClick={logout} variant="outline" className="gap-2">
          <LogOut className="h-4 w-4" />
          Sign Out
        </Button>
      </div>
    </div>
  );
}
