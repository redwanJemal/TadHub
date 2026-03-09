# Task 13: Maid Status Lifecycle Alignment

## Summary
Align the worker status lifecycle to match the spec's 13-state maid lifecycle, ensuring full traceability from registration to contract completion or exit.

## Current State

**Candidate statuses**: Received, UnderReview, Approved, Rejected, Cancelled
**Worker statuses (18)**: Available, InTraining, UnderMedicalTest, NewArrival, Booked, Hired, OnProbation, Active, Renewed, PendingReplacement, Transferred, MedicallyUnfit, Absconded, Terminated, Pregnant, Repatriated, Deported, Deceased

**Spec statuses (13)**: Registered, PendingApproval, Approved, Inventory, Booked, VisaProcessing, Traveling, Arrived, Accommodation, Deployed, Returnee, Runaway, ReturnedToCountry

## Required Changes

### 13.1 Status Mapping

| Spec Status | Current Mapping | Action Needed |
|---|---|---|
| Registered | Candidate.Received | OK |
| Pending Approval | Candidate.UnderReview | OK |
| Approved | Candidate.Approved → Worker created | OK |
| Inventory | Worker.Available | OK |
| Booked | Worker.Booked | OK |
| Visa Processing | No direct mapping | Add `VisaProcessing` to WorkerStatus or track via VisaApplication |
| Traveling | No direct mapping | Add `Traveling` to WorkerStatus or track via Placement.InTransit |
| Arrived | Worker.NewArrival | Rename to `Arrived` or map |
| Accommodation | Not in WorkerStatus | Add `InAccommodation` to WorkerStatus |
| Deployed | Worker.Active | OK (map Deployed = Active) |
| Returnee | Not a status, it's a process | Add `Returnee` to WorkerStatus or handle via ReturneeCase |
| Runaway | Worker.Absconded | OK (Absconded = Runaway) |
| Returned to Country | Worker.Repatriated | OK |

### 13.2 Changes Needed

- [ ] Add to `WorkerStatus` enum:
  - `VisaProcessing`
  - `Traveling`
  - `InAccommodation`
  - `Returnee`
- [ ] Consider keeping additional statuses that are useful (MedicallyUnfit, Deported, Deceased) even if not in spec — they may be needed operationally
- [ ] Update worker status transition logic to enforce valid transitions
- [ ] EF migration for new enum values

### 13.3 Status Transition Rules

Define valid transitions:
```
Available → Booked
Booked → VisaProcessing, Available (if booking cancelled)
VisaProcessing → Traveling, Available (if visa rejected)
Traveling → Arrived, Available (if travel cancelled)
Arrived → InAccommodation
InAccommodation → Active (Deployed)
Active → Returnee, Absconded (Runaway), Repatriated (Returned to Country)
Returnee → Available (Return to Office), Repatriated (Return to Country)
```

- [ ] Implement status transition validation in WorkerService
- [ ] Return clear error messages for invalid transitions

### 13.4 Unified Lifecycle View

- [ ] Create a visual status timeline on worker detail page showing full journey
- [ ] Each status change logged with timestamp, changed by, reason
- [ ] Frontend: status badge colors aligned to lifecycle stages

### 13.5 Frontend Updates

- [ ] Update WorkerStatus display names and colors across all pages
- [ ] Update workers list filters to include new statuses
- [ ] Update worker detail status history timeline
- [ ] Add lifecycle diagram or progress indicator on worker detail
- [ ] i18n for new status names

## Acceptance Criteria

1. Worker entity supports all 13 spec statuses (plus any operational extras)
2. Status transitions are validated — only valid transitions are allowed
3. Invalid transition attempts return clear error messages
4. Worker detail page shows a visual timeline of the full lifecycle
5. All status changes are logged with timestamp, user, and reason
6. Workers list can filter by any status
7. Status badge colors are consistent and meaningful across the UI
8. Existing workers with current statuses are not broken by migration
9. The lifecycle supports both inside-country and outside-country paths
