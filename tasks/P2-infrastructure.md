# Phase 2: Infrastructure

Build the cross-cutting infrastructure layer: EF Core with multi-tenancy, Redis, MassTransit, SSE, MinIO, Meilisearch, and the API middleware pipeline.

**Dependencies:** P1 complete

---

## P2-T01: Create filter model binder and sort parser

### Dependencies
P1-T05

### Files to Create
```
src/SaasKit.Infrastructure/Api/QueryParametersModelBinder.cs
src/SaasKit.Infrastructure/Api/QueryParametersModelBinderProvider.cs
src/SaasKit.Infrastructure/Api/FilterParser.cs
src/SaasKit.Infrastructure/Api/SortParser.cs
```

### Instructions

1. **FilterParser** - Parse bracket notation filters:
   ```csharp
   public static class FilterParser
   {
       // filter[status]=active&filter[status]=pending → FilterField { Name="status", Values=["active","pending"] }
       // filter[amount][gte]=100 → FilterField { Name="amount", Operator=Gte, Values=["100"] }
       public static List<FilterField> Parse(IQueryCollection query) { ... }
   }
   ```

2. **SortParser** - Parse sort parameter:
   ```csharp
   public static class SortParser
   {
       // "-createdAt,name" → [{ Name="createdAt", Descending=true }, { Name="name", Descending=false }]
       public static List<SortField> Parse(string? sort) { ... }
   }
   ```

3. **QueryParametersModelBinder** - Bind QueryParameters from request:
   - Clamp page (min 1) and pageSize (min 1, max 100)
   - Parse filters and sort
   - Split fields and include as comma-separated lists

4. Register in Program.cs:
   ```csharp
   options.ModelBinderProviders.Insert(0, new QueryParametersModelBinderProvider());
   ```

### Tests
- [ ] FilterParser parses 'filter[status]=active&filter[status]=pending' → one FilterField with two values
- [ ] FilterParser parses 'filter[amount][gte]=100' → FilterField with Operator=Gte
- [ ] FilterParser parses 'filter[name][contains]=acme' → FilterField with Operator=Contains
- [ ] FilterParser parses 'filter[deletedAt][isNull]=true' → FilterField with Operator=IsNull
- [ ] SortParser parses '-createdAt,name' → two SortField objects
- [ ] pageSize > 100 clamped to 100, page < 1 clamped to 1
- [ ] Integration: Controller receives correctly parsed filters

### Acceptance Criteria
Any controller action can accept QueryParameters and receive parsed filters automatically.

---

## P2-T02: Create IQueryable filter and sort extension methods

### Dependencies
P2-T01, P1-T04

### Files to Create
```
src/SaasKit.Infrastructure/Api/QueryableFilterExtensions.cs
src/SaasKit.Infrastructure/Api/QueryableSortExtensions.cs
```

### Instructions

1. **ApplyFilters** extension:
   ```csharp
   public static IQueryable<T> ApplyFilters<T>(
       this IQueryable<T> query,
       List<FilterField> filters,
       Dictionary<string, Expression<Func<T, object>>> fieldMap)
   {
       // Eq: WHERE x.Prop IN (values)
       // Gt/Gte/Lt/Lte: comparison expressions
       // Contains: EF.Functions.ILike(x.Prop, "%value%")
       // IsNull: x.Prop == null
       // Multiple values for same field: OR
       // Multiple fields: AND
   }
   ```

2. **ApplySort** extension:
   ```csharp
   public static IQueryable<T> ApplySort<T>(
       this IQueryable<T> query,
       List<SortField> sortFields,
       Dictionary<string, Expression<Func<T, object>>> fieldMap)
   {
       // Apply OrderBy/ThenBy dynamically
       // Default: -createdAt if no sort fields
       // Unknown fields: return 400 (validated upstream)
   }
   ```

3. Services use one-liner:
   ```csharp
   var result = await dbContext.Tenants
       .ApplyFilters(qp.GetFilters(), filterableFields)
       .ApplySort(qp.GetSortFields(), sortableFields)
       .ToPagedListAsync(qp.Page, qp.PageSize);
   ```

