# Task 06: Arrival Management

## Summary
A full arrival management workflow for maids arriving from outside the country. Includes driver assignment, photo uploads, pickup confirmation, and non-arrival alerts.

## Current State
- `Placement` has `ArrivedAt`, `ExpectedArrivalDate`, `FlightDetails` fields
- No driver assignment, no photo management, no arrival confirmation workflow
- No non-arrival notification logic

## Required Changes

### 6.1 Arrival Entity

- [ ] Create `Arrival` entity (SoftDeletableEntity):
  - `ArrivalCode` (auto-generated)
  - `WorkerId` (Guid)
  - `PlacementId` (Guid)
  - `SupplierId` (Guid, nullable)
  - `Status` (enum: Scheduled, InTransit, Arrived, PickedUp, AtAccommodation, NoShow, Cancelled)
  - `FlightNumber` (string, nullable)
  - `AirportCode` (string, nullable)
  - `AirportName` (string, nullable)
  - `ScheduledArrivalDate` (DateTime)
  - `ScheduledArrivalTime` (TimeOnly, nullable)
  - `ActualArrivalTime` (DateTime, nullable)
  - `PreTravelPhotoUrl` (string, nullable) — uploaded by supplier before travel
  - `ArrivalPhotoUrl` (string, nullable) — uploaded by driver at airport
  - `DriverId` (Guid, nullable) — assigned driver (user ID)
  - `DriverName` (string, nullable)
  - `DriverConfirmedPickupAt` (DateTime, nullable)
  - `DriverPickupPhotoUrl` (string, nullable) — photo of maid at airport
  - `AccommodationConfirmedAt` (DateTime, nullable)
  - `AccommodationConfirmedBy` (string, nullable)
  - `CustomerPickedUp` (bool, default false) — if customer picks up directly
  - `CustomerPickupConfirmedAt` (DateTime, nullable)
  - `Notes` (string, nullable)
- [ ] Create `ArrivalStatusHistory` entity
- [ ] EF configuration and migration

### 6.2 Arrival Service

- [ ] `ScheduleArrivalAsync` — create arrival record from placement flight details
- [ ] `AssignDriverAsync(arrivalId, driverId)` — assign driver to pickup
- [ ] `UploadPreTravelPhotoAsync` — supplier uploads maid photo before travel
- [ ] `ConfirmArrivalAsync` — mark maid as arrived
- [ ] `ConfirmPickupAsync` — driver confirms pickup + uploads airport photo
- [ ] `ConfirmAtAccommodationAsync` — accommodation staff confirms maid arrived at accommodation
- [ ] `ConfirmCustomerPickupAsync` — if customer picks up directly
- [ ] `ReportNoShowAsync` — maid did not arrive
- [ ] Non-arrival detection: scheduled job or check — if arrival time passes without confirmation, trigger notification
- [ ] Publish events: `ArrivalScheduledEvent`, `ArrivalConfirmedEvent`, `MaidNoShowEvent`, `MaidAtAccommodationEvent`

### 6.3 Non-Arrival Notifications

- [ ] If `ScheduledArrivalDate` passes and status is still `Scheduled` or `InTransit`:
  - Send notification to **Supplier**
  - Send notification to **Admin**
- [ ] Implementation: background job that checks for overdue arrivals (e.g., hourly)

### 6.4 Integration

- [ ] When a placement has `TicketArranged` status, auto-create an `Arrival` record
- [ ] When arrival is confirmed, update `Placement` status to `Arrived`
- [ ] When maid reaches accommodation, update `Placement` accordingly
- [ ] Link arrival to accommodation module (Task 07)

### 6.5 API

- [ ] `POST /api/arrivals` — schedule arrival
- [ ] `GET /api/arrivals` — list with filters (status, date range, driver)
- [ ] `GET /api/arrivals/{id}` — detail
- [ ] `PUT /api/arrivals/{id}/assign-driver`
- [ ] `PUT /api/arrivals/{id}/upload-pre-travel-photo`
- [ ] `PUT /api/arrivals/{id}/confirm-arrival`
- [ ] `PUT /api/arrivals/{id}/confirm-pickup` — driver action
- [ ] `PUT /api/arrivals/{id}/confirm-accommodation`
- [ ] `PUT /api/arrivals/{id}/confirm-customer-pickup`
- [ ] `PUT /api/arrivals/{id}/report-no-show`
- [ ] Permissions: `arrivals.view`, `arrivals.create`, `arrivals.manage`, `arrivals.driver_actions`

### 6.6 Frontend

- [ ] Arrivals list page (calendar/timeline view preferred, or table)
  - Filter by date, status, driver
  - Highlight overdue arrivals
- [ ] Arrival detail page:
  - Flight info, schedule, photos
  - Driver assignment
  - Status progression
  - Pre-travel and arrival photos side by side
- [ ] Driver view (simplified):
  - Assigned pickups list
  - Confirm pickup button + photo upload
- [ ] Arrival section on placement detail page
- [ ] Sidebar navigation entry
- [ ] i18n (en, ar)

## Acceptance Criteria

1. When a placement has a ticket arranged, an arrival record can be created with flight details
2. A driver can be assigned to an arrival
3. Supplier can upload a pre-travel photo of the maid
4. Driver can confirm pickup and upload a photo at the airport
5. Accommodation staff can confirm maid arrived at accommodation
6. If a customer picks up the maid directly, it can be registered and confirmed manually
7. If the maid does not arrive by the scheduled time, the system sends notifications to the supplier and admin
8. All arrival status changes are logged
9. Arrival photos (pre-travel, airport, pickup) are stored and viewable
10. Arrivals list shows overdue arrivals prominently
11. Arrival information is visible on the placement detail page
