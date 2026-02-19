# Phase 9: Reporting & Dashboards

Role-based dashboards and operational reports. Each role sees exactly what they need. MoHRE compliance reports auto-populated from system data.

**Estimated Time:** 2 weeks (parallel with P8)

---

## P9-T01: Implement Reporting Module with Role-Based Dashboards and MoHRE Reports

**Dependencies:** All previous phases

**Files:**
- `src/Modules/Tadbeer/Reporting/Reporting.Core/Services/DashboardService.cs`
- `src/Modules/Tadbeer/Reporting/Reporting.Core/Services/XReportExporter.cs`
- `src/Modules/Tadbeer/Reporting/Reporting.Core/Services/MoHREReportService.cs`
- `src/Modules/Tadbeer/Reporting/Reporting.Core/Services/CustomReportService.cs`
- `src/TadHub.Api/Controllers/Tadbeer/DashboardsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/ReportsController.cs`
- `src/Modules/Tadbeer/Reporting/Reporting.Core/ReportingServiceRegistration.cs`

**Instructions:**
1. `DashboardService`: Role-based data aggregation.
   - **ReceptionDashboard**: open leads, conversion rate, available workers by category, pending client verifications
   - **CashierDashboard**: today's collections by method, pending payments, refund queue
   - **ProDashboard**: pending transactions by urgency (color-coded), overdue documents
   - **ManagerDashboard**: revenue daily/weekly/monthly, active contracts, worker utilization, refund rate
   - **PlatformAdminDashboard**: tenant activity, subscription health, aggregate revenue
2. Each dashboard is a single GET endpoint returning a DTO with pre-aggregated numbers. Heavy queries are cached in Redis with 5-minute TTL. SSE push on data changes invalidates cache.
3. `MoHREReportService`: Pre-formatted reports for license renewal and regulatory audits. Worker inventory by status/nationality, contract summary, compliance status. Output in the format MoHRE expects.
4. `CustomReportService`: User-configurable filters: date range, nationality, contract type, client category, worker status, payment method. Save report templates. Export as PDF/Excel/CSV.
5. X-Report: already implemented in Financial module; reporting module adds export and historical query.
6. API: GET `.../dashboards/reception`, GET `.../dashboards/cashier`, GET `.../dashboards/pro`, GET `.../dashboards/manager`, GET `.../dashboards/platform-admin`. GET `.../reports/mohre?type=workerInventory&period=2026-01`. POST `.../reports/custom` (with filter body).

**Tests:**
- [ ] Unit test: ReceptionDashboard aggregates correct lead and worker counts
- [ ] Unit test: CashierDashboard includes today's X-Report totals
- [ ] Unit test: MoHRE report includes all required fields per compliance spec

**Acceptance:** Every role has a purpose-built dashboard. MoHRE reports auto-generated.

**Status:** ⏳ Pending
