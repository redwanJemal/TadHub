# Phase 1: Client Management

Manages the full employer lifecycle: registration, document verification, tiered categorization (Local/Expat/Investor/VIP), lead tracking, and the client document vault. Clients are the demand side of every contract.

**Estimated Time:** 3 weeks (parallel with P2)

---

## P1-T01: Create ClientManagement.Contracts

**Dependencies:** P0-T03

**Files:**
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Contracts/IClientService.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Contracts/DTOs/ClientDto.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Contracts/DTOs/CreateClientRequest.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Contracts/DTOs/ClientDocumentDto.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Contracts/DTOs/LeadDto.cs`

**Instructions:**
1. `IClientService`: RegisterAsync(CreateClientRequest), GetByIdAsync(Guid), UpdateAsync(Guid, UpdateClientRequest), VerifyAsync(Guid), BlockAsync(Guid, string reason), ListAsync(QueryParameters), GetDocumentsAsync(Guid clientId), AddDocumentAsync(Guid clientId, AddDocumentRequest), GetLeadsAsync(QueryParameters).
2. `ClientDto`: Id, EmiratesId, FullNameEn, FullNameAr, PassportNumber, Nationality, Category (Local/Expat/Investor/VIP), Phone, Email, SponsorFileStatus (Open/Pending/Active/Blocked), IsVerified, CreatedAt.
3. `CreateClientRequest`: EmiratesId, FullNameEn, FullNameAr, PassportNumber, Nationality, Phone, Email, CategoryOverride (optional, auto-detected from EmiratesId), SalaryCertificateUrl, EjariUrl.
4. Filterable fields: category, sponsorFileStatus, isVerified, nationality, emirate, createdAt.

**Tests:**
- [ ] Contract test: Interface compiles

**Acceptance:** Other modules can reference IClientService.

**Status:** ✅ Complete

---

## P1-T02: Create Client Entities and EF Configuration

**Dependencies:** P1-T01

**Files:**
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Entities/Client.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Entities/ClientDocument.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Entities/ClientCommunicationLog.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Entities/Lead.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Entities/DiscountCard.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Persistence/` (5 config files)

**Instructions:**
1. `Client` : TenantScopedEntity, IAuditable. EmiratesId (string, indexed), FullNameEn, FullNameAr, PassportNumber, Nationality (string), Phone, Email, Category (enum: Local, Expat, Investor, VIP), SponsorFileStatus (enum: Open, Pending, Active, Blocked), Emirate, IsVerified, VerifiedAt, VerifiedByUserId, BlockedReason, Notes.
2. `ClientDocument` : TenantScopedEntity. ClientId FK, DocumentType (enum: EmiratesId, Passport, SalaryCertificate, EjariContract, TenancyContract, Other), FileUrl, FileName, ExpiresAt, IsVerified, UploadedAt. Index on (ClientId, DocumentType).
3. `ClientCommunicationLog` : TenantScopedEntity. ClientId FK, Channel (enum: Phone, WhatsApp, Email, WalkIn), Direction (Inbound/Outbound), Summary, LoggedByUserId, OccurredAt.
4. `Lead` : TenantScopedEntity, IAuditable. ClientId FK (nullable, null until converted), Source (enum: WalkIn, Phone, Online, Referral, SocialMedia), Status (enum: New, Contacted, Qualified, Converted, Lost), Notes, AssignedToUserId.
5. `DiscountCard` : TenantScopedEntity. ClientId FK, CardType (enum: Saada, Fazaa, Custom), CardNumber, DiscountPercentage (decimal), ValidUntil.
6. Migration: `dotnet ef migrations add InitClientManagement`

**Tests:**
- [ ] Integration test: Create client with documents, query by category, verify filtering works
- [ ] Integration test: EmiratesId uniqueness within tenant enforced

**Acceptance:** Client tables exist. Category auto-detection from EmiratesId format is ready.

**Status:** ✅ Complete

---