### Tests
- [ ] ApplyFilters with Eq and two values generates WHERE ... IN
- [ ] ApplyFilters with Gte on date field generates correct comparison
- [ ] ApplyFilters with Contains generates ILIKE
- [ ] ApplySort with '-createdAt,name' applies OrderByDescending then ThenBy
- [ ] Integration: Insert 10 records, filter by status, verify only matching returned

### Acceptance Criteria
Services apply filters and sorting with one-liner extension calls.

---

## P2-T03: Create API response envelope wrapper and global error handler

### Dependencies
P1-T05

### Files to Create
```
src/SaasKit.Infrastructure/Api/ApiResponseWrappingMiddleware.cs
src/SaasKit.Infrastructure/Api/GlobalExceptionHandler.cs
src/SaasKit.Infrastructure/Api/FluentValidationFilter.cs
```

### Instructions

1. **ApiResponseWrappingMiddleware**:
   - Intercepts response body after controller executes
   - Raw object → wrap in ApiResponse<T>.Ok(data)
   - PagedList<T> → wrap in ApiPagedResponse<T>
   - Already ApiResponse or ApiError → pass through
   - Add meta.timestamp and meta.requestId

2. **GlobalExceptionHandler** (.NET 9 IExceptionHandler):
   - ValidationException → 422 ApiError.Validation
   - KeyNotFoundException → 404
   - UnauthorizedAccessException → 403
   - Others → 500 (generic in Production, detailed in Development)

3. **FluentValidationFilter** (IActionFilter):
   - If ModelState invalid → 422 ApiError.Validation
   - Short-circuit before controller action

4. Controllers return raw DTO/PagedList - middleware wraps

### Tests
- [ ] Middleware wraps raw DTO in { data, meta }
- [ ] Middleware wraps PagedList in { data, meta, pagination }
- [ ] GlobalExceptionHandler maps ValidationException to 422 with errors
- [ ] GlobalExceptionHandler maps unknown exception to 500
- [ ] FluentValidationFilter returns 422 when ModelState has errors
- [ ] Integration: POST with invalid body returns 422 RFC 9457 format
- [ ] Integration: GET /api/v1/tenants returns envelope format

### Acceptance Criteria
Every response follows envelope convention. Errors follow RFC 9457. Controllers stay thin.

---

## P2-T04: Create AppDbContext with multi-tenancy global query filters

### Dependencies
P1-T01, P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Persistence/AppDbContext.cs
src/SaasKit.Infrastructure/Persistence/DesignTimeDbContextFactory.cs
```

### Instructions

1. **AppDbContext**:
   ```csharp
   public class AppDbContext : DbContext, IUnitOfWork
   {
       private readonly ITenantContext _tenantContext;
       
       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           // Global query filter for TenantScopedEntity
           modelBuilder.Entity<TenantScopedEntity>()
               .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
           
           // Global query filter for SoftDeletableEntity
           modelBuilder.Entity<SoftDeletableEntity>()
               .HasQueryFilter(e => !e.IsDeleted);
           
           // Scan IEntityTypeConfiguration from assemblies
           modelBuilder.ApplyConfigurationsFromAssembly(...);
       }
   }
   ```

2. Use snake_case naming convention

3. **DesignTimeDbContextFactory** for EF tooling

### Tests
- [ ] Integration: Insert with tenantA, query with tenantB → empty
- [ ] Integration: Soft-deleted entities excluded from queries

### Acceptance Criteria
Multi-tenancy enforced at ORM level.

---

## P2-T05: Create EF Core interceptors

### Dependencies
P2-T04, P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs
src/SaasKit.Infrastructure/Persistence/Interceptors/TenantIdInterceptor.cs
src/SaasKit.Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs
src/SaasKit.Infrastructure/Persistence/Interceptors/RlsInterceptor.cs
```

### Instructions

1. **AuditableEntityInterceptor**: Set CreatedAt/UpdatedAt from IClock, CreatedBy/UpdatedBy from ICurrentUser

