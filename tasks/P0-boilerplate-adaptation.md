# Phase 0: Boilerplate Adaptation

Customize the existing boilerplate modules for the Tadbeer domain. Add agency-specific fields to Tenancy, seed domain roles and permissions into Authorization, and register the Tadbeer domain event contracts.

**Estimated Time:** 1 week

---

## P0-T01: Extend Tenant Entity for Tadbeer Agency Fields

**Dependencies:** Boilerplate complete

**Files:**
- `src/Modules/Tenancy/Tenancy.Core/Entities/Tenant.cs` (extend)
- `src/Modules/Tenancy/Tenancy.Core/Entities/TadbeerLicense.cs` (new)
- `src/Modules/Tenancy/Tenancy.Core/Entities/SharedPoolAgreement.cs` (new)
- `src/Modules/Tenancy/Tenancy.Core/Entities/SharedPoolWorker.cs` (new)
- `src/Modules/Tenancy/Tenancy.Core/Persistence/` (new configs)

**Instructions:**
1. Add to Tenant: `TadbeerLicenseNumber` (string, unique), `MohreLicenseNumber` (string), `TradeLicenseNumber` (string), `TradeLicenseExpiry` (DateTimeOffset), `Emirate` (enum: Dubai, AbuDhabi, Sharjah, Ajman, UAQ, RAK, Fujairah), `IsActive` (bool), `LicenseExpiryDate` (DateTimeOffset).
2. `TadbeerLicense`: TenantScopedEntity. LicenseType (Tadbeer/MoHRE/Trade), Number, IssuedAt, ExpiresAt, Status (Active/Expired/Suspended), DocumentUrl. One tenant has multiple licenses.
3. `SharedPoolAgreement`: BaseEntity. FromTenantId, ToTenantId, Status (Pending/Active/Revoked), ApprovedAt, RevenueSplitPercentage (decimal), AgreementDocumentUrl. Represents a bilateral worker-sharing agreement.
4. `SharedPoolWorker`: BaseEntity. SharedPoolAgreementId (FK), WorkerId (Guid), SharedAt, RevokedAt. Tracks which workers are in the pool.
5. License expiry Hangfire job: daily check, publish `LicenseExpiringEvent` at 30/14/7 days.
6. Migration: `dotnet ef migrations add TadbeerTenantExtensions`

**Tests:**
- [ ] Unit test: Tenant with Tadbeer license fields persists correctly
- [ ] Unit test: SharedPoolAgreement validates FromTenantId != ToTenantId
- [ ] Integration test: Create agreement, add worker to pool, query from partner tenant returns worker

**Acceptance:** Agencies have Tadbeer-specific registration fields. Shared pool agreements are bilateral and auditable.

**Status:** ⏳ Pending

---

## P0-T02: Seed Tadbeer Domain Roles and Permissions

**Dependencies:** Boilerplate complete

**Files:**
- `src/Modules/Authorization/Authorization.Core/Seeds/TadbeerPermissionSeeder.cs`
- `src/Modules/Authorization/Authorization.Core/Seeds/TadbeerRoleSeeder.cs`

**Instructions:**
1. New permissions (prefix by module): `clients.register`, `clients.verify`, `clients.manage`, `workers.manage`, `workers.cv.edit`, `workers.search`, `workers.passport.custody`, `contracts.create`, `contracts.approve`, `contracts.terminate`, `contracts.refund`, `financial.payments.process`, `financial.invoices.generate`, `financial.xreport.generate`, `financial.refunds.process`, `pro.tasks.manage`, `pro.visa.apply`, `pro.documents.manage`, `scheduling.bookings.create`, `scheduling.bookings.cancel`, `wps.payroll.manage`, `wps.sif.submit`, `reports.view`, `reports.export`, `reports.mohre`.
2. Domain roles seeded on TenantCreated:
   - `agency-admin` (all permissions)
   - `receptionist` (clients.register, clients.manage, workers.search, contracts.create)
   - `cashier` (financial.payments.process, financial.xreport.generate, financial.invoices.generate)
   - `pro-officer` (pro.*, workers.passport.custody, contracts.approve)
   - `agent` (workers.manage, workers.cv.edit, workers.search, scheduling.*)
   - `accountant` (financial.*, wps.*, reports.*)
   - `viewer` (reports.view)
3. Run as part of TenantCreatedConsumer (extend existing consumer to also seed domain roles).

**Tests:**
- [ ] Unit test: Seeder creates all 7 domain roles with correct permissions
- [ ] Integration test: Create tenant, verify 7 domain roles seeded alongside the 3 platform roles

**Acceptance:** Every new agency gets domain roles pre-configured. Receptionist sees only client/worker search; cashier sees only payments.

**Status:** ⏳ Pending

---

## P0-T03: Register All Tadbeer Domain Events in SharedKernel

**Dependencies:** Boilerplate complete

**Files:**
- `src/TadHub.SharedKernel/Events/Tadbeer/` (32 event record files)

