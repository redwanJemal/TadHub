# Tadbeer System — Implementation Task Overview

This document indexes all implementation tasks required to align TadHub with the Tadbeer customer specification.

## Task Files

### Phase 1 — Core Business Flow (High Priority)

| # | File | Summary |
|---|------|---------|
| 01 | [01-candidate-registration-gaps.md](01-candidate-registration-gaps.md) | Missing fields, inside/outside classification at registration, document type expansion |
| 02 | [02-trial-period-workflow.md](02-trial-period-workflow.md) | 5-day trial for inside-country maids, trial contract, outcome tracking |
| 03 | [03-returnee-process.md](03-returnee-process.md) | Returnee form, approval, refund calculation, guarantee period logic |
| 04 | [04-runaway-maid-process.md](04-runaway-maid-process.md) | Case recording, guarantee check, supplier cost recovery |
| 05 | [05-visa-processing-module.md](05-visa-processing-module.md) | Employment visa, residence visa, Emirates ID tracking |
| 06 | [06-arrival-management.md](06-arrival-management.md) | Driver assignment, photo uploads, non-arrival alerts |

### Phase 2 — Operations & Roles (Medium Priority)

| # | File | Summary |
|---|------|---------|
| 07 | [07-accommodation-module.md](07-accommodation-module.md) | Daily maid list, room assignment, departure tracking |
| 08 | [08-driver-role.md](08-driver-role.md) | New user role, pickup confirmation, photo upload |
| 09 | [09-supplier-portal.md](09-supplier-portal.md) | Self-service portal for suppliers to register maids, track status, view commissions |
| 10 | [10-accommodation-staff-role.md](10-accommodation-staff-role.md) | New role with daily list management permissions |

### Phase 3 — Finance & Reporting (Medium Priority)

| # | File | Summary |
|---|------|---------|
| 11 | [11-finance-enhancements.md](11-finance-enhancements.md) | Refund engine, amortization, guarantee-period cost recovery, commission auto-calc |
| 12 | [12-reporting-module.md](12-reporting-module.md) | Inventory, deployment, returnee, runaway, arrival, accommodation reports |

### Phase 4 — Status Lifecycle & Notifications

| # | File | Summary |
|---|------|---------|
| 13 | [13-maid-status-lifecycle.md](13-maid-status-lifecycle.md) | Align worker statuses to spec's 13-state lifecycle |
| 14 | [14-notifications-expansion.md](14-notifications-expansion.md) | Missing notification triggers for all business events |

### Phase 5 — Placement & Contract Enhancements

| # | File | Summary |
|---|------|---------|
| 15 | [15-placement-outside-country-flow.md](15-placement-outside-country-flow.md) | Full 9-step outside-country deployment process |
| 16 | [16-placement-inside-country-flow.md](16-placement-inside-country-flow.md) | Inside-country flow with trial integration |
| 17 | [17-contract-enhancements.md](17-contract-enhancements.md) | Guarantee period, contract type alignment, replacement contracts |

### Phase 6 — Reference Data & UI

| # | File | Summary |
|---|------|---------|
| 18 | [18-country-payment-packages.md](18-country-payment-packages.md) | Government-defined payment packages per country, auto-fill at contract/placement level, overridable |
| 19 | [19-sidebar-reorganization.md](19-sidebar-reorganization.md) | Regroup sidebar into Recruitment, Sales, Operations, Finance, Cases, Admin sections |
