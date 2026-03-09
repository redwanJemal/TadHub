You are implementing task "Candidate/Maid Registration — Fill Gaps" for the TadHub project (a Tadbeer maid recruitment SaaS platform).

## PROJECT CONTEXT

This is a C# .NET 9 modular monolith with:
- Backend: src/Modules/{Module}/{Module}.Contracts/ (interfaces, DTOs) and {Module}.Core/ (entities, services, EF configs)
- API: src/TadHub.Api/Controllers/
- Infrastructure & Migrations: src/TadHub.Infrastructure/Persistence/Migrations/
- Frontend: React + TypeScript + Tailwind + Radix UI in web/tenant-app/src/
- Frontend features: web/tenant-app/src/features/{feature}/
- DB: PostgreSQL with EF Core, snake_case naming
- Events: MassTransit (IPublishEndpoint / IConsumer<T>)
- Return pattern: Result<T> (Success, NotFound, ValidationError, Conflict)
- Entity hierarchy: SoftDeletableEntity → TenantScopedEntity → BaseEntity
- EF configs use IEntityTypeConfiguration<T>, enums stored as strings
- DbContext auto-discovers configs from assemblies containing ".Core"
- Module registration: AddXxxModule() extension in {Module}.Core/{Module}ServiceRegistration.cs
- Permissions seeded in Authorization.Core/Seeds/PermissionSeeder.cs
- Frontend hooks: TanStack Query with apiClient helper
- i18n: web/tenant-app/src/i18n/common/en.json and ar.json
- No Textarea UI component — use plain <textarea> with Tailwind
- Cross-module data: use raw SQL to avoid circular project refs
- EF Migration path: src/TadHub.Infrastructure/Persistence/Migrations/
- Run migrations: dotnet ef migrations add MigrationName -p src/TadHub.Infrastructure -s src/TadHub.Api
- BFF enrichment: use sequential foreach (NOT Task.WhenAll) for DbContext calls
- Photo URLs need presigning via IFileStorageService

## EXISTING MODULE STRUCTURE (follow this pattern exactly)

Reference the Placement module as your template:
- src/Modules/Placement/Placement.Contracts/DTOs/PlacementDtos.cs
- src/Modules/Placement/Placement.Contracts/IPlacementService.cs
- src/Modules/Placement/Placement.Core/Entities/ (Placement.cs, PlacementStatus.cs, PlacementStatusHistory.cs, PlacementCostItem.cs, PlacementCostType.cs)
- src/Modules/Placement/Placement.Core/Persistence/ (PlacementConfiguration.cs, etc.)
- src/Modules/Placement/Placement.Core/Services/PlacementService.cs
- src/Modules/Placement/Placement.Core/PlacementServiceRegistration.cs
- src/TadHub.Api/Controllers/PlacementsController.cs

## SUBTASK PROGRESS

These subtasks have already been tracked:
  - backend: pending
  - frontend: pending
  - migration: pending
  - screenshots: pending



## TASK SPECIFICATION

# Task 01: Candidate/Maid Registration — Fill Gaps

## Summary
The current Candidate module covers most registration needs but is missing several fields and classification options required by the Tadbeer spec.

## Current State
- `Candidate` entity has: FullNameEn/Ar, Nationality, DateOfBirth, Gender, PassportNumber, PassportExpiry, Phone, Email, PhotoUrl, VideoUrl
- `CandidateSourceType`: Supplier, Local
- `WorkerLocation` (Abroad/InCountry) exists on Worker but not on Candidate
- Document types: Passport, Visa, WorkPermit, MedicalCertificate, InsurancePolicy, EmiratesId, LabourCard, Other

## Required Changes

### 1.1 Add Missing Fields to Candidate Entity

**Backend** (`src/Modules/Candidate/Candidate.Core/`)

- [ ] Add `PlaceOfBirth` (string, optional)
- [ ] Add `MaritalStatus` (enum: Single, Married, Divorced, Widowed)
- [ ] Add `LocationType` (enum: InsideCountry, OutsideCountry) — classification at registration time
- [ ] EF migration for new columns

**Frontend** (`web/tenant-app/src/features/candidates/`)

- [ ] Add PlaceOfBirth field to CreateCandidatePage and EditCandidatePage
- [ ] Add MaritalStatus dropdown to candidate forms
- [ ] Add Inside/Outside Country selector to candidate forms
- [ ] Display these fields on CandidateDetailPage
- [ ] Add i18n keys for new fields (en.json, ar.json)

### 1.2 Expand Document Types

**Backend** (`src/Modules/Document/Document.Core/`)

- [ ] Add `CertificateOfCompetency` to `DocumentType` enum
- [ ] Add `AttestedMedicalCertificate` to `DocumentType` enum (distinct from generic MedicalCertificate)
- [ ] Add `PassportCopy` to `DocumentType` enum
- [ ] Add `PersonalPhoto` to `DocumentType` enum
- [ ] EF migration for enum changes

**Frontend**

- [ ] Update document upload dialogs to show new document types
- [ ] Add i18n keys for new document types

### 1.3 Mandatory Document Validation

- [ ] On candidate submission, validate that Passport and PersonalPhoto documents are present (or flag as required)
- [ ] Backend validation in candidate creation/approval service
- [ ] Frontend: mark Passport and Photo as mandatory in the form UI

## Acceptance Criteria

1. A supplier or office staff can register a maid with all required personal fields: Full Name, Place of Birth, Date of Birth, Nationality, Marital Status, Phone
2. The form includes an Inside/Outside Country classification
3. Passport and Personal Photo are mandatory uploads at registration
4. Attested Medical Certificate and Certificate of Competency (CoC) are optional uploads
5. All new fields are visible on the candidate detail page
6. All new fields are searchable/filterable in the candidates list
7. Existing candidates without the new fields continue to work (nullable migration)
8. i18n translations exist for English and Arabic

## INSTRUCTIONS

1. Read the task specification carefully. Implement ALL items — backend entities, services, API, permissions, EF migration, frontend pages, and i18n.
2. For NEW modules: create both Contracts and Core projects following the Placement module pattern. Add project references to TadHub.sln and TadHub.Api.csproj.
3. For existing module changes: read the current code first, then modify.
4. Always create an EF migration after entity changes.
5. Add permissions to PermissionSeeder.cs.
6. Add sidebar nav entries (follow existing pattern in Sidebar.tsx).
7. Add i18n keys to both en.json and ar.json.
8. After completing the implementation, verify by:
   - Running: dotnet build src/TadHub.Api/TadHub.Api.csproj (must succeed)
   - Checking that the frontend compiles: cd web/tenant-app && npx tsc --noEmit
9. After ALL work is done, update the progress file at docs/tasks/progress.json:
   - Set tasks.01-candidate-registration-gaps.status to "completed"
   - Set each subtask to "completed" as you finish them
   - Set tasks.01-candidate-registration-gaps.completed_at to current ISO timestamp
   - Add any important notes to tasks.01-candidate-registration-gaps.notes
10. Finally, create a git commit with message: "feat: implement 01-candidate-registration-gaps — Candidate/Maid Registration — Fill Gaps"

DO NOT skip any part of the task. Implement everything listed in the spec.
If you encounter a blocker, update progress.json notes field with the issue and set status to "failed".
