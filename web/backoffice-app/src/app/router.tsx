import { Routes, Route, Navigate } from "react-router-dom";
import { Suspense, lazy } from "react";
import { DashboardLayout } from "@/shared/components/layout/DashboardLayout";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute";
import { PageLoader } from "@/shared/components/ui/page-loader";

// Auth pages
const LoginPage = lazy(() => import("@/features/auth/LoginPage").then(m => ({ default: m.LoginPage })));
const CallbackPage = lazy(() => import("@/features/auth/CallbackPage").then(m => ({ default: m.CallbackPage })));

// Placeholder home
function HomePage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center">
      <h1 className="text-3xl font-bold mb-4">TadHub - Backoffice</h1>
      <p className="text-muted-foreground mb-8">
        Platform Administration for Tadbeer ERP
      </p>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-left max-w-2xl">
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">🏢 Tenants</h3>
          <p className="text-sm text-muted-foreground">Manage agencies, subscriptions</p>
        </div>
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">👤 Users</h3>
          <p className="text-sm text-muted-foreground">Staff accounts, roles</p>
        </div>
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">📋 Audit</h3>
          <p className="text-sm text-muted-foreground">Activity logs, compliance</p>
        </div>
      </div>
    </div>
  );
}

export function AppRoutes() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/callback" element={<CallbackPage />} />

        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<DashboardLayout />}>
            <Route path="/" element={<HomePage />} />
            {/* Feature routes will be added here */}
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
