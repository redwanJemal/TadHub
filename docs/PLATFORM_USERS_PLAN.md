# Platform Users Architecture Plan

**Date:** February 20, 2026  
**Status:** Planning

---

## Current State (❌ Wrong)

The current implementation conflates two different user pools:

```
Backoffice "Users" page → Shows ALL UserProfile records
                         (every user who ever logged in)
```

This is problematic because:
- Shows 1000s of tenant users to platform admins
- No clear distinction between platform team and regular users
- Authorization is confusing (same endpoints for both)

---

## Recommended Architecture (✅ Right)

### Three User Types

```
┌─────────────────────────────────────────────────────────────┐
│                     UserProfile                              │
│  (Global entity - synced from Keycloak for ALL users)       │
│  - keycloakId, email, firstName, lastName                   │
│  - isActive, lastLoginAt, locale                            │
└─────────────────────────────────────────────────────────────┘
           │                              │
           ▼                              ▼
┌─────────────────────┐        ┌─────────────────────────────┐
│     AdminUser       │        │       TenantUser             │
│  (Platform Team)    │        │  (Agency Staff Membership)   │
│  - 5-20 people      │        │  - Belongs to a tenant       │
│  - IsSuperAdmin     │        │  - Role: owner/admin/member  │
│  - Operates SaaS    │        │  - Many per user (multi-org) │
└─────────────────────┘        └─────────────────────────────┘
```

### Backoffice Navigation

```
Backoffice App (admin.endlessmaker.com)
├── Dashboard
├── Tenants
│   └── [Tenant Detail]
│       ├── Overview
│       ├── Members        ← View TenantUsers (read-only)
│       ├── Settings
│       └── Subscription
├── Platform Team          ← Shows AdminUsers ONLY (5-20 people)
│   ├── List admins
│   ├── Add admin (link UserProfile → AdminUser)
│   └── Manage permissions
├── Audit Logs
└── Settings
```

### API Endpoints

**Platform Admin Endpoints (Backoffice):**
```
GET    /api/v1/admin/users              → List AdminUsers
POST   /api/v1/admin/users              → Create AdminUser
GET    /api/v1/admin/users/{id}         → Get AdminUser
PATCH  /api/v1/admin/users/{id}         → Update AdminUser
DELETE /api/v1/admin/users/{id}         → Remove admin status

GET    /api/v1/admin/tenants/{id}/members  → View tenant members (read-only)
```

**Tenant-Scoped Endpoints (Tenant App):**
```
GET    /api/v1/tenant/members           → List TenantUsers (scoped by X-Tenant-ID)
POST   /api/v1/tenant/members/invite    → Invite new member
PUT    /api/v1/tenant/members/{id}/role → Change member role
DELETE /api/v1/tenant/members/{id}      → Remove member
```

---

## Implementation Plan

### Phase 1: Backend Changes

#### 1.1 Create AdminUsersController
```csharp
[Route("api/v1/admin/users")]
[Authorize(Roles = "platform-admin")]
public class AdminUsersController
{
    GET  /           → List AdminUsers with nested UserProfile
    POST /           → Create AdminUser (find/create UserProfile, link)
    GET  /{id}       → Get AdminUser by ID
    PATCH /{id}      → Update (IsSuperAdmin, etc.)
    DELETE /{id}     → Remove admin status (keeps UserProfile)
}
```

#### 1.2 Create IAdminUserService
```csharp
public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> ListAsync(QueryParameters qp);
    Task<Result<AdminUserDto>> GetByIdAsync(Guid id);
    Task<Result<AdminUserDto>> CreateAsync(CreateAdminUserRequest request);
    Task<Result<AdminUserDto>> UpdateAsync(Guid id, UpdateAdminUserRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<bool> IsAdminAsync(Guid userId);
}
```

#### 1.3 Update Existing UsersController
- Keep `/api/v1/users/me` for current user
- Remove or restrict `/api/v1/users` list (platform-admin only, for searching users to add as admin)

### Phase 2: Frontend Changes

#### 2.1 Rename "Users" to "Platform Team"
- New sidebar item: "Platform Team" (icon: Shield)
- Shows AdminUser list with:
  - User info (name, email, avatar)
  - IsSuperAdmin badge
  - Last login
  - Actions: Edit, Remove

#### 2.2 Add Admin Form
- Search existing users by email/name
- Or create new user in Keycloak
- Toggle IsSuperAdmin
- Enforce MFA option

#### 2.3 Tenant Members Tab
- In Tenant Detail page, add "Members" tab
- Read-only view of TenantUsers
- Show: Name, Email, Role, Joined date
- Optional: Impersonation button for support

---

## Files to Create/Modify

### Backend

**New Files:**
```
src/TadHub.Api/Controllers/AdminUsersController.cs
src/Modules/Identity/Identity.Contracts/IAdminUserService.cs
src/Modules/Identity/Identity.Contracts/DTOs/CreateAdminUserRequest.cs
src/Modules/Identity/Identity.Contracts/DTOs/UpdateAdminUserRequest.cs
src/Modules/Identity/Identity.Core/Services/AdminUserService.cs
```

**Modify:**
```
src/TadHub.Api/Controllers/UsersController.cs  (restrict list endpoint)
src/Modules/Identity/Identity.Core/IdentityServiceRegistration.cs  (register new service)
```

### Frontend (Backoffice)

**New Files:**
```
web/backoffice-app/src/features/platform-team/
├── api.ts
├── hooks.ts
├── types.ts
├── pages/
│   └── PlatformTeamPage.tsx
└── components/
    ├── AdminUserList.tsx
    ├── AddAdminDialog.tsx
    └── AdminUserCard.tsx
```

**Modify:**
```
web/backoffice-app/src/app/routes.tsx  (add platform-team route)
web/backoffice-app/src/shared/components/Sidebar.tsx  (rename Users → Platform Team)
web/backoffice-app/src/features/tenants/pages/TenantDetailPage.tsx  (add Members tab)
```

---

## Authorization Matrix

| Endpoint | Role Required | Scope |
|----------|--------------|-------|
| `GET /admin/users` | platform-admin | Platform |
| `POST /admin/users` | platform-admin | Platform |
| `GET /admin/tenants/{id}/members` | platform-admin | Platform (read-only) |
| `GET /tenant/members` | tenant-admin | Tenant (via X-Tenant-ID) |
| `POST /tenant/members/invite` | tenant-admin | Tenant |

---

## Migration Steps

1. **Create AdminUser records** for existing platform admins
   ```sql
   INSERT INTO admin_users (id, user_id, is_super_admin, created_at, updated_at)
   SELECT gen_random_uuid(), id, true, NOW(), NOW()
   FROM user_profiles
   WHERE email IN ('admin@endlessmaker.com', ...);
   ```

2. **Update frontend** to use new endpoints
3. **Test authorization** - ensure tenant users can't access admin endpoints
4. **Deploy and verify**

---

## Success Criteria

- [ ] Backoffice "Platform Team" shows only AdminUsers (5-20 max)
- [ ] Tenant members visible via Tenants > {name} > Members tab
- [ ] Clear separation of authorization paths
- [ ] No confusion between platform admins and agency staff
