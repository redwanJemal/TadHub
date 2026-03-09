# Task 05: Visa Processing Module

## Summary
The spec requires tracking three distinct government processes: Employment Visa, Residence Visa, and Emirates ID. Each has different document requirements and status tracking. Currently, the Placement module only has generic `GovtProcessing`/`GovtCleared` states.

## Current State
- `Placement` has `GovtProcessing` and `GovtCleared` statuses (no distinction between visa types)
- `DocumentType` has `Visa`, `EmiratesId` but no process tracking
- No structured visa application workflow exists

## Required Changes

### 5.1 Visa Application Entity

- [ ] Create `VisaApplication` entity (SoftDeletableEntity):
  - `ApplicationCode` (auto-generated)
  - `WorkerId` (Guid)
  - `ClientId` (Guid)
  - `ContractId` (Guid, nullable)
  - `PlacementId` (Guid, nullable)
  - `VisaType` (enum: EmploymentVisa, ResidenceVisa, EmiratesId)
  - `Status` (enum: NotStarted, DocumentsCollecting, Applied, UnderProcess, Approved, Rejected, Issued, Expired, Cancelled)
  - `ApplicationDate` (DateTime, nullable)
  - `ApprovalDate` (DateTime, nullable)
  - `IssuanceDate` (DateTime, nullable)
  - `ExpiryDate` (DateTime, nullable)
  - `ReferenceNumber` (string, nullable)
  - `VisaNumber` (string, nullable)
  - `Notes` (string)
  - `RejectionReason` (string, nullable)
- [ ] Create `VisaApplicationStatusHistory` entity
- [ ] Create `VisaApplicationDocument` entity:
  - `VisaApplicationId` (Guid)
  - `DocumentType` (enum)
  - `FileUrl` (string)
  - `UploadedAt` (DateTime)
  - `IsVerified` (bool)
- [ ] EF configuration and migration

### 5.2 Document Requirements by Visa Type

**Employment Visa — Outside Country:**
- Attested Medical Certificate (mandatory)
- Passport Copy (mandatory)
- Photo (mandatory)

**Employment Visa — Inside Country:**
- Passport Copy (mandatory)
- Photo (mandatory)
- Medical Certificate (optional — but system should support upload)

**Residence Visa (both):**
- Local Medical (mandatory)
- Passport (mandatory)
- Photo (mandatory)

**Emirates ID:**
- No additional documents (tracks application → approval → issuance)

- [ ] Implement document requirement validation per visa type and worker location
- [ ] Show required vs optional documents in the UI

### 5.3 Visa Service

- [ ] `CreateVisaApplicationAsync` — validates required documents, creates application
- [ ] `UpdateVisaStatusAsync` — progress through statuses
- [ ] `UploadVisaDocumentAsync` — attach documents to application
- [ ] `GetVisaApplicationsByWorkerAsync` — all visa history for a worker
- [ ] `GetVisaApplicationsByPlacementAsync` — all visa apps for a placement
- [ ] Auto-sequence: Employment Visa must be completed before Residence Visa can start
- [ ] Publish events: `VisaApplicationCreatedEvent`, `VisaStatusChangedEvent`, `VisaIssuedEvent`

### 5.4 Integration with Placement

- [ ] Replace generic `GovtProcessing`/`GovtCleared` with visa-aware statuses, or link visa applications to placement steps
- [ ] Placement detail should show all associated visa applications and their progress
- [ ] Placement cannot advance to `Deployed/Placed` until Employment Visa is at least `Approved`

### 5.5 API

- [ ] `POST /api/visa-applications` — create
- [ ] `GET /api/visa-applications` — list with filters (worker, type, status)
- [ ] `GET /api/visa-applications/{id}` — detail
- [ ] `PUT /api/visa-applications/{id}/status` — update status
- [ ] `POST /api/visa-applications/{id}/documents` — upload document
- [ ] `GET /api/workers/{workerId}/visa-applications` — worker's visa history
- [ ] Permissions: `visas.view`, `visas.create`, `visas.manage`

### 5.6 Frontend

- [ ] Visa applications list page (filterable by type, status, worker)
- [ ] Create visa application form:
  - Select worker, visa type
  - Shows required documents based on type + worker location
  - Document upload area
- [ ] Visa application detail page:
  - Status timeline
  - Document checklist (required vs uploaded)
  - Status update actions
- [ ] Visa section on worker detail page (all visa applications)
- [ ] Visa progress indicators on placement detail/board
- [ ] Sidebar navigation entry (under a "Government" or "Visa" section)
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Office staff can create visa applications for Employment Visa, Residence Visa, and Emirates ID
2. Each visa type shows correct required documents based on worker location (inside/outside)
3. Employment Visa for outside-country requires: Attested Medical, Passport Copy, Photo
4. Employment Visa for inside-country requires: Passport Copy, Photo (Medical optional)
5. Residence Visa requires: Local Medical, Passport, Photo
6. Emirates ID tracks: Application → Approval → Issuance
7. Visa applications progress through statuses with full audit trail
8. System enforces sequencing: Employment Visa before Residence Visa
9. Placement cannot reach Deployed status without Employment Visa approval
10. Visa status updates trigger notifications
11. All visa history is visible on worker detail page
12. Visa reference numbers and dates are tracked
