# Task 15: Placement — Outside Country Deployment Flow

## Summary
Align the Placement module's outside-country flow with the spec's 9-step process: Booking → Contract → Employment Visa → Ticket → Arrival → Deployment → Full Payment → Residence Visa → Emirates ID.

## Current State
- `Placement` module exists with 13 statuses: Booked, TicketArranged, InTransit, Arrived, MedicalInProgress, MedicalCleared, GovtProcessing, GovtCleared, Training, ReadyForPlacement, Placed, Completed, Cancelled
- No explicit step-by-step enforcement of the spec's flow
- No integration with visa module (not yet built)
- Contract creation is separate from placement flow

## Required Changes

### 15.1 Placement Status Alignment for Outside Country

Map the spec's 9 steps to placement statuses:

| Spec Step | Placement Status | Notes |
|---|---|---|
| 1. Booking | `Booked` | Partial/advance payment required |
| 2. Contract Creation | `ContractCreated` | 2-year contract generated |
| 3. Employment Visa | `EmploymentVisaProcessing` | Track via Visa module |
| 4. Ticket Processing | `TicketArranged` | Ticket issued, travel date set |
| 5. Arrival | `Arrived` | Arrival module handles details |
| 6. Deployment | `Deployed` | Maid deployed to customer |
| 7. Full Payment | `FullPaymentReceived` | Verify remaining balance paid |
| 8. Residence Visa | `ResidenceVisaProcessing` | Track via Visa module |
| 9. Emirates ID | `EmiratesIdProcessing` | Track via Visa module |

- [ ] Add new statuses or sub-steps to `PlacementStatus` enum if needed
- [ ] Or use a `PlacementStep` tracking entity alongside status

### 15.2 Step Enforcement

- [ ] Booking requires partial/advance payment — validate payment before proceeding
- [ ] Contract must be created before employment visa can start
- [ ] Employment visa must be approved before ticket can be arranged
- [ ] Ticket must be arranged before arrival can be tracked
- [ ] Full payment must be verified before or at deployment
- [ ] Residence visa follows after deployment
- [ ] Emirates ID follows after residence visa

### 15.3 Placement Checklist/Progress Tracker

- [ ] Create a step checklist on the placement detail page
- [ ] Each step shows: status (pending/in-progress/completed), required documents, actions
- [ ] Visual progress bar showing how far along the placement is

### 15.4 Integration Points

- [ ] **Step 1 (Booking)**: link to payment module — partial payment creation
- [ ] **Step 2 (Contract)**: auto-create or link to contract
- [ ] **Step 3 (Employment Visa)**: create visa application (Task 05)
- [ ] **Step 4 (Ticket)**: ticket details entry, trigger arrival creation (Task 06)
- [ ] **Step 5 (Arrival)**: link to arrival module (Task 06)
- [ ] **Step 6 (Deployment)**: update worker status, trigger commission (Task 11)
- [ ] **Step 7 (Full Payment)**: verify invoice is fully paid
- [ ] **Step 8 (Residence Visa)**: create visa application (Task 05)
- [ ] **Step 9 (Emirates ID)**: create visa application (Task 05)

### 15.5 API Updates

- [ ] `PUT /api/placements/{id}/advance-step` — move to next step (with validation)
- [ ] `GET /api/placements/{id}/checklist` — get step checklist with status
- [ ] Existing placement endpoints may need updates

### 15.6 Frontend

- [ ] Placement detail page: step-by-step progress view
- [ ] Each step expandable with details, documents, and actions
- [ ] Placement board: visual kanban or pipeline view showing maids at each step
- [ ] Quick actions per step (create contract, start visa, arrange ticket, etc.)

## Acceptance Criteria

1. Outside-country placement follows the 9-step process in order
2. Each step has validation — cannot skip steps
3. Booking requires partial/advance payment
4. 2-year contract is created at step 2
5. Employment visa application is linked at step 3
6. Ticket and travel details are tracked at step 4
7. Arrival is managed through the arrival module at step 5
8. Full payment is verified at step 7
9. Residence visa and Emirates ID are tracked at steps 8-9
10. Placement detail shows a visual progress tracker with all 9 steps
11. Placement board shows pipeline view of all active placements
12. Each step shows required documents and their upload status

## Dependencies
- Task 05 (Visa Processing) for steps 3, 8, 9
- Task 06 (Arrival Management) for step 5
- Task 11 (Finance Enhancements) for commission calculation at step 6
