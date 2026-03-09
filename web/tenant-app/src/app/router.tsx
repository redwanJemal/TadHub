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

// Placements
const PlacementBoardPage = lazy(() => import('@/features/placements').then(m => ({ default: m.PlacementBoardPage })));
const PlacementDetailPage = lazy(() => import('@/features/placements').then(m => ({ default: m.PlacementDetailPage })));
const CreatePlacementPage = lazy(() => import('@/features/placements').then(m => ({ default: m.CreatePlacementPage })));

// Trials
const TrialsListPage = lazy(() => import('@/features/trials').then(m => ({ default: m.TrialsListPage })));
const TrialDetailPage = lazy(() => import('@/features/trials').then(m => ({ default: m.TrialDetailPage })));
const CreateTrialPage = lazy(() => import('@/features/trials').then(m => ({ default: m.CreateTrialPage })));

// Returnees
const ReturneeCasesListPage = lazy(() => import('@/features/returnees').then(m => ({ default: m.ReturneeCasesListPage })));
const ReturneeCaseDetailPage = lazy(() => import('@/features/returnees').then(m => ({ default: m.ReturneeCaseDetailPage })));
const CreateReturneeCasePage = lazy(() => import('@/features/returnees').then(m => ({ default: m.CreateReturneeCasePage })));

// Runaways
const RunawayCasesListPage = lazy(() => import('@/features/runaways').then(m => ({ default: m.RunawayCasesListPage })));
const RunawayCaseDetailPage = lazy(() => import('@/features/runaways').then(m => ({ default: m.RunawayCaseDetailPage })));
const ReportRunawayCasePage = lazy(() => import('@/features/runaways').then(m => ({ default: m.ReportRunawayCasePage })));

// Visas
const VisaApplicationsListPage = lazy(() => import('@/features/visas').then(m => ({ default: m.VisaApplicationsListPage })));
const VisaApplicationDetailPage = lazy(() => import('@/features/visas').then(m => ({ default: m.VisaApplicationDetailPage })));
const CreateVisaApplicationPage = lazy(() => import('@/features/visas').then(m => ({ default: m.CreateVisaApplicationPage })));

// Arrivals
const ArrivalsListPage = lazy(() => import('@/features/arrivals').then(m => ({ default: m.ArrivalsListPage })));
const ArrivalDetailPage = lazy(() => import('@/features/arrivals').then(m => ({ default: m.ArrivalDetailPage })));
const ScheduleArrivalPage = lazy(() => import('@/features/arrivals').then(m => ({ default: m.ScheduleArrivalPage })));
const DriverDashboardPage = lazy(() => import('@/features/arrivals').then(m => ({ default: m.DriverDashboardPage })));

// Accommodations
const AccommodationListPage = lazy(() => import('@/features/accommodations').then(m => ({ default: m.AccommodationListPage })));
const AccommodationDetailPage = lazy(() => import('@/features/accommodations').then(m => ({ default: m.AccommodationDetailPage })));
const CheckInPage = lazy(() => import('@/features/accommodations').then(m => ({ default: m.CheckInPage })));

// Documents / Compliance
const CompliancePage = lazy(() => import('@/features/documents').then(m => ({ default: m.CompliancePage })));

// Audit
const AuditPage = lazy(() => import('@/features/audit').then(m => ({ default: m.AuditPage })));

// Settings
const SettingsPage = lazy(() => import('@/features/settings').then(m => ({ default: m.SettingsPage })));

// Notifications (full page)
const NotificationsPage = lazy(() => import('@/features/notifications').then(m => ({ default: m.NotificationsPage })));
const NotificationPreferencesPage = lazy(() => import('@/features/notifications').then(m => ({ default: m.NotificationPreferencesPage })));

// Finance
const InvoicesListPage = lazy(() => import('@/features/finance').then(m => ({ default: m.InvoicesListPage })));
const CreateInvoicePage = lazy(() => import('@/features/finance').then(m => ({ default: m.CreateInvoicePage })));
const InvoiceDetailPage = lazy(() => import('@/features/finance').then(m => ({ default: m.InvoiceDetailPage })));
const PaymentsListPage = lazy(() => import('@/features/finance').then(m => ({ default: m.PaymentsListPage })));
const RecordPaymentPage = lazy(() => import('@/features/finance').then(m => ({ default: m.RecordPaymentPage })));
const DiscountProgramsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.DiscountProgramsPage })));
const SupplierPaymentsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.SupplierPaymentsPage })));
const SupplierDebitsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.SupplierDebitsPage })));
const FinancialReportsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.FinancialReportsPage })));
const CashReconciliationPage = lazy(() => import('@/features/finance').then(m => ({ default: m.CashReconciliationPage })));
const FinancialSettingsPage = lazy(() => import('@/features/finance').then(m => ({ default: m.FinancialSettingsPage })));

// Supplier Portal
const SupplierDashboardPage = lazy(() => import('@/features/supplier-portal').then(m => ({ default: m.SupplierDashboardPage })));
const SupplierCandidatesPage = lazy(() => import('@/features/supplier-portal').then(m => ({ default: m.SupplierCandidatesPage })));
const SupplierWorkersPage = lazy(() => import('@/features/supplier-portal').then(m => ({ default: m.SupplierWorkersPage })));
const SupplierCommissionsPage = lazy(() => import('@/features/supplier-portal').then(m => ({ default: m.SupplierCommissionsPage })));
const SupplierArrivalsPage = lazy(() => import('@/features/supplier-portal').then(m => ({ default: m.SupplierArrivalsPage })));

