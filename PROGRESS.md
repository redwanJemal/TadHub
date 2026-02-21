# TadHub - Tadbeer ERP Platform Progress

**UAE Domestic Worker Recruitment Lifecycle Management**

## Overview

| Metric | Value |
|--------|-------|
| **Total Phases** | 11 (P0-P10) |
| **Total Tasks** | 28 |
| **Total Entities** | ~50 |
| **Domain Events** | 32 |
| **Estimated Timeline** | 15 weeks |

## Phase Summary

| Phase | Module | Tasks | Status | Progress |
|-------|--------|-------|--------|----------|
| P0 | Boilerplate Adaptation | 4 | ✅ Complete | 4/4 |
| P1 | Client Management | 4 | ✅ Complete | 4/4 |
| P2 | Worker/CV Management | 5 | ✅ Complete | 5/5 |
| P3 | Contract Engine | 4 | ⏳ Pending | 0/4 |
| P4 | Financial & Billing | 3 | ⏳ Pending | 0/3 |
| P5 | PRO & Govt Gateway | 3 | ⏳ Pending | 0/3 |
| P6 | Scheduling & Dispatch | 1 | ⏳ Pending | 0/1 |
| P7 | WPS | 1 | ⏳ Pending | 0/1 |
| P8 | Notification Extensions | 1 | ⏳ Pending | 0/1 |
| P9 | Reporting & Dashboards | 1 | ⏳ Pending | 0/1 |
| P10 | Integration Tests | 1 | ⏳ Pending | 0/1 |

**Overall Progress: 13/28 tasks (46%)**

---

## Phase Details

### Phase 0: Boilerplate Adaptation (Week 1) ✅

Customize existing boilerplate for Tadbeer domain.

| Task | Description | Status |
|------|-------------|--------|
| P0-T01 | Extend Tenant entity for Tadbeer agency fields | ✅ Complete |
| P0-T02 | Seed Tadbeer domain roles and permissions | ✅ Complete |
| P0-T03 | Register all Tadbeer domain events (32 events) | ✅ Complete |
| P0-T04 | Create localization infrastructure for bilingual support | ✅ Complete |

---

### Phase 1: Client Management (Weeks 2-4, parallel with P2) ✅

Employer lifecycle: registration, verification, lead tracking.

| Task | Description | Status |
|------|-------------|--------|
| P1-T01 | Create ClientManagement.Contracts | ✅ Complete |
| P1-T02 | Create Client entities and EF configuration | ✅ Complete |
| P1-T03 | Implement ClientService with category auto-detection | ✅ Complete |
| P1-T04 | Create Client API controllers and ServiceRegistration | ✅ Complete |

---

### Phase 2: Worker/CV Management (Weeks 2-4, parallel with P1) ✅

20-state worker state machine, CV management, nationality pricing.

| Task | Description | Status |
|------|-------------|--------|
| P2-T01 | Create Worker.Contracts | ✅ Complete |
| P2-T02 | Create Worker entities (8) and state machine enum | ✅ Complete |
| P2-T03 | Implement Worker state machine with domain events | ✅ Complete |
| P2-T04 | Implement WorkerService and WorkerSearchService | ✅ Complete |
| P2-T05 | Create Worker API controllers and ServiceRegistration | ✅ Complete |

---

### Phase 3: Contract Engine (Weeks 5-7)

Three contract types with lifecycle strategies.

| Task | Description | Status |
|------|-------------|--------|
| P3-T01 | Create Contract.Contracts and entities | ⏳ Pending |
| P3-T02 | Implement contract lifecycle strategies (Traditional/Temporary/Flexible) | ⏳ Pending |
| P3-T03 | Implement ContractService orchestrating strategies and events | ⏳ Pending |
| P3-T04 | Create Contract API controllers, consumers, ServiceRegistration | ⏳ Pending |

---

### Phase 4: Financial & Billing (Weeks 8-10, parallel with P5)

Invoicing, VAT, milestones, X-Reports.

| Task | Description | Status |
|------|-------------|--------|
| P4-T01 | Create Financial entities (10+) and EF config | ⏳ Pending |
| P4-T02 | Implement Financial services (invoicing, payments, VAT, refunds, X-Reports) | ⏳ Pending |
| P4-T03 | Create Financial API controllers and ServiceRegistration | ⏳ Pending |

---

### Phase 5: PRO & Government Gateway (Weeks 8-10, parallel with P4)

8 government transaction types with manual/API abstraction.

| Task | Description | Status |
|------|-------------|--------|
| P5-T01 | Create PRO entities and transaction types | ⏳ Pending |
| P5-T02 | Implement ProGatewayService with IGovernmentTransactionService abstraction | ⏳ Pending |
| P5-T03 | Create PRO API controllers and ServiceRegistration | ⏳ Pending |

