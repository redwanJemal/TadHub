# TadHub Backoffice App - Task Plan

**Last Updated:** February 20, 2026  
**Status:** 🔴 CRITICAL PATH - Blocking tenant-app development

---

## Why Backoffice First?

TadHub uses a **B2B model** where:
1. Admin creates tenants (agencies)
2. Admin adds users to tenants
3. Users can then access tenant-app with proper context

Without backoffice, users log in but have no tenants → 403 on all API calls.

---

## Tech Stack

- **Framework:** React + Vite (separate from tenant-app)
- **UI:** shadcn/ui + TailwindCSS
- **State:** React Query + Zustand
- **Auth:** Keycloak OIDC (same realm, different client: `backoffice-app`)
- **API:** Same backend (`api.endlessmaker.com/api/v1`)

---

## Phase 1: MVP (Unblock tenant-app) — Week 1

### 1.1 Project Setup
- [ ] Create backoffice-app project with Vite
- [ ] Configure TailwindCSS + shadcn/ui
- [ ] Set up Keycloak OIDC (client: `backoffice-app`)
- [ ] Configure API client with auth interceptor
- [ ] Basic layout (sidebar, header, content area)

### 1.2 Authentication
- [ ] Login redirect to Keycloak
- [ ] Callback handler
- [ ] Admin role check (platform-admin or tenant-admin)
- [ ] Logout flow
- [ ] Session persistence

### 1.3 Tenant Management (Critical)
- [ ] Tenants list page (table with pagination)
- [ ] Create tenant form
  - [ ] Name (required)
  - [ ] Slug (auto-generate or manual)
  - [ ] Description
  - [ ] Logo URL
  - [ ] Website
- [ ] Tenant detail view
- [ ] Edit tenant form
- [ ] Suspend/Reactivate actions
- [ ] Delete tenant (with confirmation)

### 1.4 User-Tenant Association (Critical)
- [ ] Users list page (all platform users)
- [ ] Search users by email/name
- [ ] Add user to tenant modal
  - [ ] Select user
  - [ ] Select tenant
  - [ ] Select role (Member/Admin/Owner)
- [ ] View user's tenant memberships
- [ ] Remove user from tenant
- [ ] Change user role in tenant

### 1.5 Role Management (Critical)
- [ ] List roles for a tenant
- [ ] Create role form
  - [ ] Role name
  - [ ] Description
  - [ ] Select permissions (grouped by module)
- [ ] Edit role permissions
- [ ] Assign role to user in tenant
- [ ] View user roles

---

## Phase 2: Complete User Management — Week 2

### 2.1 User CRUD
- [ ] User detail page
- [ ] Create user form (for admin)
- [ ] Edit user profile
- [ ] Deactivate/Reactivate user
- [ ] User login history

### 2.2 Invitations
- [ ] List pending invitations
- [ ] Send invitation (email + role)
- [ ] Revoke invitation
- [ ] Track invitation status

### 2.3 Audit & Security
- [ ] User activity logs
- [ ] Tenant audit events
- [ ] Session management

---

## Phase 3: Platform Administration — Week 3

### 3.1 Audit Logs
- [ ] Audit events list (filterable)
- [ ] Entity change logs
- [ ] Export audit data

### 3.2 Feature Flags
- [ ] Feature flags list
- [ ] Create/edit feature flag
- [ ] Toggle enabled/disabled
- [ ] Percentage rollout

### 3.3 Plans & Subscriptions
- [ ] Plans list
- [ ] Create/edit plan
- [ ] Plan features management
- [ ] Active subscriptions overview

---

## API Endpoints Used

### Tenants
```
GET    /tenants                    # List all tenants (admin)
POST   /tenants                    # Create tenant
GET    /tenants/{id}               # Get tenant
PATCH  /tenants/{id}               # Update tenant
DELETE /tenants/{id}               # Delete tenant
POST   /tenants/{id}/suspend       # Suspend tenant
POST   /tenants/{id}/reactivate    # Reactivate tenant
```

### Tenant Members
```
GET    /tenants/{id}/members           # List members
POST   /tenants/{id}/members           # Add member (need to create this?)
PATCH  /tenants/{id}/members/{userId}  # Update role
DELETE /tenants/{id}/members/{userId}  # Remove member
```

### Users
```
GET    /users                      # List all users (admin)
GET    /users/{id}                 # Get user
POST   /users                      # Create user (admin)
PATCH  /users/{id}                 # Update user
POST   /users/{id}/deactivate      # Deactivate
POST   /users/{id}/reactivate      # Reactivate
```

### Roles
```
GET    /tenants/{id}/roles             # List roles
POST   /tenants/{id}/roles             # Create role
PATCH  /tenants/{id}/roles/{roleId}    # Update role
DELETE /tenants/{id}/roles/{roleId}    # Delete role
POST   /tenants/{id}/roles/assign      # Assign role to user
DELETE /tenants/{id}/roles/users/{userId}/roles/{roleId}  # Remove role
```

### Permissions
```
GET    /permissions                # List all permissions
```

---

## Missing API Endpoints

Based on the flow, we might need:

1. **Add user to tenant** - Not clearly documented
   - Option A: `POST /tenants/{id}/members` with `{ userId, role }`
   - Option B: Use invitations flow
   - **Need to verify API implementation**

2. **List all tenants (admin)** - Need `platform-admin` role check
   - Current `/tenants` returns user's tenants only
   - May need admin override parameter

3. **Search users globally** - For "add user to tenant" UI
   - `GET /users?search=email`

---

## Deployment

- **Domain:** `admin.endlessmaker.com` (already configured in Keycloak)
- **Build:** Same as tenant-app (`npm run build`)
- **Deploy:** Coolify or similar

---

## Quick Start Commands

```bash
# Create project
npm create vite@latest backoffice-app -- --template react-ts
cd backoffice-app

# Install dependencies
npm install @tanstack/react-query react-oidc-context oidc-client-ts zustand
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p

# Add shadcn/ui
npx shadcn@latest init
npx shadcn@latest add button card input table dialog form

# Start dev
npm run dev
```

---

## Notes

- Backoffice requires `platform-admin` or equivalent role in Keycloak
- All tenant-scoped operations need `X-Tenant-Id` header
- Use same API client pattern as tenant-app
- Consider sharing UI components between apps (monorepo later?)
