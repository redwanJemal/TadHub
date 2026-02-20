import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute';
import { DashboardLayout } from '@/shared/components/layout/DashboardLayout';
import { PageLoader } from '@/shared/components/ui/page-loader';

// Lazy load pages
const LoginPage = lazy(() => import('@/features/auth/LoginPage').then(m => ({ default: m.LoginPage })));
const SignUpPage = lazy(() => import('@/features/auth/SignUpPage').then(m => ({ default: m.SignUpPage })));
const CallbackPage = lazy(() => import('@/features/auth/CallbackPage').then(m => ({ default: m.CallbackPage })));
const OnboardingPage = lazy(() => import('@/features/onboarding/OnboardingPage').then(m => ({ default: m.OnboardingPage })));
const DashboardPage = lazy(() => import('@/features/dashboard/DashboardPage').then(m => ({ default: m.DashboardPage })));
const SettingsPage = lazy(() => import('@/features/settings/SettingsPage').then(m => ({ default: m.SettingsPage })));
const ApiKeysPage = lazy(() => import('@/features/api-keys/ApiKeysPage').then(m => ({ default: m.ApiKeysPage })));
const IntegrationsPage = lazy(() => import('@/features/integrations/IntegrationsPage').then(m => ({ default: m.IntegrationsPage })));
const BillingPage = lazy(() => import('@/features/billing/BillingPage').then(m => ({ default: m.BillingPage })));
const RolesPage = lazy(() => import('@/features/roles/RolesPage').then(m => ({ default: m.RolesPage })));
const TeamPage = lazy(() => import('@/features/team/TeamPage').then(m => ({ default: m.TeamPage })));

export function AppRouter() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/signup" element={<SignUpPage />} />
        <Route path="/callback" element={<CallbackPage />} />
        
        {/* Onboarding - requires auth but no tenant setup yet */}
        <Route
          path="/onboarding"
          element={
            <ProtectedRoute requireOnboarding>
              <OnboardingPage />
            </ProtectedRoute>
          }
        />

        {/* Protected routes - require auth + completed onboarding */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="team" element={<TeamPage />} />
          <Route path="roles" element={<RolesPage />} />
          <Route path="api-keys" element={<ApiKeysPage />} />
          <Route path="integrations" element={<IntegrationsPage />} />
          <Route path="billing" element={<BillingPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
