# Task 03: Returnee Process

## Summary
When a maid returns before her 2-year contract is complete, a returnee workflow must be initiated. There are two sub-flows: **Return to Office** (maid goes back to inventory) and **Return to Country** (maid leaves, supplier may be liable).

## Current State
- `WorkerStatus` has states that partially map: `Absconded`, `Terminated`, `Repatriated`
- No returnee form or approval workflow exists
- No refund calculation logic exists
- No guarantee period tracking on contracts
- `ContractStatus` has `Terminated` but no returnee-specific handling

## Required Changes

### 3.1 Returnee Entity & Backend

- [ ] Create `ReturneeCase` entity (SoftDeletableEntity):
  - `CaseCode` (auto-generated)
  - `WorkerId` (Guid)
  - `ContractId` (Guid)
  - `ClientId` (Guid)
  - `SupplierId` (Guid, nullable)
  - `ReturnType` (enum: ReturnToOffice, ReturnToCountry)
  - `Status` (enum: Submitted, UnderReview, Approved, Rejected, Settled)
  - `ReturnDate` (DateTime)
  - `ReturnReason` (string)
  - `MonthsWorked` (int, calculated)
  - `IsWithinGuarantee` (bool, calculated from contract guarantee period)
  - `GuaranteePeriodType` (enum: SixMonths, OneYear, TwoYears, nullable)
  - Notes, ApprovedBy, ApprovedAt, RejectedReason
- [ ] Create `ReturneeCase_StatusHistory` entity
- [ ] Create `ReturneeExpense` entity:
  - `ReturneeCaseId` (Guid)
  - `ExpenseType` (enum: VisaCost, TicketCost, MedicalCost, TransportationCost, AccommodationCost, Other)
  - `Amount` (decimal)
  - `Description` (string)
  - `PaidBy` (enum: Office, Supplier, Client)
- [ ] EF configuration and migration

### 3.2 Refund Calculation Service

- [ ] Implement amortization refund formula:
  ```
  ValuePerMonth = TotalAmountPaid / 24
  RefundAmount = TotalAmountPaid - (MonthsWorked * ValuePerMonth)
  ```
- [ ] `CalculateRefundAsync(contractId, returnDate)` — returns refund breakdown
- [ ] Support partial month calculation (pro-rata or round down)

### 3.3 Returnee Service

- [ ] `SubmitReturneeCaseAsync` — creates case, calculates months worked and guarantee status
- [ ] `ApproveReturneeCaseAsync`:
  - **Return to Office**:
    - Worker status → `Available` (back to inventory)
    - Worker location → `InCountry`
    - Worker is treated as inside-country for future bookings
    - Customer refund is calculated and recorded
  - **Return to Country**:
    - Worker status → `Repatriated` (NOT back to inventory)
    - If within guarantee: supplier is liable for commission refund + costs
    - Record all expenses (visa, ticket, medical, transport, etc.)
- [ ] `RejectReturneeCaseAsync`
- [ ] `AddExpenseAsync` — add expense line items to a case
- [ ] `SettleCaseAsync` — mark case as financially settled
- [ ] Publish events: `ReturneeCaseCreatedEvent`, `ReturneeCaseApprovedEvent`, `ReturneeCaseSettledEvent`

### 3.4 Contract Integration

- [ ] Add `GuaranteePeriod` field to `Contract` entity (enum: SixMonths, OneYear, TwoYears)
- [ ] Add `GuaranteeEndDate` (calculated from contract start + guarantee period)
- [ ] On returnee approval, terminate the contract with reason

### 3.5 Financial Integration

- [ ] Auto-generate credit note / refund invoice for customer refund (Return to Office)
- [ ] Auto-generate supplier debit for commission refund (Return to Country within guarantee)
- [ ] Link refund records to `ReturneeCase`

### 3.6 API

- [ ] `POST /api/returnee-cases` — submit returnee case
- [ ] `GET /api/returnee-cases` — list with filters (status, type, worker, client)
- [ ] `GET /api/returnee-cases/{id}` — detail with expenses
- [ ] `PUT /api/returnee-cases/{id}/approve`
- [ ] `PUT /api/returnee-cases/{id}/reject`
- [ ] `PUT /api/returnee-cases/{id}/settle`
- [ ] `POST /api/returnee-cases/{id}/expenses` — add expense
- [ ] `GET /api/returnee-cases/{id}/refund-calculation` — preview refund
- [ ] Permissions: `returnees.view`, `returnees.create`, `returnees.manage`, `returnees.settle`

### 3.7 Frontend

- [ ] Returnee cases list page (filterable by status, type)
- [ ] Submit returnee case form (select worker/contract, return type, reason, date)
- [ ] Returnee case detail page:
  - Case info, status timeline
  - Refund calculation breakdown
  - Expense list with add/edit
  - Approve/Reject actions
  - Settle action
- [ ] Returnee section on worker detail page (case history)
- [ ] Returnee section on contract detail page
- [ ] Sidebar navigation entry
- [ ] i18n keys (en, ar)

## Acceptance Criteria

1. Office staff can submit a returnee case for any worker with an active contract
2. System auto-calculates months worked and whether the return is within guarantee
3. **Return to Office**:
   - On approval, worker returns to inventory as `Available` + `InCountry`
   - Customer refund is calculated using the amortization formula
   - A credit note / refund is generated in the financial module
   - The worker can be booked again as an inside-country maid
4. **Return to Country**:
   - On approval, worker is marked `Repatriated` and is NOT in inventory
   - If within guarantee, supplier liabilities are recorded (commission, ticket, etc.)
   - All expenses are tracked with who pays (Office, Supplier, Client)
5. The associated contract is terminated with the returnee reason
6. Refund preview is available before approval
7. Case status changes are logged in audit trail
8. Notifications sent to relevant parties (admin, supplier if within guarantee)
9. Financial settlements can be marked as complete
10. Returnee history is visible on worker, contract, and client detail pages
