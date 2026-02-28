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

// Team
const TeamPage = lazy(() => import('@/features/team').then(m => ({ default: m.TeamPage })));

// Suppliers
const SuppliersPage = lazy(() => import('@/features/suppliers').then(m => ({ default: m.SuppliersPage })));

// Candidates
const CandidatesPage = lazy(() => import('@/features/candidates').then(m => ({ default: m.CandidatesPage })));
const CreateCandidatePage = lazy(() => import('@/features/candidates').then(m => ({ default: m.CreateCandidatePage })));
const CandidateDetailPage = lazy(() => import('@/features/candidates').then(m => ({ default: m.CandidateDetailPage })));
const EditCandidatePage = lazy(() => import('@/features/candidates').then(m => ({ default: m.EditCandidatePage })));

// Clients
const ClientsPage = lazy(() => import('@/features/clients').then(m => ({ default: m.ClientsPage })));

// Workers
const WorkersPage = lazy(() => import('@/features/workers').then(m => ({ default: m.WorkersPage })));
const WorkerDetailPage = lazy(() => import('@/features/workers').then(m => ({ default: m.WorkerDetailPage })));
const WorkerCvPage = lazy(() => import('@/features/workers').then(m => ({ default: m.WorkerCvPage })));

// Contracts
const ContractsPage = lazy(() => import('@/features/contracts').then(m => ({ default: m.ContractsPage })));
const CreateContractPage = lazy(() => import('@/features/contracts').then(m => ({ default: m.CreateContractPage })));
const ContractDetailPage = lazy(() => import('@/features/contracts').then(m => ({ default: m.ContractDetailPage })));

// Documents / Compliance
const CompliancePage = lazy(() => import('@/features/documents').then(m => ({ default: m.CompliancePage })));

// Audit
const AuditPage = lazy(() => import('@/features/audit').then(m => ({ default: m.AuditPage })));

// Placeholder home
function HomePage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center">
      <h1 className="text-3xl font-bold mb-4">TadHub - Tenant Portal</h1>
      <p className="text-muted-foreground">
        Welcome to your workspace. Select an option from the sidebar to get started.
      </p>
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
          <Route path="team" element={<TeamPage />} />
          <Route path="suppliers" element={<SuppliersPage />} />
          <Route path="candidates" element={<CandidatesPage />} />
          <Route path="candidates/new" element={<CreateCandidatePage />} />
          <Route path="candidates/:id" element={<CandidateDetailPage />} />
          <Route path="candidates/:id/edit" element={<EditCandidatePage />} />
          <Route path="clients" element={<ClientsPage />} />
          <Route path="contracts" element={<ContractsPage />} />
          <Route path="contracts/new" element={<CreateContractPage />} />
          <Route path="contracts/:id" element={<ContractDetailPage />} />
          <Route path="workers" element={<WorkersPage />} />
          <Route path="workers/:id" element={<WorkerDetailPage />} />
          <Route path="workers/:id/cv" element={<WorkerCvPage />} />
          <Route path="compliance" element={<CompliancePage />} />
          <Route path="audit" element={<AuditPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
