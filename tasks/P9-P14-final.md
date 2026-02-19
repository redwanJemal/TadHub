# Phases 9-14: Parallel Modules + Integration

API Management, Feature Flags, Audit, Analytics, Content (can run in parallel), then Template + Integration + CI.

---

# Phase 9: API Management Module

## P9-T01: Implement API Management module

### Dependencies
P4-T04, P5-T03

### Files to Create
```
src/Modules/ApiManagement/ApiManagement.Contracts/*
src/Modules/ApiManagement/ApiManagement.Core/Entities/
  - ApiKey.cs (TenantScopedEntity: KeyHash, Prefix, Name, Permissions JSONB, ExpiresAt, IsActive, LastUsedAt)
  - ApiKeyLog.cs (TenantScopedEntity: ApiKeyId, Endpoint, Method, StatusCode, Duration, IpAddress, Timestamp)
src/Modules/ApiManagement/ApiManagement.Core/Services/ApiKeyService.cs
src/Modules/ApiManagement/ApiManagement.Core/Auth/ApiKeyAuthenticationHandler.cs
src/Modules/ApiManagement/ApiManagement.Core/RateLimiting/ApiKeyRateLimiter.cs
src/SaasKit.Api/Controllers/ApiKeysController.cs
```

### Endpoints

- `GET /api/v1/api-keys?filter[isActive]=true&sort=-lastUsedAt`
- `GET /api/v1/api-keys/{id}/logs?filter[statusCode][gte]=400&filter[createdAt][gte]=2026-01-01` - error logs from Jan 2026
- `POST /api/v1/api-keys` - create new key
- `DELETE /api/v1/api-keys/{id}` - revoke key

### Implementation

1. **ApiKeyAuthenticationHandler**: Read X-API-Key header, hash, lookup
2. **Rate limiting**: Redis sliding window. 429 response with Retry-After header and ApiError format
3. **Headers on response**:
   ```
   X-RateLimit-Limit: 1000
   X-RateLimit-Remaining: 997
   X-RateLimit-Reset: 1740400200
   ```

### Tests
- [ ] Unit: Rate limiter returns 429 with Retry-After
- [ ] Integration: API key log filtering with range operators works

### Acceptance Criteria
API keys work. Rate limiting enforced. Logs queryable.

---

# Phase 10: Feature Flags Module

## P10-T01: Implement Feature Flags module

### Dependencies
P4-T04, P7-T03

### Files to Create
```
src/Modules/FeatureFlags/FeatureFlags.Contracts/*
src/Modules/FeatureFlags/FeatureFlags.Core/Entities/
  - FeatureFlag.cs (Name unique, IsEnabled, Percentage)
  - FeatureFlagFilter.cs (Type: TenantType/Plan/Percentage, Value)
src/Modules/FeatureFlags/FeatureFlags.Core/Services/FeatureFlagService.cs
src/SaasKit.Api/Controllers/FeatureFlagsController.cs
```

### Endpoints

- `GET /api/v1/feature-flags?filter[isEnabled]=true`
- `POST /api/v1/feature-flags` - create flag
- `PUT /api/v1/feature-flags/{id}` - update flag
- `GET /api/v1/feature-flags/{name}/evaluate` - check if flag is enabled for current tenant

### Implementation

1. Evaluation: plan match, tenant type match, percentage hash
2. On flag change, push SSE 'feature_flag.changed'
3. Cache in Redis 1 min

### Tests
- [ ] Unit: Percentage rollout deterministic
- [ ] Unit: Plan filter matches

### Acceptance Criteria
Feature flags with caching and SSE push.

---

# Phase 11: Audit Module

## P11-T01: Implement Audit module

### Dependencies
P2-T07, P2-T11

### Files to Create
```
src/Modules/Audit/Audit.Contracts/*
src/Modules/Audit/Audit.Core/Entities/
  - AuditEvent.cs (EventName, Payload JSONB, Metadata)
  - AuditLog.cs (Action, EntityType, EntityId, OldValues, NewValues)
  - Webhook.cs (Url, Events, Secret, IsActive)
  - WebhookDelivery.cs (WebhookId, Payload, StatusCode, Attempts)
  - WebhookDeliveryAttempt.cs
src/Modules/Audit/Audit.Core/Consumers/DomainEventAuditConsumer.cs
src/Modules/Audit/Audit.Core/Services/WebhookDeliveryService.cs
src/SaasKit.Api/Controllers/AuditController.cs
src/SaasKit.Api/Controllers/WebhooksController.cs
```