## P1-T03: Implement ClientService with Category Auto-Detection and Verification Workflow

**Dependencies:** P1-T02

**Files:**
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Services/ClientService.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/Services/CategoryDetector.cs`

**Instructions:**
1. `RegisterAsync`: Validate documents, auto-detect category from EmiratesId (UAE nationals have a specific prefix pattern), create Client, publish ClientRegisteredEvent. If CategoryOverride is provided and user has clients.manage permission, use override.
2. `VerifyAsync`: Set IsVerified=true, VerifiedAt, VerifiedByUserId. Publish ClientVerifiedEvent. This event unblocks contract creation.
3. `BlockAsync`: Set SponsorFileStatus=Blocked, publish ClientBlockedEvent. All pending contracts for this client are paused.
4. `ListAsync`: Support `filter[category]=local&filter[category]=expat` (array), `filter[isVerified]=true`, `filter[sponsorFileStatus]=active`, `filter[nationality]=UAE`, `filter[createdAt][gte]=...`, `sort=-createdAt`.
5. `CategoryDetector`: Static utility. Parse EmiratesId format to determine Local vs Expat. Investor/VIP set manually.
6. `GetLeadsAsync`: `filter[status]=new&filter[status]=contacted&filter[source]=walkIn`, `sort=-createdAt`. Conversion funnel analytics.

**Tests:**
- [ ] Unit test: CategoryDetector identifies UAE national from EmiratesId prefix
- [ ] Unit test: VerifyAsync publishes ClientVerifiedEvent
- [ ] Unit test: BlockAsync publishes ClientBlockedEvent
- [ ] Unit test: ListAsync with `filter[category]=local&filter[category]=vip` returns both categories

**Acceptance:** Client registration, verification, and lead tracking work with full filter/sort support.

**Status:** ✅ Complete

---

## P1-T04: Create Client API Controllers and ServiceRegistration

**Dependencies:** P1-T03

**Files:**
- `src/TadHub.Api/Controllers/Tadbeer/ClientsController.cs`
- `src/TadHub.Api/Controllers/Tadbeer/LeadsController.cs`
- `src/Modules/Tadbeer/ClientManagement/ClientManagement.Core/ClientManagementServiceRegistration.cs`

**Instructions:**
1. `ClientsController`: POST `/api/v1/tadbeer/clients`, GET `.../clients` (with filters), GET `.../clients/{id}`, PUT `.../clients/{id}`, POST `.../clients/{id}/verify`, POST `.../clients/{id}/block`.
2. GET `.../clients/{id}/documents`, POST `.../clients/{id}/documents` (multipart upload via IFileStorageService).
3. GET `.../clients/{id}/communications`, POST `.../clients/{id}/communications`.
4. `LeadsController`: GET `/api/v1/tadbeer/leads?filter[status]=new&filter[status]=contacted&sort=-createdAt`, POST `.../leads`, PUT `.../leads/{id}`.
5. All endpoints use `[HasPermission('clients.*')]` as appropriate.

**Tests:**
- [ ] Integration test: Full client lifecycle: register > upload docs > verify > query by category array filter
- [ ] Integration test: Lead funnel: create lead > qualify > convert (links to client)
- [ ] Integration test: Receptionist can register clients but cannot block them (permission check)

**Acceptance:** Client management module complete.

**Status:** ✅ Complete

---

## Summary

Phase 1 Client Management module is complete with:
- Full CRUD for Clients with category auto-detection from Emirates ID
- Document vault management
- Communication logging
- Lead management with sales funnel tracking
- Discount card management (Saada, Fazaa)
- Rich filtering and sorting (array filters supported)
- Events: ClientRegisteredEvent, ClientVerifiedEvent, ClientBlockedEvent

Files created:
- ClientManagement.Contracts (DTOs, interfaces)
- ClientManagement.Core (entities, services, EF configs)
- ClientManagement.Api (controllers)

Pending: Unit tests and integration tests (marked in checkboxes).
