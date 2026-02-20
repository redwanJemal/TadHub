import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute';
import { DashboardLayout } from '@/shared/components/layout/DashboardLayout';
import { PageLoader } from '@/shared/components/ui/page-loader';

// Auth pages
const LoginPage = lazy(() => import('@/features/auth/LoginPage').then(m => ({ default: m.LoginPage })));
const SignUpPage = lazy(() => import('@/features/auth/SignUpPage').then(m => ({ default: m.SignUpPage })));
const CallbackPage = lazy(() => import('@/features/auth/CallbackPage').then(m => ({ default: m.CallbackPage })));

// Onboarding
const OnboardingPage = lazy(() => import('@/features/onboarding').then(m => ({ default: m.OnboardingPage })));

// Placeholder home
function HomePage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center">
      <h1 className="text-3xl font-bold mb-4">TadHub - Tenant Portal</h1>
      <p className="text-muted-foreground mb-8">
        Tadbeer ERP Platform for UAE Domestic Worker Recruitment
      </p>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-left max-w-2xl">
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">🧑‍💼 Workers</h3>
          <p className="text-sm text-muted-foreground">CV management, state machine, passport custody</p>
        </div>
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">👥 Clients</h3>
          <p className="text-sm text-muted-foreground">Employer management, verification, documents</p>
        </div>
        <div className="p-4 border rounded-lg">
          <h3 className="font-semibold mb-2">📊 Leads</h3>
          <p className="text-sm text-muted-foreground">Sales pipeline, follow-ups</p>
        </div>
      </div>
    </div>
  );
}

export function AppRouter() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/signup" element={<SignUpPage />} />
        <Route path="/callback" element={<CallbackPage />} />

        {/* Onboarding - for tenant selection */}
        <Route
          path="/onboarding"
          element={
            <ProtectedRoute requireOnboarding>
              <OnboardingPage />
            </ProtectedRoute>
          }
        />

        {/* Protected routes */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<HomePage />} />
          <Route path="dashboard" element={<HomePage />} />
          
          {/* Example permission-protected routes */}
          {/* <Route 
            path="workers/*" 
            element={
              <ProtectedRoute requiredPermission="workers.view">
                <WorkersModule />
              </ProtectedRoute>
            } 
          /> */}
          {/* <Route 
            path="settings/*" 
            element={
              <ProtectedRoute requiredRole="Admin">
                <SettingsModule />
              </ProtectedRoute>
            } 
          /> */}
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
