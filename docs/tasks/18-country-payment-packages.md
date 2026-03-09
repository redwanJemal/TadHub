# Task 18: Country Payment Packages (Reference Data)

## Summary
UAE government (MOHRE/Tadbeer) defines standard pricing schemas per country of origin for maid recruitment. These should be stored as reference data packages that auto-populate costs at the contract/placement level but can be overridden per candidate or contract.

## Current State
- `Countries` reference data exists in ReferenceData module
- `PlacementCostItem` tracks per-placement costs but with no defaults
- No package/pricing reference data exists
- Every cost must be entered manually per placement/contract

## Required Changes

### 18.1 CountryPackage Entity

**In ReferenceData module** (`src/Modules/ReferenceData/`):

- [ ] Create `CountryPackage` entity (TenantScopedEntity):
  - `CountryId` (Guid — links to existing Country reference)
  - `Name` (string — e.g., "Ethiopia Standard", "Philippines Premium")
  - `IsDefault` (bool — default package for this country)
  - `MaidCost` (decimal — base procurement/recruitment cost)
  - `MonthlyAccommodationCost` (decimal)
  - `VisaCost` (decimal — employment + residence visa combined or split)
  - `EmploymentVisaCost` (decimal)
  - `ResidenceVisaCost` (decimal)
  - `MedicalCost` (decimal)
  - `TransportationCost` (decimal — local transport)
  - `TicketCost` (decimal — flight cost)
  - `InsuranceCost` (decimal)
  - `EmiratesIdCost` (decimal)
  - `OtherCosts` (decimal)
  - `TotalPackagePrice` (decimal — computed or manual)
  - `SupplierCommission` (decimal — amount or percentage)
  - `SupplierCommissionType` (enum: FixedAmount, Percentage)
  - `DefaultGuaranteePeriod` (enum: SixMonths, OneYear, TwoYears)
  - `Currency` (string, default "AED")
  - `EffectiveFrom` (DateOnly)
  - `EffectiveTo` (DateOnly, nullable — null means currently active)
  - `IsActive` (bool)
  - `Notes` (string, nullable)
- [ ] EF configuration with snake_case naming
- [ ] Migration

### 18.2 Package Service

- [ ] `GetPackagesByCountryAsync(countryId)` — list packages for a country
- [ ] `GetDefaultPackageAsync(countryId)` — get the default active package
- [ ] `CreatePackageAsync`, `UpdatePackageAsync`, `DeletePackageAsync`
- [ ] `GetActivePackagesAsync` — all active packages across countries
- [ ] Validation: only one default per country at a time

### 18.3 Auto-Population at Placement/Contract Level

- [ ] When creating a placement and selecting a worker:
  - Look up worker's nationality → find default country package
  - Auto-fill all cost fields from the package
  - All fields remain editable (override per case)
- [ ] When creating a contract:
  - Auto-fill guarantee period from package default
  - Auto-fill total value from package price
- [ ] When creating an invoice from contract:
  - Line items auto-populated from package breakdown
- [ ] Candidate-level override:
  - Add optional `PackageOverride` fields on Candidate or a linked entity
  - If a candidate has custom pricing, use that instead of country default

### 18.4 API

- [ ] `GET /api/country-packages` — list all (filterable by country, active status)
- [ ] `GET /api/country-packages/{id}` — detail
- [ ] `GET /api/country-packages/by-country/{countryId}` — packages for a country
- [ ] `GET /api/country-packages/default/{countryId}` — default package for a country
- [ ] `POST /api/country-packages` — create
- [ ] `PUT /api/country-packages/{id}` — update
- [ ] `DELETE /api/country-packages/{id}` — soft delete
- [ ] Permissions: `packages.view`, `packages.create`, `packages.edit`, `packages.delete`

### 18.5 Frontend — Package Management Page

- [ ] Package list page (grouped by country or flat with country filter)
- [ ] Create/edit package form:
  - Country selector
  - All cost fields with currency
  - Commission settings (amount + type)
  - Guarantee period default
  - Effective date range
  - Default toggle
- [ ] Package detail view with cost breakdown
- [ ] Add to sidebar under Settings or as a standalone reference data page

### 18.6 Frontend — Auto-Fill Integration

- [ ] Placement creation form:
  - On worker selection → fetch default package for worker's nationality
  - Auto-fill cost fields with "From package: [name]" indicator
  - Allow editing any field (shows "Modified" badge if changed from package)
- [ ] Contract creation form:
  - Auto-fill guarantee period and total value
  - Same override UX
- [ ] Invoice creation from contract:
  - Auto-populate line items from package cost breakdown
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Admin can create payment packages per country with all cost components
2. Each country can have multiple packages but only one default
3. Packages have effective date ranges (supports price changes over time)
4. When creating a placement, selecting a worker auto-fills costs from the default package for the worker's nationality
5. All auto-filled values can be overridden at the placement/contract level
6. Modified values are visually indicated (changed from package default)
7. When creating a contract, guarantee period and total value auto-fill from the package
8. Invoice line items can be auto-populated from the package cost breakdown
9. Packages support both fixed-amount and percentage-based supplier commissions
10. Historical packages are preserved (soft delete, effective date ranges)
11. Package management page is accessible to admin users
12. All cost fields use configurable currency (default AED)
