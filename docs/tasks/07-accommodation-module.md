# Task 07: Accommodation Management Module

## Summary
Track maids staying in office accommodation. Maintain a daily list, register arrivals and departures with reasons.

## Current State
- No accommodation tracking exists in the system
- Placement has no accommodation concept

## Required Changes

### 7.1 Accommodation Entities

- [ ] Create `AccommodationStay` entity (SoftDeletableEntity):
  - `StayCode` (auto-generated)
  - `WorkerId` (Guid)
  - `PlacementId` (Guid, nullable)
  - `ArrivalId` (Guid, nullable — link to Arrival record)
  - `CheckInDate` (DateTime)
  - `CheckOutDate` (DateTime, nullable)
  - `Room` (string, nullable)
  - `Location` (string, nullable) — building, floor, etc.
  - `Status` (enum: CheckedIn, CheckedOut)
  - `DepartureReason` (enum: DeployedToCustomer, Runaway, ReturnedToCountry, Transferred, MedicalReason, Other, nullable)
  - `DepartureNotes` (string, nullable)
  - `CheckedInBy` (string)
  - `CheckedOutBy` (string, nullable)
- [ ] EF configuration and migration

### 7.2 Accommodation Service

- [ ] `CheckInAsync` — register maid arrival at accommodation
- [ ] `CheckOutAsync(stayId, reason)` — register departure with reason
- [ ] `GetDailyListAsync(date)` — all maids currently in accommodation for a given date
- [ ] `GetCurrentOccupantsAsync` — currently checked-in maids
- [ ] `GetStayHistoryByWorkerAsync` — accommodation history for a worker
- [ ] Auto check-in: when arrival module confirms maid at accommodation, auto-create stay
- [ ] Publish events: `AccommodationCheckInEvent`, `AccommodationCheckOutEvent`

### 7.3 Integration

- [ ] When `Arrival` status → `AtAccommodation`, auto-create `AccommodationStay`
- [ ] When worker is deployed (placement → Placed), auto-checkout with reason `DeployedToCustomer`
- [ ] When runaway is reported, auto-checkout with reason `Runaway`
- [ ] When returnee is approved for return to country, auto-checkout with reason `ReturnedToCountry`

### 7.4 API

- [ ] `POST /api/accommodation/check-in` — manual check-in
- [ ] `POST /api/accommodation/{stayId}/check-out` — check out with reason
- [ ] `GET /api/accommodation/daily-list?date=` — daily occupant list
- [ ] `GET /api/accommodation/current` — current occupants
- [ ] `GET /api/accommodation/history?workerId=` — worker stay history
- [ ] Permissions: `accommodation.view`, `accommodation.manage`

### 7.5 Frontend

- [ ] Accommodation dashboard:
  - Current occupants list (real-time view)
  - Daily list view (select date)
  - Check-in / check-out actions
- [ ] Check-in form (select worker, room/location)
- [ ] Check-out dialog (select reason, notes)
- [ ] Accommodation history on worker detail page
- [ ] Daily report / printable list
- [ ] Sidebar navigation entry
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Accommodation staff can check in a maid with room/location assignment
2. Accommodation staff can check out a maid with a mandatory departure reason
3. Departure reasons include: Deployed to Customer, Runaway, Returned to Country, Transferred, Medical Reason, Other
4. A daily list shows all maids currently in accommodation for any given date
5. When a maid arrives at accommodation (from arrival module), the list auto-updates
6. When a maid is deployed, she is auto-checked out with reason "Deployed to Customer"
7. Room/location tracking is supported
8. Accommodation history is visible on the worker detail page
9. The daily list is printable / exportable
10. All check-in and check-out events are audit-logged
