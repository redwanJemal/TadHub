You are implementing task "Placement — Outside Country Flow" for the TadHub project (a Tadbeer maid recruitment SaaS platform).

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

## CODING STANDARDS (MANDATORY — follow these exactly)

# TadHub Coding Standards

Follow these patterns EXACTLY. Deviations will be caught by the validation script.

## Backend

### Controller Rules
- Inherit `ControllerBase` with `[ApiController]` attribute
- Route: `[Route("api/v1/tenants/{tenantId:guid}/{resource}")]`
- Always: `[Authorize]` + `[TenantMemberRequired(TenantIdParameter = "tenantId")]`
- Permission: `[HasPermission("resource.action")]` on each endpoint
- HTTP methods: GET (list/detail), POST (create), PATCH (update), DELETE (delete)
- Return types: `IActionResult` (never typed `ActionResult<T>`)
- List params: `[FromQuery] QueryParameters qp, CancellationToken ct`
- Create: return `CreatedAtAction(nameof(GetById), new { tenantId, id }, dto)`
- Errors: use `MapResultError()` helper that switches on `result.ErrorCode`
- Add `[ProducesResponseType]` attributes on every endpoint
- Controllers ARE the BFF layer — they MAY inject multiple module services for composition/enrichment

### Service Rules
- Interface in `{Module}.Contracts/I{Entity}Service.cs`
- Implementation in `{Module}.Core/Services/{Entity}Service.cs`
- ALL methods return `Result<T>` or `Result` (never throw domain exceptions)
- ALL async methods take `CancellationToken ct = default` as last parameter
- Method names: `GetByIdAsync`, `ListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `TransitionStatusAsync`
- List returns `PagedList<TListDto>` (not Result-wrapped)
- Use `FilterableFields` and `SortableFields` dictionaries with `ApplyFilters`/`ApplySort`
- Use `_db.Set<Entity>().IgnoreQueryFilters().AsNoTracking()` for reads
- Always filter: `.Where(x => x.TenantId == tenantId && !x.IsDeleted)`
- Publish events via `IPublishEndpoint` after successful mutations

### DTO Rules
- ALL DTOs: `public sealed record` with `{ get; init; }` properties
- Naming: `{Entity}Dto`, `{Entity}ListDto`, `Create{Entity}Request`, `Update{Entity}Request`
- Full DTO: all fields + audit info (CreatedAt, UpdatedAt)
- List DTO: lightweight subset for table views
- Create request: required fields with `[Required]`, `[MaxLength]` attributes
- Update request: all fields nullable (PATCH semantics)
- Bilingual: `NameEn` + `NameAr` pattern
- Nested refs: lightweight `{Entity}RefDto` with just Id + Name

### Entity Rules
- Inherit from `TenantScopedEntity` (which inherits `SoftDeletableEntity` → `BaseEntity`)
- EF config: `IEntityTypeConfiguration<T>` in `{Module}.Core/Persistence/`
- Enums: stored as strings via `.HasConversion<string>()` with `.HasMaxLength(30)`
- Table name: snake_case plural (e.g., `candidates`, `placements`)
- Column names: auto snake_case via convention
- Indexes: named `ix_{table}_{column}`

### Error Codes (standardized)
- `"NOT_FOUND"` → 404
- `"VALIDATION_ERROR"` → 400
- `"CONFLICT"` → 409
- `"UNAUTHORIZED"` → 401
- `"FORBIDDEN"` → 403

### Permission Naming
- Format: `{resource}.{action}`
- Actions: `view`, `create`, `edit`, `delete`, `manage_status`, `manage`
- Seed in `Authorization.Core/Seeds/PermissionSeeder.cs`

### Module Registration
- Extension method: `Add{Module}Module()` in `{Module}ServiceRegistration.cs`
- Register in `Program.cs`: `builder.Services.Add{Module}Module()`
- Add project refs to `TadHub.Api.csproj` for both `.Contracts` and `.Core`

### Cross-Module Communication (CRITICAL — STRICTLY ENFORCED)

**RULE: Modules MUST NOT call each other's services directly.**

- A module's `.Core` project MUST NOT reference another module's `.Contracts` or `.Core` project
- A service MUST NOT inject or call interfaces from other modules (e.g., CandidateService must NOT inject ISupplierService)
- A consumer MUST NOT inject services from other modules — use only the event payload data

**How modules communicate:**
1. **Events (MassTransit pub/sub):** Module A publishes an event → Module B has a consumer that reacts
   - Events defined in `TadHub.SharedKernel/Events/` (shared, not module-owned)
   - Include ALL necessary data in the event payload (snapshot pattern) so consumers don't need to call back
   - Example: `CandidateApprovedEvent` includes full `CandidateSnapshotDto` with all fields
2. **Raw SQL:** When a service needs read-only data from another module's tables, use `_db.Database.SqlQueryRaw<T>()`
3. **BFF at Controller layer:** Controllers (in TadHub.Api) CAN inject multiple module services to compose responses
   - Use sequential foreach for enrichment (NOT Task.WhenAll — DbContext is not thread-safe)
   - Example: ContractsController enriches ContractDto with worker name from IWorkerService

**What to put in events:**
- Include snapshot DTOs with all fields the consumer might need
- Include IDs, names, status, timestamps — anything the consumer would otherwise need to query
- NEVER make the consumer call back to the publishing module for more data

**Known violations (legacy — do NOT add more):**
- PlacementService injects ICandidateService, IClientService
- ContractService injects IWorkerService, IClientService
- CandidateService injects ISupplierService
- These will be refactored. New code MUST NOT repeat this pattern.

## Frontend

### Type Rules
- All types in `features/{feature}/types.ts`
- camelCase properties (match JSON serialization)
- Enum/union values: PascalCase strings matching backend (e.g., `'Active'`, `'Abroad'`)
- Interfaces mirror backend DTOs: `CandidateDto`, `CandidateListDto`, `CreateCandidateRequest`, `UpdateCandidateRequest`

### API Rules
- API functions in `features/{feature}/api.ts`
- Use `apiClient.get/post/patch/delete` (never raw fetch)
- Use `tenantPath()` helper for tenant-scoped routes
- Function names: `list{Entity}s`, `get{Entity}`, `create{Entity}`, `update{Entity}`, `delete{Entity}`

### Hook Rules
- Hooks in `features/{feature}/hooks.ts`
- Use TanStack Query: `useQuery` for reads, `useMutation` for writes
- Query key: `['{entity}s', params]` for lists, `['{entity}s', id]` for single
- Mutations must `invalidateQueries` on success
- Hook names: `use{Entity}s`, `use{Entity}`, `useCreate{Entity}`, `useUpdate{Entity}`, `useDelete{Entity}`

### Constants Rules
- Constants in `features/{feature}/constants.ts`
- `STATUS_CONFIG` map with variant, icon, category
- `ALLOWED_TRANSITIONS` map for state machines
- `ALL_{ENUM}S` arrays for filter dropdowns

### i18n Rules
- Feature translations: `features/{feature}/i18n/en.json` and `ar.json`
- Common translations: `i18n/common/en.json` and `ar.json`
- Key structure: `{feature}.{section}.{key}` (e.g., `candidates.create.fullName`)
- Always add BOTH English and Arabic translations

### Loading States (MANDATORY — Skeleton Loading)

**RULE: Every page MUST have skeleton loading. Never show blank screens or spinner-only states.**

**List Pages:**
- Use `DataTableAdvanced` with `isLoading={isLoading}` prop — it renders skeleton rows automatically
- Summary cards above tables: show `<Skeleton className="h-8 w-16" />` when loading

**Detail Pages:**
- Create a `DetailSkeleton()` component at the bottom of the page file
- Return it when `isLoading` is true, BEFORE the main render
- Skeleton should mirror the page layout: Card with Skeleton blocks for each info section
- Pattern:
```tsx
function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-20" />
      </div>
      <Card>
        <CardHeader><Skeleton className="h-5 w-32" /></CardHeader>
        <CardContent className="grid grid-cols-2 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="space-y-1">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-5 w-40" />
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}
```

**Create/Edit Pages:**
- If pre-loading data (edit page), show skeleton for the form until data arrives
- Create pages with no pre-load don't need skeleton

**Dashboard:**
- Use `KpiCard` with `isLoading` prop for stat cards
- Show skeleton for charts/activity feeds while loading

**Import:** `import { Skeleton } from '@/shared/components/ui/skeleton'`

### Page Rules
- List page: DataTableAdvanced with filters, search, pagination, skeleton loading
- Detail page: Card-based layout with Info items, DetailSkeleton on load
- Create/Edit: Form with controlled state, toast on success, navigate back
- Use `PermissionGate` for permission-gated UI elements
- No `Textarea` component — use `<textarea>` with Tailwind classes

### Sidebar
- Add nav entry in `shared/components/layout/Sidebar.tsx`
- Follow existing section grouping pattern
- Add i18n keys for nav labels

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
  - status-alignment: pending
  - step-enforcement: pending
  - integrations: pending
  - frontend: pending
  - screenshots: pending



## TASK SPECIFICATION

# Task 15: Placement — Outside Country Deployment Flow

## Summary
Align the Placement module's outside-country flow with the spec's 9-step process: Booking → Contract → Employment Visa → Ticket → Arrival → Deployment → Full Payment → Residence Visa → Emirates ID.

## Current State
- `Placement` module exists with 13 statuses: Booked, TicketArranged, InTransit, Arrived, MedicalInProgress, MedicalCleared, GovtProcessing, GovtCleared, Training, ReadyForPlacement, Placed, Completed, Cancelled
- No explicit step-by-step enforcement of the spec's flow
- No integration with visa module (not yet built)
- Contract creation is separate from placement flow

## Required Changes

### 15.1 Placement Status Alignment for Outside Country

Map the spec's 9 steps to placement statuses:

| Spec Step | Placement Status | Notes |
|---|---|---|
| 1. Booking | `Booked` | Partial/advance payment required |
| 2. Contract Creation | `ContractCreated` | 2-year contract generated |
| 3. Employment Visa | `EmploymentVisaProcessing` | Track via Visa module |
| 4. Ticket Processing | `TicketArranged` | Ticket issued, travel date set |
| 5. Arrival | `Arrived` | Arrival module handles details |
| 6. Deployment | `Deployed` | Maid deployed to customer |
| 7. Full Payment | `FullPaymentReceived` | Verify remaining balance paid |
| 8. Residence Visa | `ResidenceVisaProcessing` | Track via Visa module |
| 9. Emirates ID | `EmiratesIdProcessing` | Track via Visa module |

- [ ] Add new statuses or sub-steps to `PlacementStatus` enum if needed
- [ ] Or use a `PlacementStep` tracking entity alongside status

### 15.2 Step Enforcement

- [ ] Booking requires partial/advance payment — validate payment before proceeding
- [ ] Contract must be created before employment visa can start
- [ ] Employment visa must be approved before ticket can be arranged
- [ ] Ticket must be arranged before arrival can be tracked
- [ ] Full payment must be verified before or at deployment
- [ ] Residence visa follows after deployment
- [ ] Emirates ID follows after residence visa

### 15.3 Placement Checklist/Progress Tracker

- [ ] Create a step checklist on the placement detail page
- [ ] Each step shows: status (pending/in-progress/completed), required documents, actions
- [ ] Visual progress bar showing how far along the placement is

### 15.4 Integration Points

- [ ] **Step 1 (Booking)**: link to payment module — partial payment creation
- [ ] **Step 2 (Contract)**: auto-create or link to contract
- [ ] **Step 3 (Employment Visa)**: create visa application (Task 05)
- [ ] **Step 4 (Ticket)**: ticket details entry, trigger arrival creation (Task 06)
- [ ] **Step 5 (Arrival)**: link to arrival module (Task 06)
- [ ] **Step 6 (Deployment)**: update worker status, trigger commission (Task 11)
- [ ] **Step 7 (Full Payment)**: verify invoice is fully paid
- [ ] **Step 8 (Residence Visa)**: create visa application (Task 05)
- [ ] **Step 9 (Emirates ID)**: create visa application (Task 05)

### 15.5 API Updates

- [ ] `PUT /api/placements/{id}/advance-step` — move to next step (with validation)
- [ ] `GET /api/placements/{id}/checklist` — get step checklist with status
- [ ] Existing placement endpoints may need updates

### 15.6 Frontend

- [ ] Placement detail page: step-by-step progress view
- [ ] Each step expandable with details, documents, and actions
- [ ] Placement board: visual kanban or pipeline view showing maids at each step
- [ ] Quick actions per step (create contract, start visa, arrange ticket, etc.)

## Acceptance Criteria

1. Outside-country placement follows the 9-step process in order
2. Each step has validation — cannot skip steps
3. Booking requires partial/advance payment
4. 2-year contract is created at step 2
5. Employment visa application is linked at step 3
6. Ticket and travel details are tracked at step 4
7. Arrival is managed through the arrival module at step 5
8. Full payment is verified at step 7
9. Residence visa and Emirates ID are tracked at steps 8-9
10. Placement detail shows a visual progress tracker with all 9 steps
11. Placement board shows pipeline view of all active placements
12. Each step shows required documents and their upload status

## Dependencies
- Task 05 (Visa Processing) for steps 3, 8, 9
- Task 06 (Arrival Management) for step 5
- Task 11 (Finance Enhancements) for commission calculation at step 6

## KNOWN ISSUES & SOLUTIONS (check FIRST before debugging)

Read scripts/known-issues.md BEFORE you start. If you hit any issue:
1. Check scripts/known-issues.md — if the issue is documented, follow the solution exactly
2. If NOT documented, research and fix it properly at the root cause
3. After fixing, APPEND your issue and solution to scripts/known-issues.md following the format:
   Symptom → Root Cause → Solution → DO NOT (what NOT to do as workaround)

## ERROR HANDLING POLICY (CRITICAL — NO WORKAROUNDS)

- NEVER use workarounds, hacks, or shortcuts to bypass errors
- NEVER use `// @ts-ignore`, `any` type, `--no-verify`, or `--force` to make things pass
- NEVER skip a failing step — fix the root cause
- If a build fails, read the FULL error, understand WHY, and fix the source
- If a test fails, fix the code or the test — never delete or skip tests
- If an EF migration fails, understand the model mismatch and resolve it
- If a cross-module dependency is needed, use events/raw SQL/BFF — NEVER add direct service references
- Take your time. Research the issue. Read relevant source files. Fix it properly.
- After resolving any non-trivial issue, document it in scripts/known-issues.md

