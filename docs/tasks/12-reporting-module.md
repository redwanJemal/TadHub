# Task 12: Reporting Module Expansion

## Summary
The spec requires comprehensive reporting beyond the current financial reports. Need inventory, deployment, returnee, runaway, arrival, and accommodation reports.

## Current State
- `FinancialReportsPage` exists with financial reporting
- `FinancialReportService` in Financial.Core
- No operational or workforce reports

## Required Changes

### 12.1 Maid/Workforce Reports

- [ ] **Inventory Report**: all maids currently in inventory, filterable by nationality, location, supplier, availability
- [ ] **Deployed Report**: all currently deployed maids with contract info, client, deployment date
- [ ] **Returnee Report**: all returnee cases with status, refund amounts, settlement status
- [ ] **Runaway Report**: all runaway cases with status, guarantee info, cost recovery status

### 12.2 Operational Reports

- [ ] **Arrivals Report**: arrivals by date range, status (scheduled/arrived/no-show), driver performance
- [ ] **Accommodation Daily List**: printable daily list of all maids in accommodation
- [ ] **Deployment Status Report**: pipeline view — how many maids at each stage (booked, visa processing, traveling, arrived, deployed)

### 12.3 Finance Reports (Extensions)

- [ ] **Supplier Commission Report**: commissions paid/pending per supplier, per period
- [ ] **Refund Report**: customer refunds issued, amounts, linked returnee cases
- [ ] **Cost Report per Maid**: total cost breakdown per maid (procurement, visa, medical, transport, accommodation)

### 12.4 Backend

- [ ] Create `ReportService` or extend existing report services per module
- [ ] Each report should support:
  - Date range filtering
  - Status filtering
  - Export to CSV/Excel
  - Pagination
- [ ] API endpoints for each report type

### 12.5 Frontend

- [ ] Reports hub page or expand existing reports section
- [ ] Each report type as a sub-page:
  - Filter controls
  - Data table with results
  - Export button (CSV at minimum)
  - Print-friendly layout
- [ ] Sidebar: expand reporting section or add "Reports" parent menu
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Inventory report shows all maids in inventory with filters for nationality, location, supplier, availability
2. Deployed report shows all currently deployed maids with contract and client details
3. Returnee report shows all returnee cases with refund calculations and settlement status
4. Runaway report shows all runaway cases with guarantee and cost recovery details
5. Arrivals report shows arrivals by date range with status breakdown
6. Accommodation daily list is available for any date and is printable
7. Deployment pipeline report shows counts per stage
8. Supplier commission report shows per-supplier totals for any period
9. All reports support date range filtering and CSV export
10. Reports are accessible based on user permissions

## Dependencies
- Tasks 03, 04 (Returnee/Runaway) for those reports
- Task 06 (Arrival) for arrival reports
- Task 07 (Accommodation) for accommodation reports
