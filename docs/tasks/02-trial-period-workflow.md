# Task 02: Trial Period Workflow

## Summary
For maids already inside the country (including returnees), a 5-day trial period must be initiated before a full 2-year contract is created. This is a completely new workflow.

## Current State
- `ContractStatus` has `OnProbation` but no dedicated trial logic
- `Placement` module tracks booking-to-deployment but has no trial step
- No trial contract generation exists
- No trial outcome tracking exists

## Required Changes

### 2.1 Trial Entity & Backend

**New entities in Contract or Placement module:**

- [ ] Create `Trial` entity (SoftDeletableEntity):
  - `TrialCode` (auto-generated, human-readable)
  - `WorkerId` (Guid)
  - `ClientId` (Guid)
  - `PlacementId` (Guid, nullable — link to placement if applicable)
  - `StartDate` (DateOnly)
  - `EndDate` (DateOnly) — auto-calculated as StartDate + 5 days
  - `Status` (enum: Active, Successful, Failed, Cancelled)
  - `Outcome` (enum: ProceedToContract, ReturnToInventory, nullable)
  - `OutcomeNotes` (string, nullable)
  - `OutcomeDate` (DateTime, nullable)
  - `ContractId` (Guid, nullable — set when 2-year contract is created on success)
- [ ] Create `TrialStatusHistory` entity for audit trail
- [ ] EF configuration and migration

### 2.2 Trial Service

- [ ] `CreateTrialAsync` — validates worker is InsideCountry or Returnee, creates 5-day trial
- [ ] `CompleteTrialAsync(trialId, outcome)`:
  - If **Successful**: auto-create 2-year contract, change worker status, initiate employment visa process
  - If **Failed**: return worker to inventory (Available status), update placement status
- [ ] `CancelTrialAsync` — cancels an active trial
- [ ] `GetTrialsByWorkerAsync`, `GetTrialsByClientAsync`
- [ ] Publish events: `TrialCreatedEvent`, `TrialCompletedEvent`

### 2.3 Trial API

- [ ] `POST /api/trials` — create trial
- [ ] `GET /api/trials` — list trials with filtering
- [ ] `GET /api/trials/{id}` — trial detail
- [ ] `PUT /api/trials/{id}/complete` — mark outcome
- [ ] `PUT /api/trials/{id}/cancel` — cancel trial
- [ ] Add `trials.view`, `trials.create`, `trials.manage` permissions

### 2.4 Frontend

- [ ] Trial list page (filterable by status, client, worker)
- [ ] Create trial form (select worker + client, start date auto-sets end date)
- [ ] Trial detail page showing status, countdown, outcome
- [ ] Complete trial dialog with Successful/Failed options
- [ ] On success: prompt to create contract or auto-navigate to contract creation
- [ ] Add trial section to worker detail page (trial history)
- [ ] Sidebar navigation entry for Trials
- [ ] i18n keys (en, ar)

### 2.5 Integration with Placement

- [ ] When placement is for an inside-country worker, the flow should route through trial first
- [ ] Placement status should reflect trial state (e.g., `InTrial` status or similar)
- [ ] On trial success, placement continues to visa processing
- [ ] On trial failure, placement is cancelled or returned

## Acceptance Criteria

1. Office staff can initiate a 5-day trial for any inside-country or returnee worker
2. Trial auto-calculates end date as start date + 5 days
3. A trial period contract/document can be generated
4. When trial is marked **Successful**:
   - A 2-year employment contract is automatically created (or user is prompted to create one)
   - Worker status changes appropriately
   - Employment visa process can begin
5. When trial is marked **Failed**:
   - Worker returns to inventory with `Available` status
   - Associated placement is cancelled
6. Trial history is visible on worker and client detail pages
7. Trial status changes are logged in audit trail
8. Notifications are sent on trial creation and completion
9. Only inside-country and returnee workers can be put on trial (validation enforced)
10. Active trials cannot be duplicated for the same worker (one active trial per worker at a time)