2. **TenantIdInterceptor**: Auto-set TenantId on new entities, throw on cross-tenant write

3. **SoftDeleteInterceptor**: Convert Delete to soft-delete (IsDeleted=true, DeletedAt set)

4. **RlsInterceptor**: SET app.current_tenant_id on connection open

### Tests
- [ ] AuditableEntityInterceptor sets timestamps
- [ ] TenantIdInterceptor throws on wrong-tenant write
- [ ] SoftDeleteInterceptor converts Delete to soft-delete

### Acceptance Criteria
Interceptors handle audit, tenancy, soft-delete automatically.

---

## P2-T06: Create PostgreSQL RLS policies init script

### Dependencies
P2-T04

### Files to Create
```
docker/postgres/init.sql
docker/postgres/rls-policies.sql
```

### Instructions

1. Extensions: uuid-ossp, pgcrypto

2. RLS template:
   ```sql
   ALTER TABLE tenant_users ENABLE ROW LEVEL SECURITY;
   CREATE POLICY tenant_isolation ON tenant_users
       USING (tenant_id = current_setting('app.current_tenant_id')::uuid);
   ```

3. Apply to: tenant_users, roles, user_roles, api_keys, notifications

### Tests
- [ ] Integration: Raw SQL without SET app.current_tenant_id returns zero rows

### Acceptance Criteria
RLS as defense-in-depth. Even SQL bypassing EF cannot cross tenants.

---

## P2-T07: Configure MassTransit with RabbitMQ

### Dependencies
P1-T03

### Files to Create
```
src/SaasKit.Infrastructure/Messaging/MassTransitConfiguration.cs
src/SaasKit.Infrastructure/Messaging/Filters/TenantContextPublishFilter.cs
```

### Instructions

1. AddMessaging() extension:
   - MassTransit + RabbitMQ
   - Register consumers from module assemblies
   - Retry: 3 immediate, then _error queue
   - SystemTextJson serialization

2. TenantContextPublishFilter: Add TenantId header to outgoing events

3. Test harness: AddMassTransitTestHarness() for in-memory tests

### Tests
- [ ] Integration: Publish event, consumer receives within 5s
- [ ] Unit: Publish filter adds tenant_id header

### Acceptance Criteria
Domain events flow between modules via MassTransit.

---

## P2-T08: Configure Redis distributed cache

