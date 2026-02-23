import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAppAuth } from "../AuthProvider";
import { PageLoader } from "@/shared/components/ui/page-loader";
import { AccessDeniedPage } from "../AccessDeniedPage";

function getRealmRoles(accessToken: string | null | undefined): string[] {
  if (!accessToken) return [];
  try {
    const payload = accessToken.split('.')[1] as string | undefined;
    if (!payload) return [];
    const decoded = JSON.parse(atob(payload));
    return decoded?.realm_access?.roles ?? [];
  } catch {
    return [];
  }
}

export function ProtectedRoute() {
  const { isAuthenticated, isLoading, user } = useAppAuth();
  const location = useLocation();

  // Show loader while checking auth status
  if (isLoading) {
    return <PageLoader />;
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Verify user has platform-admin role (from access token, not ID token)
  const roles = getRealmRoles(user?.access_token);
  const isPlatformAdmin = roles.includes('platform-admin');

  if (!isPlatformAdmin) {
    return <AccessDeniedPage />;
  }

  return <Outlet />;
}
