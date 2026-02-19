# Phases 5-8: Core Modules

Authorization, Notification, Subscription, and Portal modules.

---

# Phase 5: Authorization Module

Layered RBAC: Keycloak realm roles + app DB tenant roles and permissions.

## P5-T01: Create Authorization entities (7 entities)

### Files to Create
```
src/Modules/Authorization/Authorization.Contracts/*
src/Modules/Authorization/Authorization.Core/Entities/
  - Role.cs (TenantScopedEntity: Name, Description, IsDefault, IsSystem)
  - Permission.cs (BaseEntity: Name unique, Description, Module)
  - RolePermission.cs
  - UserRole.cs
  - Group.cs
  - GroupUser.cs
  - TenantUserRole.cs
src/Modules/Authorization/Authorization.Core/Persistence/*.cs
src/Modules/Authorization/Authorization.Core/Seeds/PermissionSeeder.cs
```

### Instructions

- Role: TenantScopedEntity with Name, Description, IsDefault, IsSystem
- Permission: BaseEntity (global), Name (unique like 'tenancy.manage'), Module
- Seed default permissions on startup via PermissionSeeder
- ITenantService.GetRolesAsync(tenantId, QueryParameters) supports filter[isDefault]=true, filter[name][contains]=admin

### Tests
- [ ] Integration: Seed data creates default permissions

---

## P5-T02: Implement AuthorizationService and policy handler

### Files to Create
```
src/Modules/Authorization/Authorization.Core/Services/AuthorizationModuleService.cs
src/Modules/Authorization/Authorization.Core/Consumers/TenantCreatedConsumer.cs
src/SaasKit.Infrastructure/Auth/PermissionRequirement.cs
src/SaasKit.Infrastructure/Auth/PermissionAuthorizationHandler.cs
src/SaasKit.Infrastructure/Auth/HasPermissionAttribute.cs
```

### Instructions

1. **TenantCreatedConsumer**: Seed 3 default roles with permissions, assign owner to creator

2. **GetUserPermissionsAsync**: Cache in Redis 5 min

3. **[HasPermission("tenancy.manage")]**: PermissionAuthorizationHandler checks via IAuthorizationModuleService

### Tests
- [ ] Unit: TenantCreatedConsumer seeds roles
- [ ] Integration: [HasPermission] returns 403 for unpermissioned user

---

## P5-T03: Create Authorization API and ServiceRegistration

### Files to Create
```
src/SaasKit.Api/Controllers/RolesController.cs
src/SaasKit.Api/Controllers/PermissionsController.cs
src/Modules/Authorization/Authorization.Core/AuthorizationServiceRegistration.cs
```

### Endpoints

- `GET /api/v1/roles?filter[isDefault]=true` - supports filters
- `GET /api/v1/permissions?filter[module]=tenancy` - filter by module

### Tests
- [ ] Integration: Full flow - create tenant > roles seeded > assign permission > call protected endpoint

---

# Phase 6: Notification Module

SSE push infrastructure, notification persistence, email dispatch.

## P6-T01: Implement Notification module

### Files to Create
```
src/Modules/Notification/Notification.Contracts/*
src/Modules/Notification/Notification.Core/Entities/Notification.cs
src/Modules/Notification/Notification.Core/Services/NotificationService.cs
src/SaasKit.Api/Controllers/NotificationsController.cs
```

### Entity

```csharp
public class Notification : TenantScopedEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string Type { get; set; }  // info, warning, success, error
    public string? Link { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTimeOffset? ReadAt { get; set; }
}
```

### Endpoints

- `GET /api/v1/notifications?filter[isRead]=false&filter[type]=info&filter[type]=warning&sort=-createdAt`
- `POST /api/v1/notifications/{id}/read`
- `GET /api/v1/notifications/unread-count`

On create, push via ISseNotifier.SendToUserAsync('notification.new', dto)

### Tests
- [ ] Unit: CreateAsync calls ISseNotifier
- [ ] Integration: GET with filter[type]=info&filter[type]=warning returns both types

---

# Phase 7: Subscription Module

Plans, pricing, Stripe, feature gates, credits, usage tracking.

## P7-T01: Create Subscription entities (10 entities)

### Files to Create
```
src/Modules/Subscription/Subscription.Contracts/*
src/Modules/Subscription/Subscription.Core/Entities/
  - Plan.cs
  - PlanPrice.cs
  - PlanFeature.cs
  - PlanUsageBasedPrice.cs
  - TenantSubscription.cs
  - TenantSubscriptionProduct.cs
  - TenantSubscriptionPrice.cs
  - TenantUsageRecord.cs
  - CheckoutSessionStatus.cs
  - Credit.cs
src/Modules/Subscription/Subscription.Core/Persistence/*.cs
src/Modules/Subscription/Subscription.Core/Seeds/PlanSeeder.cs
```

### Instructions

- Seed 3 default plans: Free, Pro, Enterprise
- GET /api/v1/plans supports filter[isActive]=true
- GET /api/v1/credits/history supports filter[type]=purchase&filter[type]=bonus&filter[createdAt][gte]=2026-01-01

### Tests
- [ ] Integration: Seed data creates plans with prices and features

