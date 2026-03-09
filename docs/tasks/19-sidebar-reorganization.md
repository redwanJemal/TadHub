# Task 19: Sidebar Navigation Reorganization

## Summary
Reorganize the sidebar navigation to group items logically according to the Tadbeer business workflow: Recruitment, Sales & Inventory, Operations, Finance, Cases, and Administration.

## Current State
```
Dashboard
Team
Suppliers
Candidates
Clients
Workers
Placements
Contracts
Finance >
  Invoices
  Payments
  Financial Reports
  Financial Settings
Compliance
Audit
Settings >
  General
  Notifications
```

Flat structure with no logical grouping beyond Finance and Settings.

## Proposed Structure

```
Dashboard

RECRUITMENT
  Suppliers
  Candidates

INVENTORY & SALES
  Workers (Inventory)
  Clients
  Placements (Bookings)
  Trials
  Contracts

OPERATIONS
  Arrivals
  Accommodation
  Visa Processing
  Compliance (Documents)

FINANCE
  Invoices
  Payments
  Supplier Payments
  Discount Programs
  Reports
  Settings

CASES
  Returnees
  Runaways

ADMIN
  Team
  Country Packages
  Audit
  Settings >
    General
    Notifications
```

## Required Changes

### 19.1 Section Headers

- [ ] Add visual section headers/dividers to the sidebar
- [ ] Section headers should be subtle (uppercase, small text, muted color)
- [ ] Sections collapse/expand independently (or always visible with just headers)
- [ ] In collapsed sidebar mode, section headers are hidden (just icons)

### 19.2 Navigation Items Update

- [ ] Add new nav items for new modules (as they are built):
  - Trials (Task 02)
  - Arrivals (Task 06)
  - Accommodation (Task 07)
  - Visa Processing (Task 05)
  - Returnees (Task 03)
  - Runaways (Task 04)
  - Country Packages (Task 18)
- [ ] Move Supplier Payments and Discount Programs into Finance section (currently they exist as pages but aren't prominent in nav)
- [ ] Move Compliance under Operations
- [ ] Move Team and Audit under Admin

### 19.3 Finance Section

Finance should be a collapsible group (already is) but expanded:
- [ ] Invoices
- [ ] Payments
- [ ] Supplier Payments
- [ ] Discount Programs
- [ ] Cash Reconciliation
- [ ] Reports
- [ ] Settings

### 19.4 Icon Selection

- [ ] Select appropriate Lucide icons for new items:
  - Trials: `Scale` or `Timer`
  - Arrivals: `PlaneLanding`
  - Accommodation: `Building2` or `Hotel`
  - Visa Processing: `Stamp` or `FileCheck`
  - Returnees: `RotateCcw` or `UserMinus`
  - Runaways: `UserX` or `AlertTriangle`
  - Country Packages: `Package`

### 19.5 Permission Gating

Each nav item must be permission-gated:
- [ ] `trials.view` for Trials
- [ ] `arrivals.view` for Arrivals
- [ ] `accommodation.view` for Accommodation
- [ ] `visas.view` for Visa Processing
- [ ] `returnees.view` for Returnees
- [ ] `runaways.view` for Runaways (or `runaways.report`)
- [ ] `packages.view` for Country Packages

### 19.6 Responsive Behavior

- [ ] Section headers hidden in collapsed sidebar
- [ ] Mobile sidebar shows full navigation with sections
- [ ] Active item highlighting works within sections

### 19.7 i18n

- [ ] Add i18n keys for all section headers:
  - `nav.section_recruitment`
  - `nav.section_sales`
  - `nav.section_operations`
  - `nav.section_finance`
  - `nav.section_cases`
  - `nav.section_admin`
- [ ] Add i18n keys for new nav items
- [ ] English and Arabic translations

## Acceptance Criteria

1. Sidebar is organized into clearly labeled sections
2. Section headers are visually distinct but subtle (not competing with nav items)
3. All new modules have corresponding nav entries (added as modules are built)
4. Finance section is expanded with all sub-items
5. Each nav item is permission-gated — users only see items they have access to
6. Empty sections (where user has no permissions) are hidden entirely
7. Collapsed sidebar mode works correctly (icons only, no section headers)
8. Mobile sidebar displays all sections correctly
9. Active item highlighting works across all sections
10. i18n translations exist for all section headers and nav items (en, ar)

## Implementation Note
This task can be partially implemented now (restructure existing items) and incrementally updated as new modules from other tasks are built. Each new module task should include "add sidebar nav entry" as a subtask.