### Dependencies
P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Caching/RedisConfiguration.cs
src/SaasKit.Infrastructure/Caching/IRedisCacheService.cs
src/SaasKit.Infrastructure/Caching/RedisCacheService.cs
```

### Instructions

1. Register IConnectionMultiplexer singleton
2. Register IDistributedCache
3. IRedisCacheService: Get, Set, Remove, GetOrSet with TTL
4. Key convention: `saaskit:{tenant_id}:{module}:{entity}:{id}`

### Tests
- [ ] Integration: Set/Get round-trip
- [ ] TTL expiry works

### Acceptance Criteria
Redis available for caching, rate limiting, SSE backplane.

---

## P2-T09: Create SSE infrastructure

### Dependencies
P2-T08

### Files to Create
```
src/SaasKit.Infrastructure/Sse/ISseConnectionManager.cs
src/SaasKit.Infrastructure/Sse/SseConnectionManager.cs
src/SaasKit.Infrastructure/Sse/SseConnection.cs
src/SaasKit.Infrastructure/Sse/ISseNotifier.cs
src/SaasKit.Infrastructure/Sse/SseNotifier.cs
```

### Instructions

1. **SseConnection**: HttpResponse ref, UserId, TenantId, WriteEventAsync
2. **SseConnectionManager**: ConcurrentDictionary by userId, Redis Pub/Sub for multi-instance
3. **ISseNotifier**: SendToUserAsync, SendToTenantAsync

### Tests
- [ ] Unit: Manager tracks add/remove correctly
- [ ] Integration: SendToTenant delivers to all tenant connections

### Acceptance Criteria
Modules call ISseNotifier to push real-time events.

---

## P2-T10: Create SSE HTTP endpoint

### Dependencies
P2-T09

### Files to Create
```
src/SaasKit.Api/Endpoints/SseEndpoint.cs
```

### Instructions

1. GET /api/v1/events/stream [Authorize]
2. Content-Type: text/event-stream
3. Extract userId/tenantId from claims
4. Register SseConnection, send 'connected' event
5. Keepalive every 30s, cleanup on disconnect

### Tests
- [ ] Integration: Authenticated request returns text/event-stream
- [ ] Integration: Receives 'connected' event first

### Acceptance Criteria
Clients connect to SSE endpoint. Events pushed via ISseNotifier arrive.

---

## P2-T11: Configure MinIO, Meilisearch, Hangfire

### Dependencies
P1-T02, P2-T04

### Files to Create
```
src/SaasKit.Infrastructure/Storage/IFileStorageService.cs
src/SaasKit.Infrastructure/Storage/MinioFileStorageService.cs
src/SaasKit.Infrastructure/Search/ISearchService.cs
src/SaasKit.Infrastructure/Search/MeilisearchService.cs
src/SaasKit.Infrastructure/Jobs/HangfireConfiguration.cs
```

### Instructions

1. **IFileStorageService**: Upload, GetPresignedUrl, Delete (per-tenant bucket)
2. **ISearchService**: IndexDocument, Search, Delete, ConfigureIndex
3. **Hangfire**: PostgreSQL storage, /hangfire dashboard (platform-admin only)

### Tests
- [ ] Integration: Upload to MinIO, get presigned URL, download
- [ ] Integration: Index and search in Meilisearch

### Acceptance Criteria
File storage, search, and background jobs available.

---

## P2-T12: Create Keycloak Admin API client

### Dependencies
P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Keycloak/IKeycloakAdminClient.cs
src/SaasKit.Infrastructure/Keycloak/KeycloakAdminClient.cs
src/SaasKit.Infrastructure/Keycloak/KeycloakConfiguration.cs
src/SaasKit.Infrastructure/Keycloak/Models/
```

### Instructions

1. IKeycloakAdminClient: GetUser, CreateUser, UpdateUser, DeleteUser, SendVerificationEmail, ResetPassword, AssignRealmRole
2. HttpClient-based, authenticates with saas-api client credentials
3. Token cached for lifetime

### Tests
- [ ] Integration: CreateUser, GetUser, DeleteUser round-trip

### Acceptance Criteria
Identity module can manage Keycloak users programmatically.

---

## P2-T13: Create Infrastructure ServiceRegistration

### Dependencies
P2-T01 through P2-T12

### Files to Create
```
src/SaasKit.Infrastructure/InfrastructureServiceRegistration.cs
```

### Instructions

1. AddInfrastructure(IServiceCollection, IConfiguration): calls all infra registrations
2. Registers model binder provider, response wrapping, exception handler, validation filter
3. Update Program.cs:
   ```csharp
   builder.Services.AddInfrastructure(builder.Configuration);
   app.UseMiddleware<ApiResponseWrappingMiddleware>();
   ```

### Tests
- [ ] Build and run
- [ ] All infrastructure services resolve
- [ ] Envelope wrapping works on /health

### Acceptance Criteria
Program.cs has single AddInfrastructure() call. All infra wired.

---

## Phase 2 Checklist

- [ ] P2-T01: Filter model binder and sort parser
- [ ] P2-T02: IQueryable filter/sort extensions
- [ ] P2-T03: Response envelope wrapper and error handler
- [ ] P2-T04: AppDbContext with multi-tenancy
- [ ] P2-T05: EF Core interceptors
- [ ] P2-T06: PostgreSQL RLS policies
- [ ] P2-T07: MassTransit with RabbitMQ
- [ ] P2-T08: Redis distributed cache
- [ ] P2-T09: SSE infrastructure
- [ ] P2-T10: SSE HTTP endpoint
- [ ] P2-T11: MinIO, Meilisearch, Hangfire
- [ ] P2-T12: Keycloak Admin API client
- [ ] P2-T13: Infrastructure ServiceRegistration
- [ ] All integration tests pass
