# Task 16: Placement — Inside Country Deployment Flow

## Summary
For maids already inside the country (including returnees), the placement flow includes a 5-day trial period before contract creation, followed by a simplified visa process.

## Current State
- Placement module doesn't distinguish between inside/outside country flows
- No trial period integration
- No flow differentiation based on worker location

## Required Changes

### 16.1 Flow Detection

- [ ] When creating a placement, detect worker's `LocationType` (InsideCountry vs OutsideCountry)
- [ ] Route to appropriate flow automatically
- [ ] Also detect returnees (workers with prior deployment history) — treat as inside country

### 16.2 Inside Country Step Sequence

| Step | Status | Notes |
|---|---|---|
| 1. Booking | `Booked` | No advance payment required (configurable) |
| 2. Trial Period | `InTrial` | 5-day trial contract (Task 02) |
| 3. Trial Outcome | `TrialSuccessful` or back to inventory | |
| 4. Contract Creation | `ContractCreated` | 2-year contract on trial success |
| 5. Status Change | `StatusChanged` | Worker status updated |
| 6. Employment Visa | `EmploymentVisaProcessing` | Passport + Photo required; Medical optional |
| 7. Residence Visa | `ResidenceVisaProcessing` | Local Medical + Passport + Photo |
| 8. Emirates ID | `EmiratesIdProcessing` | Track application/approval/issuance |

### 16.3 Trial Integration

- [ ] After booking, system prompts to create a trial (links to Task 02)
- [ ] Placement status reflects trial state
- [ ] If trial fails → placement cancelled, worker back to inventory
- [ ] If trial succeeds → proceed to contract creation

### 16.4 Differences from Outside Country

- [ ] No ticket processing step
- [ ] No arrival management step
- [ ] Trial period before contract (not after booking directly)
- [ ] Employment visa: medical is **optional** (not required)
- [ ] Payment timing may differ (configurable)

### 16.5 Frontend

- [ ] Placement creation form shows different steps based on worker location
- [ ] Step progress tracker adapts to inside-country flow (fewer steps, trial included)
- [ ] Clear visual indicator that this is an inside-country placement
- [ ] Trial status integrated into placement progress

### 16.6 API

- [ ] Placement creation auto-detects flow type based on worker location
- [ ] Step validation adapts to flow type
- [ ] `GET /api/placements/{id}/checklist` returns appropriate steps for the flow type

## Acceptance Criteria

1. System auto-detects inside-country placement based on worker location
2. Returnee workers are treated as inside-country placements
3. Trial period is initiated as the first step after booking
4. If trial fails, placement is cancelled and worker returns to inventory
5. If trial succeeds, 2-year contract is created automatically
6. Employment visa for inside-country requires only Passport + Photo (medical optional)
7. Residence visa requires Local Medical + Passport + Photo
8. Emirates ID is tracked after residence visa
9. No ticket or arrival steps for inside-country flow
10. Placement detail page shows the correct step sequence for inside-country flow
11. The placement board distinguishes between inside and outside country placements

## Dependencies
- Task 02 (Trial Period Workflow)
- Task 05 (Visa Processing Module)
