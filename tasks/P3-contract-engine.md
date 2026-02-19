# Phase 3: Contract Engine

The heart of the ERP. Three contract types (Traditional, Temporary, Flexible) with distinct lifecycles, guarantee periods, replacement logic, refund triggers, and sponsorship transfers. Uses the Strategy pattern to keep type-specific logic isolated.

**Estimated Time:** 3 weeks

---

## P3-T01: Create Contract.Contracts and Entities

**Dependencies:** P0-T03, P1-T01, P2-T01

**Files:**
- `src/Modules/Tadbeer/Contract/Contract.Contracts/IContractService.cs`
- `src/Modules/Tadbeer/Contract/Contract.Contracts/DTOs/...`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/Contract.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractStateHistory.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractGuaranteePeriod.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractReplacement.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractRenewal.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractTermination.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Entities/ContractMultiWorker.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Persistence/` (7 configs)

**Instructions:**
1. `Contract` : TenantScopedEntity, IAuditable. ContractNumber (string, unique per tenant, auto-generated), ContractType (enum: Traditional, Temporary, Flexible), ClientId FK, WorkerId FK, Status (enum: Draft, PendingApproval, Active, OnProbation, Renewed, Terminated, Cancelled, Expired), SponsorshipType (enum: EmployerFamily, AgencyCompany), StartDate, EndDate, TotalAmount (decimal), VatAmount (decimal), Currency ('AED'), GuaranteePeriodDays (int, default 180 for Traditional), ActivatedAt, TerminatedAt, TerminationReason.
2. `ContractGuaranteePeriod` : TenantScopedEntity. ContractId FK, StartsAt, ExpiresAt, Status (Active/Expired/ClaimFiled), ClaimReason, ClaimFiledAt. Hangfire job checks daily and publishes GuaranteePeriodExpiringEvent at 30/14/7 days.
3. `ContractReplacement` : TenantScopedEntity. OriginalContractId FK, OriginalWorkerId, ReplacementWorkerId, Reason (enum: Incompetence, Absconding, MedicalUnfitness, AgencyNonCompliance), RequestedAt, CompletedAt.
4. `ContractRenewal` : TenantScopedEntity. OriginalContractId FK, NewContractId FK, RenewedAt, NewEndDate, PriceAdjustment.
5. `ContractTermination` : TenantScopedEntity. ContractId FK, Reason (enum), InitiatedBy (Client/Agency/System), TerminatedAt, RefundEligible (bool), RefundAmount, SettlementNotes.
6. `ContractMultiWorker` : TenantScopedEntity. ContractId FK, WorkerId FK, Role (string), Notes. For VIP multi-worker contracts.
7. `ContractStateHistory`: Same pattern as WorkerStateHistory.
8. Migration: `dotnet ef migrations add InitContractEngine`

**Tests:**
- [ ] Integration test: Create traditional contract, verify 180-day guarantee auto-created
- [ ] Integration test: Contract number auto-generates in sequential format

**Acceptance:** Contract data model supports all 3 types with guarantee, replacement, renewal, and termination tracking.

**Status:** ⏳ Pending

---

## P3-T02: Implement Contract Lifecycle Strategies (Traditional, Temporary, Flexible)

**Dependencies:** P3-T01

**Files:**
- `src/Modules/Tadbeer/Contract/Contract.Core/Strategies/IContractLifecycleStrategy.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Strategies/TraditionalContractStrategy.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Strategies/TemporaryContractStrategy.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Strategies/FlexibleContractStrategy.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Strategies/ContractStrategyFactory.cs`

**Instructions:**
1. `IContractLifecycleStrategy`: ValidateCreation(Contract), CalculatePrice(Contract), Activate(Contract), CanRenew(Contract), Renew(Contract), CanTerminate(Contract), Terminate(Contract, TerminationReason), GetGuaranteePeriodDays(). Each method returns `Result<T>`.
2. `TraditionalContractStrategy`: 2-year duration. Sponsorship = EmployerFamily. GuaranteePeriod = 180 days. Price from NationalityPricingService. Activation requires: worker medical cleared, visa valid, insurance active, client verified. Replacement allowed during guarantee. Full refund if agency at fault.
3. `TemporaryContractStrategy`: 6-month initial. Sponsorship = AgencyCompany. After 6 months, can transfer sponsorship to employer (triggers SponsorshipTransferEvent). Prorated fee on transfer. No guarantee period (agency retains sponsorship control).
4. `FlexibleContractStrategy`: Variable duration (4h to 12mo). Sponsorship = AgencyCompany. Per-unit pricing. No guarantee period. Linked to Scheduling module for booking management. Max 12h/day, 8h rest enforced (delegates to Scheduling for validation).
5. `ContractStrategyFactory`: Resolves strategy from ContractType enum. Registered in DI.

**Tests:**
- [ ] Unit test: Traditional activation fails without valid medical (returns Failure with reason)
- [ ] Unit test: Temporary prorated fee calculated correctly at month 4 of 6
- [ ] Unit test: Flexible price lookup returns correct rate for 4h/8h/monthly duration
- [ ] Unit test: Factory returns correct strategy for each ContractType

**Acceptance:** Three distinct lifecycle strategies. Shared interface, isolated logic.

**Status:** ⏳ Pending

---

## P3-T03: Implement ContractService Orchestrating Strategies and Events

**Dependencies:** P3-T02

**Files:**
- `src/Modules/Tadbeer/Contract/Contract.Core/Services/ContractService.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Services/GuaranteePeriodService.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Services/ReplacementService.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Services/RefundTriggerService.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Jobs/GuaranteePeriodCheckJob.cs`

**Instructions:**
1. `ContractService.CreateAsync`: Validate via strategy.ValidateCreation, calculate price, create Contract in Draft status, publish ContractCreatedEvent. Worker transitions to Booked state via event consumer.
2. `ContractService.ApproveAsync`: Strategy validates all preconditions (medical, visa, insurance, client verification). Hard blocks, not soft warnings. On success, transition to Active, publish ContractActivatedEvent. Worker transitions to Hired.
3. `ContractService.TerminateAsync`: Delegate to strategy. Create ContractTermination. If refund eligible, publish RefundTriggeredEvent (Financial module picks up). Worker transitions to Terminated. Publish ContractTerminatedEvent.
4. `GuaranteePeriodService`: Create guarantee record on traditional contract activation. Hangfire recurring job (daily at 2 AM) checks for expiring periods. Publishes alerts at 30/14/7 days.
5. `ReplacementService`: Link new worker to existing contract. Original worker > PendingReplacement state. New worker inherits remaining guarantee days. Original financial record carries over.
6. `RefundTriggerService`: On refund trigger, start 14-day countdown. Categorize by reason code. Generate credit note data (Financial module handles actual money). Publish RefundTriggeredEvent.
7. **Compliance validation:** Contract CANNOT activate unless ALL of: worker.MedicalStatus == Cleared, worker has valid visa, worker has active insurance, client.IsVerified == true. These are hard blocks returning 409 Conflict with specific missing requirements.
8. `ListAsync`: `filter[contractType]=traditional&filter[contractType]=temporary`, `filter[status]=active&filter[status]=onProbation`, `filter[clientId]=...`, `filter[workerId]=...`, `filter[startDate][gte]=...`, `sort=-createdAt`.

**Tests:**
- [ ] Unit test: CreateAsync transitions worker to Booked via domain event
- [ ] Unit test: ApproveAsync with missing medical returns 409 with 'Medical clearance required'
- [ ] Unit test: Replacement preserves remaining guarantee days
- [ ] Unit test: Refund trigger starts 14-day countdown and publishes event
- [ ] Unit test: `filter[contractType]=traditional&filter[status]=active` returns correct results

**Acceptance:** Contract lifecycle is fully managed. Compliance is enforced. Events cascade correctly.

**Status:** ⏳ Pending

---

## P3-T04: Create Contract API Controllers, Consumers, ServiceRegistration

**Dependencies:** P3-T03

**Files:**
- `src/TadHub.Api/Controllers/Tadbeer/ContractsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/ContractReplacementsController.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/WorkerAbscondedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/ClientVerifiedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/PaymentReceivedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/VisaIssuedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/MedicalTestCompletedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/Consumers/InsuranceActivatedConsumer.cs`
- `src/Modules/Tadbeer/Contract/Contract.Core/ContractServiceRegistration.cs`

**Instructions:**
1. `ContractsController`: POST `.../contracts`, GET `.../contracts` (filtered), GET `.../contracts/{id}?include=client,worker,guarantee`, POST `.../contracts/{id}/approve`, POST `.../contracts/{id}/terminate`, POST `.../contracts/{id}/renew`.
2. `ContractReplacementsController`: POST `.../contracts/{id}/replacements`, GET `.../contracts/{id}/replacements`.
3. Consumers: WorkerAbscondedConsumer auto-terminates active contracts for that worker. PaymentReceivedConsumer can auto-approve contracts waiting on payment. VisaIssuedConsumer + MedicalTestCompletedConsumer + InsuranceActivatedConsumer check if all preconditions are now met, and if so, notify that the contract is ready for activation.
4. Permission checks: `contracts.create` (receptionist), `contracts.approve` (PRO officer), `contracts.terminate` (agency admin), `contracts.refund` (agency admin).

**Tests:**
- [ ] Integration test: Full traditional contract lifecycle: create > pay > approve (with all preconditions met) > guarantee period > terminate > refund
- [ ] Integration test: Worker absconding auto-terminates active contract and triggers refund
- [ ] Integration test: Flexible contract links to scheduling for booking validation

**Acceptance:** Contract Engine complete. Events cascade to Financial, PRO, Worker, Scheduling.

**Status:** ⏳ Pending
