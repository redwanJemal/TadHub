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