### Endpoints

- `GET /api/v1/audit/events?filter[name]=TenantCreatedEvent&filter[name]=UserCreatedEvent&filter[createdAt][gte]=2026-01-01&filter[createdAt][lte]=2026-01-31&sort=-createdAt`
- `GET /api/v1/audit/logs?filter[action]=update&filter[entityType]=Tenant`
- `GET /api/v1/webhooks` - list webhooks
- `POST /api/v1/webhooks` - create webhook
- pageSize max override to 200 for audit endpoints

### Implementation

1. **DomainEventAuditConsumer**: IConsumer<IDomainEvent>. Records all events.
2. **WebhookDeliveryService**: Hangfire retry 3x with backoff. Log each attempt.

### Tests
- [ ] Unit: Consumer records any domain event
- [ ] Integration: Audit events filterable by name array and date range

### Acceptance Criteria
All domain events recorded. Queryable with rich filters.

---

# Phase 12: Analytics Module

## P12-T01: Implement Analytics module

### Dependencies
P2-T08, P2-T11

### Files to Create
```
src/Modules/Analytics/Analytics.Contracts/*
src/Modules/Analytics/Analytics.Core/Entities/
  - PageView.cs (TenantScopedEntity: Url, UserId?, SessionId, Referrer, UserAgent)
  - AnalyticsEvent.cs (TenantScopedEntity: Name, Properties JSONB)
  - Session.cs
  - DailyStats.cs (pre-aggregated)
src/Modules/Analytics/Analytics.Core/Services/AnalyticsService.cs
src/Modules/Analytics/Analytics.Core/Jobs/AnalyticsBatchInsertJob.cs
src/SaasKit.Api/Controllers/AnalyticsController.cs
```

### Endpoints

- `GET /api/v1/analytics/pageviews?filter[url][contains]=/pricing&filter[createdAt][gte]=2026-01-01&sort=-createdAt`
- `GET /api/v1/analytics/events?filter[name]=signup&filter[name]=checkout` - array filter on event names
- `POST /api/v1/analytics/track` - track event (batched)

### Implementation

1. Buffered writes: Redis list, Hangfire batch insert every 30s
2. Pre-aggregate daily stats for dashboards

### Tests
- [ ] Integration: Track events, batch insert, query with filters

### Acceptance Criteria
Analytics with buffered writes and filtered queries.

---

# Phase 13: Content Module

## P13-T01: Implement Content module (16 entities)

### Dependencies
P2-T04, P2-T11

### Files to Create
```
src/Modules/Content/Content.Contracts/*
src/Modules/Content/Content.Core/Entities/
  - BlogPost.cs
  - BlogCategory.cs
  - BlogTag.cs
  - BlogPostTag.cs
  - KnowledgeBase.cs
  - KBCategory.cs
  - KBArticle.cs
  - KBArticleVersion.cs
  - Page.cs
  - PageBlock.cs
  - PageVersion.cs
  - Media.cs
  - MediaFolder.cs
  - Template.cs
  - ContentTranslation.cs
  - ContentRevision.cs
src/Modules/Content/Content.Core/Services/
  - BlogService.cs
  - KnowledgeBaseService.cs
  - PageService.cs
src/Modules/Content/Content.Core/Jobs/MeilisearchIndexJob.cs
src/SaasKit.Api/Controllers/BlogController.cs
src/SaasKit.Api/Controllers/KnowledgeBaseController.cs
src/SaasKit.Api/Controllers/PagesController.cs
```

### Endpoints

- `GET /api/v1/blog/posts?filter[status]=published&filter[status]=draft&filter[categoryId]=abc&sort=-publishedAt&search=kubernetes`
- `GET /api/v1/blog/posts?include=category,tags` - eager loading
- `GET /api/v1/kb/articles?filter[language]=en&filter[status]=published&filter[categoryId][isNull]=false`
- `POST /api/v1/blog/posts` - create post
- `PUT /api/v1/blog/posts/{id}/publish` - publish post

### Implementation

1. On publish, index to Meilisearch via Hangfire
2. `search` parameter uses Meilisearch for full-text search
3. Support `include` for eager loading relations

### Tests
- [ ] Unit: Publish triggers Meilisearch index job
- [ ] Integration: Blog posts filterable by status array, searchable

