# Phase 6: Scheduling & Dispatch

Manages flexible and temporary contract worker assignments. One worker can serve multiple clients. Enforces UAE labor law: 12h/day max, 8h consecutive rest, weekly rest day.

**Estimated Time:** 2 weeks (parallel with P7)

---

## P6-T01: Create Scheduling Entities, Booking Engine, Conflict Detection, Labor Law Enforcement

**Dependencies:** P3-T01

**Files:**
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Entities/Booking.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Entities/BookingRecurrence.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Entities/WorkerDailySchedule.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Entities/TransportAssignment.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Persistence/` (4 configs)
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Services/BookingService.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Services/ConflictDetectionService.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Services/LaborLawEnforcer.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/Services/UtilizationService.cs`
- `src/TadHub.Api/Controllers/Tadbeer/BookingsController.cs`
- `src/Modules/Tadbeer/Scheduling/Scheduling.Core/SchedulingServiceRegistration.cs`

**Instructions:**
1. `Booking` : TenantScopedEntity. ContractId FK, WorkerId FK, ClientId FK, SlotType (enum: FourHour, EightHour, Daily, Weekly, Monthly), StartTime (DateTimeOffset), EndTime, Status (enum: Scheduled, InProgress, Completed, Cancelled, NoShow), IsRecurring (bool), RecurrenceId FK, CancellationReason, TransportRequired (bool).
2. `BookingRecurrence` : TenantScopedEntity. BookingId FK (template), Pattern (enum: Daily, Weekly, BiWeekly, Monthly), DaysOfWeek (int flags), RepeatUntil, GeneratedCount.
3. `WorkerDailySchedule` : Materialized view or denormalized table. WorkerId, Date, TotalHoursBooked, SlotDetails (JSONB). Updated on every booking change. Used for fast conflict detection.
4. `ConflictDetectionService`: Hard block on double-booking (same worker, overlapping time). Soft warning when approaching 12h daily max. Reject bookings that violate 8h consecutive rest rule.
5. `LaborLawEnforcer`: ValidateBooking(WorkerId, StartTime, EndTime). Checks:
   - (a) total hours for the day <= 12
   - (b) at least 8 consecutive hours rest in any 24h window
   - (c) at least 1 full rest day per week
   Returns Result with specific violation details.
6. `BookingService`: CRUD + cancel + no-show recording. On create, publish BookingCreatedEvent (Financial generates invoice). On cancel, publish BookingCancelledEvent. On no-show, publish NoShowRecordedEvent.
7. `UtilizationService`: Billable hours vs available hours per worker. Idle worker identification.
8. API: GET `.../bookings?filter[workerId]=...&filter[status]=scheduled&filter[status]=inProgress&filter[startTime][gte]=2026-02-20&sort=startTime`. POST `.../bookings` with conflict detection. POST `.../bookings/{id}/cancel`. POST `.../bookings/{id}/no-show`.

**Tests:**
- [ ] Unit test: Double-booking same worker same time returns 409 Conflict
- [ ] Unit test: Booking that pushes worker past 12h/day returns 422 with labor law violation detail
- [ ] Unit test: 8h rest rule enforced: booking at 6 AM rejected if worker worked until 11 PM previous day
- [ ] Unit test: Recurring booking generates correct schedule
- [ ] Unit test: No-show publishes event and marks booking

**Acceptance:** Scheduling with labor law enforcement, conflict detection, and financial integration.

**Status:** ⏳ Pending
