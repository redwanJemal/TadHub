import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAppAuth } from "../AuthProvider";
import { PageLoader } from "@/shared/components/ui/page-loader";

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAppAuth();
  const location = useLocation();

  // Show loader while checking auth status
  if (isLoading) {
    return <PageLoader />;
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
}