### Acceptance Criteria
Content module with search, filtering, includes.

---

# Phase 14: Template + Integration + CI

## P14-T01: Create _Template module

### Dependencies
All previous phases

### Files to Create
```
src/Modules/_Template/Template.Contracts/ITemplateService.cs
src/Modules/_Template/Template.Contracts/DTOs/*.cs
src/Modules/_Template/Template.Core/Entities/TemplateEntity.cs
src/Modules/_Template/Template.Core/Services/TemplateService.cs
src/Modules/_Template/Template.Core/Consumers/TemplateEventConsumer.cs
src/Modules/_Template/Template.Core/Validators/CreateTemplateRequestValidator.cs
src/Modules/_Template/Template.Core/TemplateServiceRegistration.cs
src/Modules/_Template/README.md
```

### Instructions

1. Fully working minimal module demonstrating:
   - Entity
   - Service with filter/sort via ApplyFilters/ApplySort
   - MassTransit consumer
   - FluentValidation
   - DI registration

2. Controller returns raw DTOs/PagedList (middleware wraps in envelope)

3. README: step-by-step instructions to copy and customize for a new bounded context

4. Documents which fields are filterable, sortable, includable

### Tests
- [ ] Template module builds and all patterns demonstrated

### Acceptance Criteria
New developers create a domain module in under 30 minutes.

---

## P14-T02: Write end-to-end integration tests

### Dependencies
All previous phases

### Files to Create
```
tests/SaasKit.Tests.Integration/
  - SaasKitWebApplicationFactory.cs
  - Fixtures/
  - TenantLifecycleTests.cs
  - MultiTenancyIsolationTests.cs
  - ApiConventionTests.cs
  - PortalFlowTests.cs
  - SubscriptionFlowTests.cs
```

### Instructions

1. **SaasKitWebApplicationFactory**: Testcontainers for PostgreSQL/Redis, MassTransit in-memory

2. **TenantLifecycleTests**: Create > invite > accept > filter members > verify

3. **MultiTenancyIsolationTests**: Two tenants, verify filter returns only scoped data

4. **ApiConventionTests**:
   - Verify every list endpoint returns { data, meta, pagination } shape
   - Verify 422 returns RFC 9457 with errors dict
   - Verify filter[x]=a&filter[x]=b returns union
   - Verify unknown filter is ignored
   - Verify invalid sort field returns 400

5. **PortalFlowTests**: Create portal > register user > create page > verify portal API

### Tests
- [ ] All integration tests pass in CI

### Acceptance Criteria
Full system validated including API conventions.

---

## P14-T03: Create GitHub Actions CI pipeline

### Dependencies
P14-T02

### Files to Create
```
.github/workflows/ci.yml
```

### Instructions

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:17
        env:
          POSTGRES_USER: saaskit
          POSTGRES_PASSWORD: saaskit_test
          POSTGRES_DB: saaskit_test
        ports:
          - 5432:5432
      redis:
        image: redis:7
        ports:
          - 6379:6379

    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Format Check
        run: dotnet format --verify-no-changes
      
      - name: Unit Tests
        run: dotnet test --no-build --filter "Category=Unit"
      
      - name: Integration Tests
        run: dotnet test --no-build --filter "Category=Integration"
```

### Tests
- [ ] CI passes on push
- [ ] All tests green

### Acceptance Criteria
Every PR validated automatically.

---

## Phases 9-14 Checklist

### Phase 9: API Management
- [ ] P9-T01: API Management module complete

### Phase 10: Feature Flags
- [ ] P10-T01: Feature Flags module complete

### Phase 11: Audit
- [ ] P11-T01: Audit module complete

### Phase 12: Analytics
- [ ] P12-T01: Analytics module complete

### Phase 13: Content
- [ ] P13-T01: Content module complete (16 entities)

### Phase 14: Template + Integration + CI
- [ ] P14-T01: _Template module
- [ ] P14-T02: Integration tests
- [ ] P14-T03: GitHub Actions CI

---

## 🎉 Project Complete Checklist

When all phases are done:

- [ ] 55 tasks completed
- [ ] ~71 entities created
- [ ] All endpoints follow API conventions (envelope, filters, sorting, pagination, RFC 9457 errors)
- [ ] Full test coverage (unit + integration)
- [ ] CI pipeline green
- [ ] Documentation complete
- [ ] _Template module ready for new bounded contexts
