import { Routes, Route, Navigate } from "react-router-dom";
import { Suspense, lazy } from "react";
import { DashboardLayout } from "@/shared/components/layout/DashboardLayout";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute";
import { PageLoader } from "@/shared/components/ui/page-loader";

// Auth pages
const LoginPage = lazy(() => import("@/features/auth/LoginPage").then(m => ({ default: m.LoginPage })));
const CallbackPage = lazy(() => import("@/features/auth/CallbackPage").then(m => ({ default: m.CallbackPage })));

// Tenant pages
const TenantsListPage = lazy(() => import("@/features/tenants/pages/TenantsListPage").then(m => ({ default: m.TenantsListPage })));
const TenantFormPage = lazy(() => import("@/features/tenants/pages/TenantFormPage").then(m => ({ default: m.TenantFormPage })));
const TenantDetailPage = lazy(() => import("@/features/tenants/pages/TenantDetailPage").then(m => ({ default: m.TenantDetailPage })));

// Users pages (all user profiles - for searching/adding to tenants)
const UsersListPage = lazy(() => import("@/features/users/pages/UsersListPage").then(m => ({ default: m.UsersListPage })));

// Platform Team (admin users)
const PlatformTeamPage = lazy(() => import("@/features/platform-team/pages/PlatformTeamPage").then(m => ({ default: m.PlatformTeamPage })));

// Audit pages
const AuditLogsPage = lazy(() => import("@/features/audit/pages/AuditLogsPage").then(m => ({ default: m.AuditLogsPage })));

// Notification pages
const NotificationsPage = lazy(() => import("@/features/notifications/pages/NotificationsPage").then(m => ({ default: m.NotificationsPage })));
const SendNotificationPage = lazy(() => import("@/features/notifications/pages/SendNotificationPage").then(m => ({ default: m.SendNotificationPage })));

// Dashboard home
function DashboardHome() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">
          Welcome to TadHub Platform Administration
        </p>
      </div>
      
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <a
          href="/tenants"
          className="block p-6 border rounded-lg hover:bg-muted/50 transition-colors"
        >
          <div className="flex items-center gap-4">
            <div className="p-2 rounded-lg bg-primary/10">
              <svg className="h-6 w-6 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <div>
              <h3 className="font-semibold">Tenants</h3>
              <p className="text-sm text-muted-foreground">Manage agencies and organizations</p>
            </div>
          </div>
        </a>
        
        <a
          href="/users"
          className="block p-6 border rounded-lg hover:bg-muted/50 transition-colors"
        >
          <div className="flex items-center gap-4">
            <div className="p-2 rounded-lg bg-primary/10">
              <svg className="h-6 w-6 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
            <div>
              <h3 className="font-semibold">Users</h3>
              <p className="text-sm text-muted-foreground">Platform user accounts</p>
            </div>
          </div>
        </a>
        
        <a
          href="/audit"
          className="block p-6 border rounded-lg hover:bg-muted/50 transition-colors"
        >
          <div className="flex items-center gap-4">
            <div className="p-2 rounded-lg bg-primary/10">
              <svg className="h-6 w-6 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <div>
              <h3 className="font-semibold">Audit Logs</h3>
              <p className="text-sm text-muted-foreground">Activity and compliance logs</p>
            </div>
          </div>
        </a>
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
            {/* Dashboard */}
            <Route path="/" element={<DashboardHome />} />
            
            {/* Tenants */}
            <Route path="/tenants" element={<TenantsListPage />} />
            <Route path="/tenants/new" element={<TenantFormPage />} />
            <Route path="/tenants/:tenantId" element={<TenantDetailPage />} />
            <Route path="/tenants/:tenantId/edit" element={<TenantFormPage />} />
            
            {/* Platform Team (Admin Users) */}
            <Route path="/platform-team" element={<PlatformTeamPage />} />
            
            {/* Users (All User Profiles - for searching) */}
            <Route path="/users" element={<UsersListPage />} />
            
            {/* Notifications */}
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/notifications/send" element={<SendNotificationPage />} />

            {/* Audit Logs */}
            <Route path="/audit-logs" element={<AuditLogsPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