**Instructions:**
1. Create all 32 domain event records from the registry. Each is a C# record implementing `IDomainEvent`.
2. Every event includes: `EventId` (Guid), `OccurredAt` (DateTimeOffset), `TenantId` (Guid), `CorrelationId` (Guid).
3. Entity-specific properties: `WorkerStatusChangedEvent` includes WorkerId, OldStatus, NewStatus, Reason. `ContractCreatedEvent` includes ContractId, ContractType, ClientId, WorkerId. `PaymentReceivedEvent` includes PaymentId, InvoiceId, Amount, Method.
4. Group files in subfolders:
   - `Events/Tadbeer/Client/`
   - `Events/Tadbeer/Worker/`
   - `Events/Tadbeer/Contract/`
   - `Events/Tadbeer/Financial/`
   - `Events/Tadbeer/Pro/`
   - `Events/Tadbeer/Scheduling/`
   - `Events/Tadbeer/Wps/`

**Domain Event Registry:**
| Event | Published By | Consumed By |
|-------|-------------|-------------|
| ClientRegisteredEvent | Client Mgmt | Notifications |
| ClientVerifiedEvent | Client Mgmt | Contract Engine, Notifications |
| ClientBlockedEvent | Client Mgmt | Contract Engine, Financial |
| WorkerStatusChangedEvent | Worker/CV | Contract Engine, Scheduling, WPS, Notifications, Reporting |
| WorkerAbscondedEvent | Worker/CV | Contract Engine, Financial, PRO, WPS, Notifications |
| WorkerMedicalResultEvent | Worker/CV | Contract Engine, PRO, Notifications |
| WorkerBookedEvent | Worker/CV | Contract Engine, Scheduling, Notifications |
| ContractCreatedEvent | Contract Engine | Worker/CV, Financial, PRO, Scheduling, Notifications |
| ContractActivatedEvent | Contract Engine | Worker/CV, Financial, PRO, Scheduling, WPS, Notifications |
| ContractTerminatedEvent | Contract Engine | Worker/CV, Financial, PRO, WPS, Notifications |
| ContractRenewedEvent | Contract Engine | Worker/CV, Financial, PRO, Notifications |
| GuaranteePeriodExpiringEvent | Contract Engine | Notifications |
| ReplacementRequestedEvent | Contract Engine | Worker/CV, Financial, Notifications |
| RefundTriggeredEvent | Contract Engine | Financial, Notifications |
| SponsorshipTransferEvent | Contract Engine | PRO, Financial, Worker/CV, Notifications |
| InvoiceGeneratedEvent | Financial | Notifications |
| PaymentReceivedEvent | Financial | Contract Engine, Notifications |
| PaymentOverdueEvent | Financial | Notifications, Reporting |
| RefundProcessedEvent | Financial | Contract Engine, Notifications |
| CreditNoteIssuedEvent | Financial | Notifications |
| VisaIssuedEvent | PRO Gateway | Worker/CV, Contract Engine, Notifications |
| VisaExpiringEvent | PRO Gateway | Notifications, Reporting |
| MedicalTestCompletedEvent | PRO Gateway | Worker/CV, Contract Engine, Notifications |
| EmiratesIdIssuedEvent | PRO Gateway | Worker/CV, Notifications |
| InsuranceActivatedEvent | PRO Gateway | Worker/CV, Contract Engine, Notifications |
| DocumentExpiringEvent | PRO Gateway | Notifications, Reporting |
| BookingCreatedEvent | Scheduling | Financial, Worker/CV, Notifications |
| BookingCancelledEvent | Scheduling | Financial, Worker/CV, Notifications |
| NoShowRecordedEvent | Scheduling | Financial, Notifications |
| SifSubmittedEvent | WPS | Financial, Notifications |
| WpsComplianceAlertEvent | WPS | Notifications, Reporting |
| SalaryPaidEvent | WPS | Financial, Notifications |

**Tests:**
- [ ] Unit test: Every event serializes/deserializes to JSON without data loss
- [ ] Unit test: CorrelationId is propagated correctly across related events

**Acceptance:** All domain events are defined. Modules can publish and consume them.

**Status:** ⏳ Pending

---

## P0-T04: Create Localization Infrastructure for Bilingual Support

**Dependencies:** Boilerplate complete

**Files:**
- `src/TadHub.SharedKernel/Localization/LocalizedString.cs`
- `src/TadHub.SharedKernel/Localization/ILocalizationService.cs`
- `src/TadHub.Infrastructure/Localization/LocalizationService.cs`

**Instructions:**
1. `LocalizedString`: Value type with `En` (string) and `Ar` (string) properties. Stored as JSONB in PostgreSQL. Used for any user-facing text that needs bilingual support (notification templates, contract terms, report headers).
2. EF Core value converter: `LocalizedStringConverter` that serializes to/from JSONB.
3. `ILocalizationService`: `Resolve(LocalizedString, string locale)` returns the correct language string. Default locale from tenant config or user preference.
4. Entities with localized fields: `NotificationTemplate.Title`, `NotificationTemplate.Body`, `WorkerJobCategory.Name`, `ContractTerms.Description`.

**Tests:**
- [ ] Unit test: LocalizedString serializes to `{ "en": "...", "ar": "..." }` JSONB
- [ ] Unit test: Resolve with 'ar' returns Arabic string

**Acceptance:** Bilingual support is a first-class type, not a retrofit.

**Status:** ⏳ Pending
