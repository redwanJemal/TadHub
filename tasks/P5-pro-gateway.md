# Phase 5: PRO & Government Gateway

Manages all government-facing processes: 8 transaction types (entry permit, change of status, Emirates ID, medical, insurance, visa stamping, renewal, cancellation). Each is a mini state machine. Designed with an abstraction layer so v1 uses manual status updates and v2 can plug in MoHRE API integration.

**Estimated Time:** 3 weeks (parallel with P4)

---

## P5-T01: Create PRO Entities and Transaction Types

**Dependencies:** P3-T01

**Files:**
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Entities/GovernmentTransaction.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Entities/SponsorFile.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Entities/ProTask.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Entities/DocumentTracking.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Persistence/` (4 configs)

**Instructions:**
1. `GovernmentTransaction` : TenantScopedEntity, IAuditable. TransactionType (enum: EntryPermit, ChangeOfStatus, EmiratesId, MedicalFitness, HealthInsurance, VisaStamping, VisaRenewal, VisaCancellation), WorkerId FK, ContractId FK (nullable), ClientId FK (nullable), Status (enum: NotStarted, InProgress, Submitted, Approved, Rejected, Completed, Expired), SubmittedAt, CompletedAt, RejectionReason, GovernmentReferenceNumber, Fee (decimal), ValidFrom, ValidUntil, DocumentUrl, Notes.
2. `SponsorFile` : TenantScopedEntity. ClientId FK, Emirate (enum), FileNumber, Status (enum: Open, Pending, Active, Blocked), OpenedAt, Notes. A client can have multiple sponsor files (multi-emirate VIP).
3. `ProTask` : TenantScopedEntity. GovernmentTransactionId FK, AssignedToUserId, Priority (enum: Low, Medium, High, Urgent), DueDate, CompletedAt, Notes. The PRO officer's work queue.
4. `DocumentTracking` : TenantScopedEntity. WorkerId FK, DocumentType (enum: Passport, EmiratesId, MedicalCertificate, InsurancePolicy, LaborContract, ResidenceVisa), DocumentNumber, IssuedAt, ExpiresAt, Status (Active/Expired/Pending), FileUrl. Centralized across the module.
5. Migration: `dotnet ef migrations add InitProGateway`

**Tests:**
- [ ] Integration test: Create government transaction, track through status changes

**Acceptance:** PRO data model supports all 8 transaction types.

**Status:** ⏳ Pending

---

## P5-T02: Implement ProGatewayService with IGovernmentTransactionService Abstraction

**Dependencies:** P5-T01

**Files:**
- `src/Modules/Tadbeer/ProGateway/ProGateway.Contracts/IProGatewayService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Contracts/IGovernmentTransactionService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Services/ProGatewayService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Services/ManualGovernmentTransactionService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Services/ProTaskService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Services/DocumentTrackingService.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Consumers/ContractActivatedConsumer.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Consumers/ContractTerminatedConsumer.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Consumers/SponsorshipTransferConsumer.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Jobs/DocumentExpiryCheckJob.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/Jobs/VisaExpiryCheckJob.cs`

**Instructions:**
1. `IGovernmentTransactionService`: SubmitAsync(GovernmentTransaction), CheckStatusAsync(Guid transactionId), CancelAsync(Guid). V1: ManualGovernmentTransactionService (PRO officer manually updates status). V2: MoHREApiTransactionService (direct API calls).
2. `ProGatewayService`: Orchestrates the full flow. On ContractActivated: auto-create transactions for all required steps (medical, visa, insurance, Emirates ID) in NotStarted status. Create ProTasks for each assigned to the available PRO officer.
3. `ProTaskService`: The PRO officer's queue. ListAsync with `filter[priority]=urgent&filter[priority]=high`, `filter[status]=notStarted&filter[status]=inProgress`, `filter[transactionType]=visaStamping&filter[transactionType]=medicalFitness`, `sort=dueDate`.
4. `DocumentTrackingService`: Centralized document status. When a transaction completes (visa issued, Emirates ID obtained, medical cleared), update the worker's document records. Publish domain events (VisaIssuedEvent, MedicalTestCompletedEvent, etc.).
5. `DocumentExpiryCheckJob`: Daily Hangfire job. Check all active documents for expiry within 30/14/7 days. Publish DocumentExpiringEvent.
6. ContractActivatedConsumer: Creates the chain of government transactions. ContractTerminatedConsumer: Creates visa cancellation transaction.

**Tests:**
- [ ] Unit test: ContractActivated auto-creates medical + visa + insurance + EmiratesID transactions
- [ ] Unit test: PRO task queue sorted by priority then due date
- [ ] Unit test: Document expiry job publishes alerts at correct intervals
- [ ] Unit test: Manual status update to Completed publishes VisaIssuedEvent

**Acceptance:** PRO module manages all government processes. V1 manual, V2-ready for API integration.

**Status:** ⏳ Pending

---

## P5-T03: Create PRO API Controllers and ServiceRegistration

**Dependencies:** P5-T02

**Files:**
- `src/TadHub.Api/Controllers/Tadbeer/ProTasksController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/GovernmentTransactionsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/SponsorFilesController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/DocumentTrackingController.cs`
- `src/Modules/Tadbeer/ProGateway/ProGateway.Core/ProGatewayServiceRegistration.cs`

**Instructions:**
1. `ProTasksController`: GET `.../pro/tasks?filter[priority]=urgent&filter[priority]=high&filter[transactionType]=visaStamping&filter[status]=inProgress&sort=dueDate` (the PRO officer's main screen).
2. `GovernmentTransactionsController`: Full CRUD + POST `.../transactions/{id}/submit` + POST `.../transactions/{id}/complete`.
3. `SponsorFilesController`: CRUD per client. `filter[status]=active&filter[emirate]=dubai`.
4. `DocumentTrackingController`: GET `.../documents?filter[status]=expired&filter[status]=pending&filter[expiresAt][lte]=2026-03-01` (the document status dashboard).

**Tests:**
- [ ] Integration test: Contract activation auto-populates PRO task queue
- [ ] Integration test: Complete medical transaction > document tracking updated > worker state machine checks pass

**Acceptance:** PRO module complete.

**Status:** ⏳ Pending
