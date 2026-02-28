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

// Settings
const SettingsPage = lazy(() => import('@/features/settings').then(m => ({ default: m.SettingsPage })));

// Notifications (full page)
const NotificationsPage = lazy(() => import('@/features/notifications').then(m => ({ default: m.NotificationsPage })));

// Finance
const InvoicesListPage = lazy(() => import('@/features/finance').then(m => ({ default: m.InvoicesListPage })));
const CreateInvoicePage = lazy(() => import('@/features/finance').then(m => ({ default: m.CreateInvoicePage })));
const InvoiceDetailPage = lazy(() => import('@/features/finance').then(m => ({ default: m.InvoiceDetailPage })));
const PaymentsListPage = lazy(() => import('@/features/finance').then(m => ({ default: m.PaymentsListPage })));
const RecordPaymentPage = lazy(() => import('@/features/finance').then(m => ({ default: m.RecordPaymentPage })));
const DiscountProgramsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.DiscountProgramsPage })));
const SupplierPaymentsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.SupplierPaymentsPage })));
const FinancialReportsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.FinancialReportsPage })));
const CashReconciliationPage = lazy(() => import('@/features/finance').then(m => ({ default: m.CashReconciliationPage })));
const FinancialSettingsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.FinancialSettingsPage })));

// Dashboard
const DashboardPage = lazy(() => import('@/features/dashboard').then(m => ({ default: m.DashboardPage })));

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
          <Route index element={<DashboardPage />} />
          <Route path="dashboard" element={<DashboardPage />} />
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
          <Route path="finance/invoices" element={<InvoicesListPage />} />
          <Route path="finance/invoices/new" element={<CreateInvoicePage />} />
          <Route path="finance/invoices/:invoiceId" element={<InvoiceDetailPage />} />
          <Route path="finance/payments" element={<PaymentsListPage />} />
          <Route path="finance/payments/record" element={<RecordPaymentPage />} />
          <Route path="finance/discount-programs" element={<DiscountProgramsPage />} />
          <Route path="finance/supplier-payments" element={<SupplierPaymentsPage />} />
          <Route path="finance/reports" element={<FinancialReportsPage />} />
          <Route path="finance/cash-reconciliation" element={<CashReconciliationPage />} />
          <Route path="finance/settings" element={<FinancialSettingsPage />} />
          <Route path="compliance" element={<CompliancePage />} />
          <Route path="audit" element={<AuditPage />} />
          <Route path="settings" element={<Navigate to="/settings/notifications" replace />} />
          <Route path="settings/:tab" element={<SettingsPage />} />
          <Route path="notifications" element={<NotificationsPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
