import { Routes, Route, Navigate } from "react-router-dom";
import { Suspense, lazy } from "react";

// Lazy load features for code splitting
const LoginPage = lazy(() => import("@/features/auth/LoginPage").then(m => ({ default: m.LoginPage })));
const CallbackPage = lazy(() => import("@/features/auth/CallbackPage").then(m => ({ default: m.CallbackPage })));
const DashboardPage = lazy(() => import("@/features/dashboard/DashboardPage").then(m => ({ default: m.DashboardPage })));
const TenantsPage = lazy(() => import("@/features/tenants/TenantsPage").then(m => ({ default: m.TenantsPage })));
const TenantDetailPage = lazy(() => import("@/features/tenants/TenantDetailPage").then(m => ({ default: m.TenantDetailPage })));
const UsersPage = lazy(() => import("@/features/users/UsersPage").then(m => ({ default: m.UsersPage })));
const RolesPage = lazy(() => import("@/features/roles/RolesPage").then(m => ({ default: m.RolesPage })));
const AuditLogsPage = lazy(() => import("@/features/audit-logs/AuditLogsPage").then(m => ({ default: m.AuditLogsPage })));
const LookupsPage = lazy(() => import("@/features/lookups/LookupsPage").then(m => ({ default: m.LookupsPage })));
const SettingsPage = lazy(() => import("@/features/settings/SettingsPage").then(m => ({ default: m.SettingsPage })));

import { DashboardLayout } from "@/shared/components/layout/DashboardLayout";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute";
import { PageLoader } from "@/shared/components/ui/page-loader";

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
            <Route path="/" element={<DashboardPage />} />
            <Route path="/tenants" element={<TenantsPage />} />
            <Route path="/tenants/:id" element={<TenantDetailPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
            <Route path="/lookups" element={<LookupsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
