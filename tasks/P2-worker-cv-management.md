# Phase 2: Worker / CV Management

The inventory engine. Every domestic worker is a managed service item with a 20+ state finite state machine, digital CV, 19 MoHRE job categories, passport custody tracking, and nationality-based pricing. This is the most entity-rich and logic-heavy domain module.

**Estimated Time:** 3 weeks (parallel with P1)

---

## P2-T01: Create Worker.Contracts

**Dependencies:** P0-T03

**Files:**
- `src/Modules/Tadbeer/Worker/Worker.Contracts/IWorkerService.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/IWorkerSearchService.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/DTOs/WorkerDto.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/DTOs/WorkerCvDto.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/DTOs/CreateWorkerRequest.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/DTOs/WorkerStateTransitionRequest.cs`
- `src/Modules/Tadbeer/Worker/Worker.Contracts/DTOs/WorkerSearchCriteria.cs`

**Instructions:**
1. `IWorkerService`: CreateAsync, GetByIdAsync, UpdateCvAsync, TransitionStateAsync(Guid workerId, WorkerStateTransitionRequest), GetPassportCustodyAsync, TransferPassportAsync, ListAsync(QueryParameters).
2. `IWorkerSearchService`: SearchAsync(WorkerSearchCriteria) returns ranked results. Criteria: nationality, jobCategories (array), skills, languages, religion, ageRange, priceRange, availability, status.
3. `WorkerDto`: Full CV data + CurrentStatus + PassportLocation + AvailabilityStatus.
4. `WorkerStateTransitionRequest`: TargetState (enum), Reason (string), RelatedEntityId (Guid?, e.g., contractId).

**Tests:**
- [ ] Contract test: All interfaces and 20+ DTOs compile

**Acceptance:** Worker contracts are referenceable by Contract Engine, PRO, Scheduling.

**Status:** ⏳ Pending

---

## P2-T02: Create Worker Entities (8 Entities) and State Machine Enum

**Dependencies:** P2-T01