---

## P7-T02: Implement StripePaymentGateway and services

### Files to Create
```
src/Modules/Subscription/Subscription.Core/Services/SubscriptionService.cs
src/Modules/Subscription/Subscription.Core/Services/FeatureGateService.cs
src/Modules/Subscription/Subscription.Core/Services/CreditService.cs
src/Modules/Subscription/Subscription.Core/Gateways/IPaymentGateway.cs
src/Modules/Subscription/Subscription.Core/Gateways/StripePaymentGateway.cs
```

### Instructions

1. **StripePaymentGateway**: Stripe.net SDK
2. **Webhook handler**: checkout.session.completed, invoice.paid, customer.subscription.updated/deleted
3. **FeatureGateService**: Check tenant plan features, return FeatureGateResult
4. **CreditService**: Append-only ledger, Redis balance cache

### Tests
- [ ] Unit: FeatureGateService returns Denied when limit exceeded
- [ ] Unit: CreditService fails on insufficient balance

---

## P7-T03: Create Subscription API and ServiceRegistration

### Files to Create
```
src/SaasKit.Api/Controllers/PlansController.cs
src/SaasKit.Api/Controllers/SubscriptionsController.cs
src/SaasKit.Api/Controllers/CreditsController.cs
src/SaasKit.Api/Controllers/StripeWebhookController.cs
src/Modules/Subscription/Subscription.Core/SubscriptionServiceRegistration.cs
```

### Endpoints

- `GET /api/v1/plans?filter[isActive]=true` - public
- `GET /api/v1/credits/history?filter[type]=spend&filter[type]=purchase&sort=-createdAt&page=1&pageSize=50`
- `POST /api/v1/webhooks/stripe` - anonymous, validates Stripe signature

### Tests
- [ ] Integration: GET /api/v1/plans returns seeded plans in envelope
- [ ] Integration: Credit history with array filters works

---

# Phase 8: Portal Module

B2B2C portal system. Each tenant creates portals with branding, subdomain, portal users.

## P8-T01: Create Portal entities (10 entities)

### Files to Create
```
src/Modules/Portal/Portal.Contracts/*
src/Modules/Portal/Portal.Core/Entities/
  - Portal.cs (TenantScopedEntity: Subdomain unique, branding, SEO, auth config)
  - PortalUser.cs (TenantScopedEntity + PortalId)
  - PortalPage.cs
  - PortalUserRegistration.cs
  - PortalSubscription.cs
  - PortalTheme.cs
  - PortalDomain.cs
  - PortalSettings.cs
  - PortalInvitation.cs
  - PortalApiKey.cs
```

### Instructions

- Portal: TenantScopedEntity with Subdomain (unique), branding fields, SEO, auth config, StripeAccountId
- PortalUser: TenantScopedEntity + PortalId, global query filter adds PortalId check

### Tests
- [ ] Integration: Create portal, create portal user, verify PortalId isolation

---

## P8-T02: Implement Portal services and resolution middleware

### Files to Create
```
src/Modules/Portal/Portal.Core/Services/PortalService.cs
src/Modules/Portal/Portal.Core/Services/PortalUserService.cs
src/SaasKit.Infrastructure/Tenancy/PortalResolutionMiddleware.cs
src/SaasKit.Infrastructure/Tenancy/IPortalContext.cs
src/SaasKit.Infrastructure/Tenancy/PortalContext.cs
```

### Instructions

- PortalService: CRUD, branding, domain mapping. ListAsync with filter[isActive]=true
- PortalUserService: Registration, profile. ListAsync with filter[email][contains]=...
- PortalResolutionMiddleware: Resolve PortalId from subdomain/custom domain

### Tests
- [ ] Unit: PortalService validates subdomain uniqueness (returns 409 Conflict)

---

## P8-T03: Create Portal API and ServiceRegistration

### Files to Create
```
src/SaasKit.Api/Controllers/PortalsController.cs
src/SaasKit.Api/Controllers/PortalUsersController.cs
src/SaasKit.Api/Controllers/PortalPagesController.cs
src/Modules/Portal/Portal.Core/PortalServiceRegistration.cs
```

### Endpoints

- Tenant admin: `/api/v1/portals` (CRUD with filters)
- Portal user: `/portal/v1/` (separate namespace)
- `GET /api/v1/portals?filter[isActive]=true&filter[isActive]=false` (array filter)
- Duplicate subdomain POST returns 409 ApiError.Conflict

### Tests
- [ ] Integration: Create portal, configure branding, verify portal API returns branded data

---

## Phases 5-8 Checklist

### Phase 5: Authorization
- [ ] P5-T01: Authorization entities (7)
- [ ] P5-T02: AuthorizationService and handlers
- [ ] P5-T03: Authorization API

### Phase 6: Notification
- [ ] P6-T01: Notification module complete

### Phase 7: Subscription
- [ ] P7-T01: Subscription entities (10)
- [ ] P7-T02: Stripe and services
- [ ] P7-T03: Subscription API

### Phase 8: Portal
- [ ] P8-T01: Portal entities (10)
- [ ] P8-T02: Portal services and middleware
- [ ] P8-T03: Portal API
