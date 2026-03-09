# Task 04: Runaway Maid Process

## Summary
When a maid runs away, the system must record the case. If within the guarantee period, the supplier covers costs (commission refund, visa, medical, transportation, etc.). The maid is NOT returned to inventory.

## Current State
- `WorkerStatus.Absconded` exists but has no dedicated workflow
- No case management for runaway incidents
- No supplier cost recovery tracking

## Required Changes

### 4.1 Runaway Case Entity

- [ ] Create `RunawayCase` entity (SoftDeletableEntity):
  - `CaseCode` (auto-generated)
  - `WorkerId` (Guid)
  - `ContractId` (Guid)
  - `ClientId` (Guid)
  - `SupplierId` (Guid, nullable)
  - `Status` (enum: Reported, UnderInvestigation, Confirmed, Settled, Closed)
  - `ReportedDate` (DateTime)
  - `ReportedBy` (string)
  - `LastKnownLocation` (string, nullable)
  - `PoliceReportNumber` (string, nullable)
  - `PoliceReportDate` (DateTime, nullable)
  - `IsWithinGuarantee` (bool)
  - `GuaranteePeriodType` (enum, nullable)
  - `Notes` (string)
  - `ConfirmedAt`, `SettledAt`, `ClosedAt` (DateTime, nullable)
- [ ] Create `RunawayCaseStatusHistory` entity
- [ ] Create `RunawayExpense` entity (same structure as ReturneeExpense):
  - `RunawayCaseId`, `ExpenseType`, `Amount`, `Description`, `PaidBy`
  - ExpenseType: CommissionRefund, VisaCost, MedicalCost, TransportationCost, Other
- [ ] EF configuration and migration

### 4.2 Runaway Service

- [ ] `ReportRunawayAsync` — creates case, checks guarantee period, sets worker status to `Absconded`
- [ ] `ConfirmRunawayAsync` — marks case as confirmed after investigation
- [ ] `AddExpenseAsync` — track costs to be recovered from supplier
- [ ] `SettleCaseAsync` — mark financial settlement complete
- [ ] `CloseCaseAsync` — final closure
- [ ] On case creation:
  - Worker status → `Absconded`
  - Worker removed from inventory (not available for booking)
  - Contract terminated
  - If within guarantee → flag supplier liability
- [ ] Publish events: `RunawayCaseReportedEvent`, `RunawayCaseConfirmedEvent`

### 4.3 Supplier Cost Recovery

- [ ] When within guarantee period, auto-generate list of supplier liabilities:
  - Commission refund
  - Visa cost
  - Medical cost
  - Transportation cost
  - Other costs
- [ ] Link to `SupplierPayment` module for tracking recovery
- [ ] Supplier notification of liability

### 4.4 API

- [ ] `POST /api/runaway-cases` — report runaway
- [ ] `GET /api/runaway-cases` — list with filters
- [ ] `GET /api/runaway-cases/{id}` — detail
- [ ] `PUT /api/runaway-cases/{id}/confirm`
- [ ] `PUT /api/runaway-cases/{id}/settle`
- [ ] `PUT /api/runaway-cases/{id}/close`
- [ ] `POST /api/runaway-cases/{id}/expenses`
- [ ] Permissions: `runaways.view`, `runaways.report`, `runaways.manage`, `runaways.settle`

### 4.5 Frontend

- [ ] Runaway cases list page
- [ ] Report runaway form (select worker/contract, date, details, police report info)
- [ ] Case detail page:
  - Case info, timeline
  - Guarantee status and supplier liability
  - Expense tracking
  - Status progression actions
- [ ] Runaway section on worker detail page
- [ ] Sidebar navigation entry
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Office staff can report a maid as runaway
2. Worker status immediately changes to `Absconded`
3. Worker is removed from inventory — cannot be booked
4. Associated contract is terminated
5. System auto-detects if the runaway is within the guarantee period
6. If within guarantee:
   - Supplier liabilities are auto-calculated (commission, visa, medical, transport)
   - Supplier is notified
   - Expenses are tracked per line item
7. Case progresses through: Reported → UnderInvestigation → Confirmed → Settled → Closed
8. Police report number and date can be recorded
9. Financial settlement is tracked and can be marked complete
10. All status changes are audit-logged
11. Runaway maids are stored separately from inventory (visible in reports but not bookable)