## INSTRUCTIONS

1. Read the task specification carefully. Implement ALL items — backend entities, services, API, permissions, EF migration, frontend pages, and i18n.
2. For NEW modules: create both Contracts and Core projects following the Placement module pattern. Add project references to TadHub.sln and TadHub.Api.csproj.
3. For existing module changes: read the current code first, then modify.
4. Always create an EF migration after entity changes.
5. Add permissions to PermissionSeeder.cs.
6. Add sidebar nav entries (follow existing pattern in Sidebar.tsx).
7. Add i18n keys to both en.json and ar.json.
8. Frontend: ALL pages MUST have skeleton loading states (Skeleton component for detail pages, DataTableAdvanced isLoading for list pages). Never show blank screens.
9. After completing the implementation, verify by:
   - Running: dotnet build src/TadHub.Api/TadHub.Api.csproj (must succeed with 0 errors)
   - Checking that the frontend compiles: cd web/tenant-app && npx tsc --noEmit (must succeed with 0 errors)
   - If either fails, FIX the errors — do not proceed with a broken build
10. After ALL work is done, update the progress file at docs/tasks/progress.json:
    - Set tasks.15-placement-outside-country-flow.status to "completed"
    - Set each subtask to "completed" as you finish them
    - Set tasks.15-placement-outside-country-flow.completed_at to current ISO timestamp
    - Add any important notes to tasks.15-placement-outside-country-flow.notes
11. Finally, create a git commit with message: "feat: implement 15-placement-outside-country-flow — Placement — Outside Country Flow"

DO NOT skip any part of the task. Implement everything listed in the spec.
If you encounter a blocker, update progress.json notes field with the issue and set status to "failed".