// Reports
const ReportsHubPage = lazy(() => import('@/features/reports').then(m => ({ default: m.ReportsHubPage })));
const InventoryReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.InventoryReportPage })));
const DeployedReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.DeployedReportPage })));
const ReturneeReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.ReturneeReportPage })));
const RunawayReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.RunawayReportPage })));
const ArrivalsReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.ArrivalsReportPage })));
const AccommodationDailyPage = lazy(() => import('@/features/reports').then(m => ({ default: m.AccommodationDailyPage })));
const DeploymentPipelinePage = lazy(() => import('@/features/reports').then(m => ({ default: m.DeploymentPipelinePage })));
const SupplierCommissionReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.SupplierCommissionReportPage })));
const RefundReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.RefundReportPage })));
const CostPerMaidReportPage = lazy(() => import('@/features/reports').then(m => ({ default: m.CostPerMaidReportPage })));

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
          <Route path="placements" element={<PlacementBoardPage />} />
          <Route path="placements/new" element={<CreatePlacementPage />} />
          <Route path="placements/:id" element={<PlacementDetailPage />} />
          <Route path="trials" element={<TrialsListPage />} />
          <Route path="trials/new" element={<CreateTrialPage />} />
          <Route path="trials/:id" element={<TrialDetailPage />} />
          <Route path="returnees" element={<ReturneeCasesListPage />} />
          <Route path="returnees/new" element={<CreateReturneeCasePage />} />
          <Route path="returnees/:id" element={<ReturneeCaseDetailPage />} />
          <Route path="runaways" element={<RunawayCasesListPage />} />
          <Route path="runaways/new" element={<ReportRunawayCasePage />} />
          <Route path="runaways/:id" element={<RunawayCaseDetailPage />} />
          <Route path="visa-applications" element={<VisaApplicationsListPage />} />
          <Route path="visa-applications/new" element={<CreateVisaApplicationPage />} />
          <Route path="visa-applications/:id" element={<VisaApplicationDetailPage />} />
          <Route path="arrivals" element={<ArrivalsListPage />} />
          <Route path="arrivals/new" element={<ScheduleArrivalPage />} />
          <Route path="arrivals/:id" element={<ArrivalDetailPage />} />
          <Route path="driver" element={<DriverDashboardPage />} />
          <Route path="accommodations" element={<AccommodationListPage />} />
          <Route path="accommodations/check-in" element={<CheckInPage />} />
          <Route path="accommodations/:id" element={<AccommodationDetailPage />} />
          <Route path="finance/invoices" element={<InvoicesListPage />} />
          <Route path="finance/invoices/new" element={<CreateInvoicePage />} />
          <Route path="finance/invoices/:invoiceId" element={<InvoiceDetailPage />} />
          <Route path="finance/payments" element={<PaymentsListPage />} />
          <Route path="finance/payments/record" element={<RecordPaymentPage />} />
          <Route path="finance/discount-programs" element={<DiscountProgramsPage />} />
          <Route path="finance/supplier-payments" element={<SupplierPaymentsPage />} />
          <Route path="finance/supplier-debits" element={<SupplierDebitsPage />} />
          <Route path="finance/reports" element={<FinancialReportsPage />} />
          <Route path="finance/cash-reconciliation" element={<CashReconciliationPage />} />
          <Route path="finance/settings" element={<FinancialSettingsPage />} />
          <Route path="supplier-portal" element={<SupplierDashboardPage />} />
          <Route path="supplier-portal/candidates" element={<SupplierCandidatesPage />} />
          <Route path="supplier-portal/workers" element={<SupplierWorkersPage />} />
          <Route path="supplier-portal/commissions" element={<SupplierCommissionsPage />} />
          <Route path="supplier-portal/arrivals" element={<SupplierArrivalsPage />} />
          <Route path="reports" element={<ReportsHubPage />} />
          <Route path="reports/inventory" element={<InventoryReportPage />} />
          <Route path="reports/deployed" element={<DeployedReportPage />} />
          <Route path="reports/returnees" element={<ReturneeReportPage />} />
          <Route path="reports/runaways" element={<RunawayReportPage />} />
          <Route path="reports/arrivals" element={<ArrivalsReportPage />} />
          <Route path="reports/accommodation-daily" element={<AccommodationDailyPage />} />
          <Route path="reports/deployment-pipeline" element={<DeploymentPipelinePage />} />
          <Route path="reports/supplier-commissions" element={<SupplierCommissionReportPage />} />
          <Route path="reports/refunds" element={<RefundReportPage />} />
          <Route path="reports/cost-per-maid" element={<CostPerMaidReportPage />} />
          <Route path="compliance" element={<CompliancePage />} />
          <Route path="audit" element={<AuditPage />} />
          <Route path="settings" element={<Navigate to="/settings/notifications" replace />} />
          <Route path="settings/:tab" element={<SettingsPage />} />
          <Route path="notifications" element={<NotificationsPage />} />
          <Route path="notification-preferences" element={<NotificationPreferencesPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