---

### Phase 6: Scheduling & Dispatch (Weeks 11-12, parallel with P7)

Flexible bookings with labor law enforcement.

| Task | Description | Status |
|------|-------------|--------|
| P6-T01 | Create Scheduling entities, booking engine, conflict detection, labor law | ⏳ Pending |

---

### Phase 7: WPS (Weeks 11-12, parallel with P6)

SIF generation, payroll, compliance.

| Task | Description | Status |
|------|-------------|--------|
| P7-T01 | Implement WPS module (payroll, SIF generation, compliance) | ⏳ Pending |

---

### Phase 8: Notification Extensions (Weeks 13-14, parallel with P9)

WhatsApp, SMS, bilingual templates, escalation.

| Task | Description | Status |
|------|-------------|--------|
| P8-T01 | Extend Notification module for Tadbeer domain | ⏳ Pending |

---

### Phase 9: Reporting & Dashboards (Weeks 13-14, parallel with P8)

Role-based dashboards, MoHRE compliance reports.

| Task | Description | Status |
|------|-------------|--------|
| P9-T01 | Implement Reporting module with role-based dashboards | ⏳ Pending |

---

### Phase 10: Integration Tests (Week 15)

End-to-end business flow validation.

| Task | Description | Status |
|------|-------------|--------|
| P10-T01 | Write cross-module integration tests | ⏳ Pending |

---

## Key Milestones

| Milestone | Target | Status |
|-----------|--------|--------|
| Boilerplate adapted | Week 1 | ✅ |
| Client + Worker modules complete | Week 4 | ✅ |
| Contract Engine complete | Week 7 | ⏳ |
| Financial + PRO complete | Week 10 | ⏳ |
| Scheduling + WPS complete | Week 12 | ⏳ |
| Notifications + Reporting complete | Week 14 | ⏳ |
| All integration tests passing | Week 15 | ⏳ |

---

## Legend

- ⏳ Pending
- 🔄 In Progress
- ✅ Complete
- ⚠️ Blocked

---

## Notes

- **Boilerplate Foundation Complete**: Multi-tenancy, IAM, JWT auth, MassTransit event bus, Redis caching, SSE, Hangfire jobs, PostgreSQL with EF Core, API response envelope, bracket-notation filters, RFC 9457 error handling.
- **Backend Only**: Frontend will be developed separately.
- **Compliance**: Federal Decree-Law No. 9 of 2022, MoHRE Standards, WPS 2025.

---

## Recent Changes

### 2026-02-21: ReferenceData Module

**Added:** New `ReferenceData` module for global reference data shared across all modules.

**Components:**
- `Country` entity: ISO 3166-1 codes, bilingual names (en/ar), nationality adjectives, dialing codes
- `JobCategory` entity: 19 MoHRE official job categories (moved from Worker module)
- API endpoints: `/api/v1/countries`, `/api/v1/job-categories`
- Seeders: 70+ countries (10 common Tadbeer nationalities prioritized), 19 job categories

**Files Added:**
- `src/Modules/ReferenceData/` (new module)
- `src/TadHub.Api/Controllers/CountriesController.cs`
- `src/TadHub.Api/Controllers/JobCategoriesController.cs`

**Files Modified:**
- Worker module now references `ReferenceData.Core.Entities.JobCategory`
- Migration `AddReferenceDataModule` adds `countries` table

---

## Recent Fixes

### 2026-02-21: Authorization Global Query Filter Fix

**Issue:** Tenant-scoped endpoints returning 403 even with correct permissions assigned.

**Root Cause:** `AppDbContext` applies a global query filter `WHERE TenantId = _tenantContext.TenantId` to all `TenantScopedEntity` queries. The `GetUserPermissionsAsync` method was explicitly filtering by `tenantId` parameter, but the global filter was also applied, causing a conflict when `_tenantContext.TenantId` didn't match or was resolved differently.

**Fix:** Added `.IgnoreQueryFilters()` to permission lookup queries in `AuthorizationModuleService.cs` to bypass the global filter when explicitly filtering by tenant.

**Files Changed:**
- `src/Modules/Authorization/Authorization.Core/Services/AuthorizationModuleService.cs`

### 2026-02-21: CORS Configuration Fix

**Issue:** CORS headers not being sent, causing browser requests to fail.

**Root Cause:** Environment variable naming mismatch. Used `Cors__Origins__0` but `CorsSettings` expects `Cors__AllowedOriginsString` (comma-separated) or `Cors__AllowedOrigins__0`.

**Fix:** Use `Cors__AllowedOriginsString=https://tadbeer.endlessmaker.com,https://admin.endlessmaker.com`
