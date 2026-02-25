# TadHub Project Review — February 25, 2026

## Executive Summary

TadHub is an **Enterprise Resource Planning (ERP) platform** for Tadbeer centers — UAE domestic worker recruitment agencies. It manages the full lifecycle of worker recruitment, placement, and employment with UAE regulatory compliance built in.

**Overall Completion: ~46% (13/28 phases complete)**
**Current Branch:** `feature/remove-tadbeer-module`

---

## Technology Stack

| Layer | Technologies |
|-------|-------------|
| **Backend** | .NET 9 (C# 13), Entity Framework Core, FluentValidation |
| **Database** | PostgreSQL 17 with Row-Level Security |
| **Auth** | Keycloak 26 (OIDC/JWT) |
| **Caching** | Redis 7 |
| **Messaging** | RabbitMQ 3 + MassTransit |
| **Search** | Meilisearch |
| **Storage** | MinIO (S3-compatible) |
| **Jobs** | Hangfire |
| **Frontend** | React 19, TypeScript 5.9, Vite 6, TailwindCSS, Zustand, TanStack Query/Table |
| **Testing** | xUnit (unit/integration), Playwright (E2E) |
| **CI/CD** | GitHub Actions, Docker Compose, Coolify PaaS |

---

## Architecture Highlights

### Modular Monolith (14 Modules)

```
src/Modules/
├── Tenancy           — Multi-tenant foundation
├── Identity          — User profiles & authentication
├── Authorization     — Roles & permissions (RBAC)
├── Notification      — Multi-channel notifications
├── Analytics         — Usage tracking
├── Audit             — Change auditing
├── ReferenceData     — Countries, job categories (seeded data)
├── Content           — Blog, pages, knowledge base
├── FeatureFlags      — A/B testing, per-tenant toggles
├── Portal            — Multi-portal support
├── Subscription      — Plan management
├── ApiManagement     — API keys, rate limiting
├── Supplier          — Supplier management
└── _Template         — Scaffolding template for new modules
```

Each module follows a **Contracts/Core** split — Contracts expose public interfaces and DTOs; Core contains entities, persistence, services, and event consumers.

### Key Architectural Patterns

- **Multi-Tenancy**: Global EF Core query filters + PostgreSQL Row-Level Security
- **Event-Driven**: 32 domain events, MassTransit consumers for cross-module communication
- **Entity Hierarchy**: `BaseEntity > TenantScopedEntity > SoftDeletableEntity + IAuditable`
- **EF Interceptors**: TenantId auto-assignment, RLS enforcement, audit tracking, soft-delete
- **Standardized API Envelope**: `ApiResponse<T>` with `ApiError` (RFC 7807 Problem Details)
- **Bracket-Notation Filtering**: `?filter[category]=local&sort=-createdAt`

### Frontend Structure

Two Vite-based React apps in a pnpm monorepo:

- **`web/tenant-app/`** — Tadbeer agency portal (features: auth, onboarding, team management)
- **`web/backoffice-app/`** — Platform admin portal (features: audit, tenant management, users)

Both include bilingual support (English/Arabic) via i18next with RTL layout.

---

## Completed Work

### Phase 0 — Boilerplate Adaptation
- Tenant entity extended with Tadbeer licensing fields
- 7 domain roles seeded (agency-admin, receptionist, cashier, pro-officer, agent, accountant, viewer)
- 32 Tadbeer domain events registered
- Bilingual (en/ar) localization infrastructure

### Phase 1 — Client Management
- Full CRUD with category auto-detection from Emirates ID
- Client categories: Local, Expat, Investor, VIP
- Document vault, communication logging, lead funnel
- Discount card management (Saada, Fazaa, Custom)

### Phase 2 — Worker/CV Management
- 20-state worker state machine
- CV management with job preferences
- Passport custody tracking, nationality-based pricing
- Full-text search via Meilisearch, PDF CV export

### Infrastructure Modules (Boilerplated)
Identity, Authorization, Audit, Notification, Analytics, ReferenceData, Content, FeatureFlags, Portal, Subscription, ApiManagement, Supplier

---

## Remaining Work (54%)

| Phase | Name | Status | Key Deliverables |
|-------|------|--------|------------------|
| P3 | Contract Engine | Pending | 3 contract types (Traditional/Temporary/Flexible), state machines, guarantee periods, replacement/refund flows |
| P4 | Financial & Billing | Pending | Invoicing, payments, VAT, milestone billing, X-Reports, credit notes |
| P5 | PRO & Government Gateway | Pending | 8 government transaction types (visa, medical, Emirates ID, insurance, etc.) |
| P6 | Scheduling & Dispatch | Pending | Booking engine, labor law enforcement, conflict detection |
| P7 | WPS Compliance | Pending | Payroll, SIF generation, MOHRE WPS integration |
| P8 | Notifications | Pending | WhatsApp, SMS, email, escalation policies, bilingual templates |
| P9 | Reporting & Dashboards | Pending | Role-based dashboards, compliance reports, analytics |
| P10 | Integration Tests | Pending | End-to-end business flow validation |

**Next priority:** Phase 3 (Contract Engine) — the core business logic.

---

## Codebase Metrics

| Metric | Count |
|--------|-------|
| C# Source Files | ~508 |
| TypeScript Files | ~150+ |
| API Controllers | 20+ |
| Database Tables | 30+ |
| Domain Events | 32 |
| EF Interceptors | 4 |
| Docker Services | 6 (Postgres, Keycloak, Redis, RabbitMQ, Meilisearch, MinIO) |

---

## Recent Activity (Last 15 Commits)

1. **fix:** Resolve 403 errors in tenant permission system (global query filter fix)
2. **fix:** Resolve duplicate Traefik router conflicts for API, tenant, and backoffice
3. **fix:** Resolve Keycloak 504 gateway timeout (duplicate Traefik router)
4. **fix:** Resolve tenant permission checks and API client error handling
5. **chore:** Update screenshots, remove stale worker/client screenshots
6. **feat:** Add Team Management page to tenant app
7. **feat:** Remove Worker & ClientManagement modules (keep Tenancy intact) — refactoring
8. **fix:** Correct Keycloak Change Password URL
9. **feat:** Add Change Password button to both apps
10. **feat:** Add client-level access control and branded tenant login page

Recent work has focused on **stabilization** — fixing authorization issues, Traefik routing, Keycloak integration — and **refactoring** by removing module definitions that were moved or consolidated.

---

## Deployment

| Service | URL |
|---------|-----|
| API | `https://api.endlessmaker.com` |
| Auth (Keycloak) | `https://auth.endlessmaker.com` |
| Storage (MinIO) | `https://storage.endlessmaker.com` |
| Tenant App | `https://tadbeer.endlessmaker.com` |

CI/CD via GitHub Actions: build, format check, unit tests (filtered), integration tests with Postgres + Redis services.

Multiple Docker Compose files: `docker-compose.yml` (dev), `docker-compose.prod.yml` (production), `docker-compose.coolify.yml` (Coolify PaaS).

---

## Observations & Recommendations

### Strengths
- **Solid architecture**: Modular monolith with clear boundaries, event-driven communication, proper multi-tenancy with RLS
- **Enterprise-grade foundations**: Audit trails, soft deletes, RBAC, feature flags — all in place
- **Production-ready infrastructure**: Docker, CI/CD, Keycloak SSO, distributed caching
- **Regulatory awareness**: UAE compliance (MoHRE, WPS, Emirates ID) deeply embedded in the domain model
- **Developer experience**: Type-safe stack end-to-end, auto-discovery of module configurations, structured logging

### Areas to Watch
- **Module refactoring in progress**: The current branch (`feature/remove-tadbeer-module`) indicates an ongoing restructuring — Worker & Client modules were removed from the module directory. This needs to be completed and merged cleanly.
- **Test coverage**: Unit tests exist for infrastructure concerns (interceptors, caching, exception handling, query parsing) but business logic tests for Client/Worker modules are not visible. Phase 10 (integration tests) is the last phase, which means business logic runs largely untested until the very end.
- **Frontend feature parity**: The frontend currently covers auth, onboarding, and team management. Phases 1-2 (Client/Worker) had backend work but the corresponding frontend pages appear to have been removed or are not yet rebuilt.
- **Contract Engine complexity**: P3 is the most complex remaining phase (3 contract types with different lifecycles, state machines, guarantees). Careful design upfront will pay dividends.
- **No database migrations visible**: EF Core migrations directory was not found — unclear if migrations are auto-applied or managed externally. This should be formalized before production.

---

*Review conducted by analyzing codebase structure, source files, configuration, git history, and documentation. No task files were consulted for status — all findings are derived from the code itself.*