**Files:**
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/Worker.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/WorkerSkill.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/WorkerLanguage.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/WorkerMedia.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/WorkerPassportCustody.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/WorkerStateHistory.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/JobCategory.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Entities/NationalityPricing.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Persistence/` (8 configs)

**Instructions:**
1. `Worker` : TenantScopedEntity, IAuditable. PassportNumber (unique per tenant), EmiratesId, CvSerial, FullNameEn, FullNameAr, Nationality, Religion, MaritalStatus, DateOfBirth, Gender, Education, Photo, VideoUrl, JobCategoryId FK, CurrentStatus (WorkerStatus enum), PassportLocation (enum: WithAgency, WithSponsor, WithImmigration, Surrendered), MonthlyBaseSalary (decimal), IsAvailableForFlexible (bool), Notes.
2. `WorkerStatus` enum (20 states): NewArrival, InTraining, ReadyForMarket, Booked, Hired, OnProbation, Active, Renewed, Transferred, UnderMedicalTest, AwaitingVisa, InProbationReview, PendingReplacement, Absconded, Deported, Pregnant, MedicallyUnfit, Terminated, Repatriated, Deceased.
3. `WorkerSkill` : TenantScopedEntity. WorkerId FK, SkillName (Cooking, Cleaning, Childcare, Eldercare, Laundry, Ironing, Driving), Rating (int 0-100). Composite index (WorkerId, SkillName).
4. `WorkerLanguage` : TenantScopedEntity. WorkerId FK, Language (string), Proficiency (enum: Poor, Fair, Fluent).
5. `WorkerMedia` : TenantScopedEntity. WorkerId FK, MediaType (Photo/Video), FileUrl, IsPrimary, UploadedAt.
6. `WorkerPassportCustody` : TenantScopedEntity. WorkerId FK, Location (enum), HandedToName, HandedToEntityId (Guid?), HandedAt, ReceivedAt, Notes. Append-only audit trail.
7. `WorkerStateHistory` : TenantScopedEntity. WorkerId FK, FromStatus, ToStatus, Reason, TriggeredByUserId, RelatedEntityId, OccurredAt. Append-only.
8. `JobCategory` : BaseEntity (global). Name (LocalizedString), MoHRECode (string), IsActive. Seed all 19 categories.
9. `NationalityPricing` : TenantScopedEntity. Nationality (string), ContractType (enum: Traditional, Temporary, Flexible), Amount (decimal), Currency ('AED'), EffectiveFrom (DateTimeOffset), EffectiveTo (DateTimeOffset?). Supports time-ranged pricing per MoHRE 6-month revision cycle.
10. Migration: `dotnet ef migrations add InitWorkerManagement`

**Tests:**
- [ ] Integration test: Create worker with skills and languages, query back
- [ ] Integration test: NationalityPricing with overlapping date ranges resolves to most recent
- [ ] Integration test: 19 job categories seeded

**Acceptance:** Worker data model is complete with all CV fields, state machine, and pricing.

**Status:** ⏳ Pending

---

## P2-T03: Implement Worker State Machine with Domain Event Publishing

**Dependencies:** P2-T02

**Files:**
- `src/Modules/Tadbeer/Worker/Worker.Core/StateMachine/WorkerStateMachine.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/StateMachine/StateTransitionValidator.cs`

**Instructions:**
1. `WorkerStateMachine`: Static class with a `Dictionary<(WorkerStatus from, WorkerStatus to), TransitionRule>`. Each rule defines: IsAllowed (bool), RequiredConditions (list of checks), EventToPublish (event type).
2. Valid transitions (subset): NewArrival > InTraining, InTraining > ReadyForMarket, ReadyForMarket > Booked, Booked > Hired, Hired > OnProbation, OnProbation > Active, Active > Renewed/Terminated/Absconded, ReadyForMarket > UnderMedicalTest, UnderMedicalTest > ReadyForMarket/MedicallyUnfit, any > Absconded/Deceased (emergency transitions).
3. Invalid transitions throw `InvalidStateTransitionException` (maps to 409 Conflict via GlobalExceptionHandler).
4. `StateTransitionValidator`: Before transition, check preconditions. E.g., Booked > Hired requires valid medical, valid visa, active insurance, verified client. Returns `Result<bool>` with specific failure reason.
5. On successful transition: update `Worker.CurrentStatus`, append `WorkerStateHistory` record, publish `WorkerStatusChangedEvent`. If Absconded, also publish `WorkerAbscondedEvent`.

**Tests:**
- [ ] Unit test: Valid transition ReadyForMarket > Booked succeeds
- [ ] Unit test: Invalid transition NewArrival > Active throws InvalidStateTransitionException
- [ ] Unit test: Transition Booked > Hired with missing medical returns Failure('Medical clearance required')
- [ ] Unit test: Absconded transition from any state publishes WorkerAbscondedEvent
- [ ] Unit test: State history is appended (not overwritten) on every transition

**Acceptance:** State machine is bulletproof. Invalid transitions are impossible. Every transition is audited and triggers domain events.

**Status:** ⏳ Pending

---

## P2-T04: Implement WorkerService and WorkerSearchService

**Dependencies:** P2-T03

**Files:**
- `src/Modules/Tadbeer/Worker/Worker.Core/Services/WorkerService.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Services/WorkerSearchService.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/Services/NationalityPricingService.cs`

**Instructions:**
1. `WorkerService.ListAsync`: `filter[status]=readyForMarket&filter[status]=inTraining` (array), `filter[nationality]=philippines&filter[nationality]=india`, `filter[jobCategoryId]=...`, `filter[passportLocation]=withAgency`, `sort=-createdAt`. Includes shared pool workers if agreements are active.
2. `WorkerService.TransitionStateAsync`: Delegate to WorkerStateMachine. Return `Result<WorkerDto>`.
3. `WorkerSearchService`: Weighted scoring search. Nationality match (20%), skill ratings (30%), language proficiency (20%), experience (15%), availability (15%). Returns ranked list. Uses Meilisearch for text search + DB for scoring.
4. `NationalityPricingService`: GetPriceAsync(string nationality, ContractType type, DateTimeOffset asOf). Finds the NationalityPricing record where EffectiveFrom <= asOf and (EffectiveTo is null or > asOf). If no pricing found, return Result.Failure.
5. Shared pool query: If tenant has active SharedPoolAgreements, union own workers with shared pool workers for search results. Shared pool workers are marked with a SharedFromTenantId in the DTO.

**Tests:**
- [ ] Unit test: Search ranks Philippines maid with 90% cooking higher than 60% cooking
- [ ] Unit test: NationalityPricingService returns correct price for time range
- [ ] Unit test: ListAsync with shared pool active returns workers from partner tenant
- [ ] Unit test: `filter[status]=readyForMarket&filter[status]=booked` returns both statuses

**Acceptance:** Worker CRUD, state transitions, search/matchmaking, and nationality pricing work.

**Status:** ⏳ Pending

---

## P2-T05: Create Worker API Controllers and ServiceRegistration

**Dependencies:** P2-T04

**Files:**
- `src/TadHub.Api/Controllers/Tadbeer/WorkersController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/WorkerSearchController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/NationalityPricingController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/JobCategoriesController.cs`
- `src/Modules/Tadbeer/Worker/Worker.Core/WorkerServiceRegistration.cs`

**Instructions:**
1. `WorkersController`: CRUD + POST `.../workers/{id}/transition` (state change), GET `.../workers/{id}/history` (state history), GET `.../workers/{id}/passport-custody`, POST `.../workers/{id}/passport-transfer`.
2. `WorkerSearchController`: POST `/api/v1/tadbeer/workers/search` with body criteria (complex search). GET `/api/v1/tadbeer/workers?filter[...]` for simple list filtering.
3. `NationalityPricingController`: CRUD for pricing rules. GET `.../nationality-pricing?filter[nationality]=philippines&filter[contractType]=traditional`.
4. `JobCategoriesController`: GET (list all 19, with localized names). Admin-only POST/PUT.
5. Permission checks: `workers.manage` for CRUD, `workers.search` for search, `workers.passport.custody` for passport operations.

**Tests:**
- [ ] Integration test: Full lifecycle: create worker > train > ready > search finds them > book > hire (with precondition checks)
- [ ] Integration test: Passport custody transfer creates audit trail
- [ ] Integration test: Agent role can search but cannot transition to Terminated

**Acceptance:** Worker module complete. 20-state machine enforced via API.

**Status:** ⏳ Pending
