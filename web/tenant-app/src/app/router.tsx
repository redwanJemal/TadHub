import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute';
import { DashboardLayout } from '@/shared/components/layout/DashboardLayout';
import { PageLoader } from '@/shared/components/ui/page-loader';
import { PermissionGate } from '@/shared/components/PermissionGate';

// Unauthorized fallback
function Unauthorized() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-4 py-20">
      <h1 className="text-2xl font-bold text-destructive">Access Denied</h1>
      <p className="text-muted-foreground">You do not have permission to view this page.</p>
    </div>
  );
}

/** Wraps a page with a permission check */
function Guarded({ permission, anyOf, children }: { permission?: string; anyOf?: string[]; children: React.ReactNode }) {
  return (
    <PermissionGate permission={permission} anyOf={anyOf} fallback={<Unauthorized />}>
      {children}
    </PermissionGate>
  );
}

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
const SupplierDetailPage = lazy(() => import('@/features/suppliers').then(m => ({ default: m.SupplierDetailPage })));

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


// Country Packages
const CountryPackagesPage = lazy(() => import('@/features/country-packages').then(m => ({ default: m.CountryPackagesPage })));
const CreateCountryPackagePage = lazy(() => import('@/features/country-packages').then(m => ({ default: m.CreateCountryPackagePage })));
const CountryPackageDetailPage = lazy(() => import('@/features/country-packages').then(m => ({ default: m.CountryPackageDetailPage })));

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
          {/* Dashboard */}
          <Route index element={<Guarded permission="dashboard.view"><DashboardPage /></Guarded>} />
          <Route path="dashboard" element={<Guarded permission="dashboard.view"><DashboardPage /></Guarded>} />

          {/* Team */}
          <Route path="team" element={<Guarded permission="members.view"><TeamPage /></Guarded>} />

          {/* Suppliers */}
          <Route path="suppliers" element={<Guarded permission="suppliers.view"><SuppliersPage /></Guarded>} />
          <Route path="suppliers/:id" element={<Guarded permission="suppliers.view"><SupplierDetailPage /></Guarded>} />

          {/* Candidates */}
          <Route path="candidates" element={<Guarded permission="candidates.view"><CandidatesPage /></Guarded>} />
          <Route path="candidates/new" element={<Guarded permission="candidates.create"><CreateCandidatePage /></Guarded>} />
          <Route path="candidates/:id" element={<Guarded permission="candidates.view"><CandidateDetailPage /></Guarded>} />
          <Route path="candidates/:id/edit" element={<Guarded permission="candidates.edit"><EditCandidatePage /></Guarded>} />

          {/* Clients */}
          <Route path="clients" element={<Guarded permission="clients.view"><ClientsPage /></Guarded>} />

          {/* Workers */}
          <Route path="workers" element={<Guarded permission="workers.view"><WorkersPage /></Guarded>} />
          <Route path="workers/:id" element={<Guarded permission="workers.view"><WorkerDetailPage /></Guarded>} />
          <Route path="workers/:id/cv" element={<Guarded permission="workers.view"><WorkerCvPage /></Guarded>} />

          {/* Contracts */}
          <Route path="contracts" element={<Guarded permission="contracts.view"><ContractsPage /></Guarded>} />
          <Route path="contracts/new" element={<Guarded permission="contracts.create"><CreateContractPage /></Guarded>} />
          <Route path="contracts/:id" element={<Guarded permission="contracts.view"><ContractDetailPage /></Guarded>} />

          {/* Placements */}
          <Route path="placements" element={<Guarded permission="placements.view"><PlacementBoardPage /></Guarded>} />
          <Route path="placements/new" element={<Guarded permission="placements.create"><CreatePlacementPage /></Guarded>} />
          <Route path="placements/:id" element={<Guarded permission="placements.view"><PlacementDetailPage /></Guarded>} />

          {/* Trials */}
          <Route path="trials" element={<Guarded permission="trials.view"><TrialsListPage /></Guarded>} />
          <Route path="trials/new" element={<Guarded permission="trials.create"><CreateTrialPage /></Guarded>} />
          <Route path="trials/:id" element={<Guarded permission="trials.view"><TrialDetailPage /></Guarded>} />

          {/* Returnees */}
          <Route path="returnees" element={<Guarded permission="returnees.view"><ReturneeCasesListPage /></Guarded>} />
          <Route path="returnees/new" element={<Guarded permission="returnees.create"><CreateReturneeCasePage /></Guarded>} />
          <Route path="returnees/:id" element={<Guarded permission="returnees.view"><ReturneeCaseDetailPage /></Guarded>} />

          {/* Runaways */}
          <Route path="runaways" element={<Guarded permission="runaways.view"><RunawayCasesListPage /></Guarded>} />
          <Route path="runaways/new" element={<Guarded permission="runaways.report"><ReportRunawayCasePage /></Guarded>} />
          <Route path="runaways/:id" element={<Guarded permission="runaways.view"><RunawayCaseDetailPage /></Guarded>} />

          {/* Visa Applications */}
          <Route path="visa-applications" element={<Guarded permission="visas.view"><VisaApplicationsListPage /></Guarded>} />
          <Route path="visa-applications/new" element={<Guarded permission="visas.create"><CreateVisaApplicationPage /></Guarded>} />
          <Route path="visa-applications/:id" element={<Guarded permission="visas.view"><VisaApplicationDetailPage /></Guarded>} />

          {/* Arrivals */}
          <Route path="arrivals" element={<Guarded permission="arrivals.view"><ArrivalsListPage /></Guarded>} />
          <Route path="arrivals/new" element={<Guarded permission="arrivals.create"><ScheduleArrivalPage /></Guarded>} />
          <Route path="arrivals/:id" element={<Guarded permission="arrivals.view"><ArrivalDetailPage /></Guarded>} />
          <Route path="driver" element={<Guarded permission="arrivals.driver_actions"><DriverDashboardPage /></Guarded>} />

          {/* Accommodations */}
          <Route path="accommodations" element={<Guarded permission="accommodations.view"><AccommodationListPage /></Guarded>} />
          <Route path="accommodations/check-in" element={<Guarded permission="accommodations.manage"><CheckInPage /></Guarded>} />
          <Route path="accommodations/:id" element={<Guarded permission="accommodations.view"><AccommodationDetailPage /></Guarded>} />

          {/* Finance */}
          <Route path="finance/invoices" element={<Guarded permission="invoices.view"><InvoicesListPage /></Guarded>} />
          <Route path="finance/invoices/new" element={<Guarded permission="invoices.create"><CreateInvoicePage /></Guarded>} />
          <Route path="finance/invoices/:invoiceId" element={<Guarded permission="invoices.view"><InvoiceDetailPage /></Guarded>} />
          <Route path="finance/payments" element={<Guarded permission="payments.view"><PaymentsListPage /></Guarded>} />
          <Route path="finance/payments/record" element={<Guarded permission="payments.create"><RecordPaymentPage /></Guarded>} />
          <Route path="finance/discount-programs" element={<Guarded permission="discounts.view"><DiscountProgramsPage /></Guarded>} />
          <Route path="finance/supplier-payments" element={<Guarded permission="supplier_payments.view"><SupplierPaymentsPage /></Guarded>} />
          <Route path="finance/supplier-debits" element={<Guarded permission="supplier_debits.view"><SupplierDebitsPage /></Guarded>} />
          <Route path="finance/reports" element={<Guarded permission="financial_reports.view"><FinancialReportsPage /></Guarded>} />
          <Route path="finance/cash-reconciliation" element={<Guarded permission="financial_reports.view"><CashReconciliationPage /></Guarded>} />
          <Route path="finance/settings" element={<Guarded permission="financial_reports.manage"><FinancialSettingsPage /></Guarded>} />


          {/* Reports Hub */}
          <Route path="reports" element={<Guarded permission="reports.view"><ReportsHubPage /></Guarded>} />
          <Route path="reports/inventory" element={<Guarded permission="reports.view"><InventoryReportPage /></Guarded>} />
          <Route path="reports/deployed" element={<Guarded permission="reports.view"><DeployedReportPage /></Guarded>} />
          <Route path="reports/returnees" element={<Guarded permission="reports.view"><ReturneeReportPage /></Guarded>} />
          <Route path="reports/runaways" element={<Guarded permission="reports.view"><RunawayReportPage /></Guarded>} />
          <Route path="reports/arrivals" element={<Guarded permission="reports.view"><ArrivalsReportPage /></Guarded>} />
          <Route path="reports/accommodation-daily" element={<Guarded permission="reports.view"><AccommodationDailyPage /></Guarded>} />
          <Route path="reports/deployment-pipeline" element={<Guarded permission="reports.view"><DeploymentPipelinePage /></Guarded>} />
          <Route path="reports/supplier-commissions" element={<Guarded permission="reports.view"><SupplierCommissionReportPage /></Guarded>} />
          <Route path="reports/refunds" element={<Guarded permission="reports.view"><RefundReportPage /></Guarded>} />
          <Route path="reports/cost-per-maid" element={<Guarded permission="reports.view"><CostPerMaidReportPage /></Guarded>} />

          {/* Country Packages */}
          <Route path="country-packages" element={<Guarded permission="packages.view"><CountryPackagesPage /></Guarded>} />
          <Route path="country-packages/new" element={<Guarded permission="packages.create"><CreateCountryPackagePage /></Guarded>} />
          <Route path="country-packages/:id" element={<Guarded permission="packages.view"><CountryPackageDetailPage /></Guarded>} />

          {/* Compliance & Documents */}
          <Route path="compliance" element={<Guarded permission="documents.view"><CompliancePage /></Guarded>} />

          {/* Audit */}
          <Route path="audit" element={<Guarded permission="audit.view"><AuditPage /></Guarded>} />

          {/* Settings */}
          <Route path="settings" element={<Navigate to="/settings/notifications" replace />} />
          <Route path="settings/:tab" element={<Guarded permission="settings.manage"><SettingsPage /></Guarded>} />

          {/* Notifications */}
          <Route path="notifications" element={<Guarded permission="notifications.view"><NotificationsPage /></Guarded>} />
          <Route path="notification-preferences" element={<Guarded permission="notifications.view"><NotificationPreferencesPage /></Guarded>} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  );
}
