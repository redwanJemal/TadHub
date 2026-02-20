# TadHub Frontend Progress Tracker

**Last Updated:** February 20, 2026  
**Frontend Stack:** React/Vite, TypeScript, TailwindCSS, React Query, shadcn/ui

---

## Overview

This document tracks the frontend implementation progress for TadHub. Tasks are organized by phase and priority.

### Status Legend
- ⬜ Not Started
- 🟡 In Progress
- ✅ Complete
- 🔴 Blocked

### Current Blockers
> **🔴 BLOCKER:** Tenant-app requires users to have tenant associations before using. Users must be added to tenants via backoffice-app first. See Phase 3 priorities.

---

## Phase 0: Authentication (CRITICAL - Do First!)

> **Why Phase 0?** Nothing works without auth. All API calls require valid tokens and tenant context.

### Keycloak OIDC Integration
- ✅ Install `react-oidc-context` package
- ✅ Configure Keycloak provider settings
- ✅ Set up OIDC authority URL (Keycloak realm)
- ✅ Configure client ID for tenant-app
- ✅ Configure client ID for backoffice-app
- ✅ Set redirect URIs (login callback)
- ✅ Set post-logout redirect URI

### Token Management
- ✅ Access token extraction from OIDC context
- ✅ Automatic token refresh before expiry
- ✅ Token refresh error handling
- ✅ Silent refresh in background
- ✅ Token storage (localStorage with WebStorageStateStore)

### API Client Setup
- ✅ Create fetch instance with base URL
- ✅ Add Authorization header interceptor (Bearer token)
- ✅ Add X-Tenant-ID header interceptor
- ✅ 401 response handler (redirect to login)
- ✅ 403 response handler (permission denied UI)
- ✅ Network error handling
- ✅ Request retry logic

### Auth Context & Hooks
- ✅ AuthProvider wrapper component
- ✅ useAuth hook (isAuthenticated, user, token)
- ✅ useTenant hook (current tenant context)
- ✅ usePermissions hook (check user permissions)
- ✅ hasPermission utility function

### Protected Routes
- ✅ ProtectedRoute wrapper component
- ✅ Redirect to login if not authenticated
- ✅ Loading state while checking auth
- ✅ Permission-based route protection
- ✅ Role-based route protection

### Login Flow
- ✅ Login page with Keycloak redirect
- ✅ OAuth callback page handler
- ✅ Extract tokens from callback
- ✅ Fetch user profile after login (`/users/me`)
- 🔴 Fetch tenant list for user (blocked: requires backoffice tenant setup)
- 🔴 Tenant selection (if user has multiple) (blocked: no tenants to select)
- ✅ Set active tenant in context (X-Tenant-ID header)
- ✅ Redirect to dashboard after login

> **Note (Feb 20):** Auth flow works end-to-end (Keycloak → token → API). Blocking issue: `/me` returns `tenants: []` because users must be added to tenants via backoffice first. This is by design for B2B SaaS.

### Logout Flow
- ✅ Logout button in header/menu
- ✅ Clear local auth state
- ✅ Keycloak logout redirect
- ✅ Redirect to login page after logout

### User Profile
- ✅ Fetch /users/me endpoint (useUser hook)
- ✅ Display user name in header
- ✅ Display user avatar
- ✅ User profile dropdown menu

### Session Persistence
- ✅ Remember selected tenant
- ✅ Restore session on page refresh
- ✅ Handle expired session gracefully

**Phase 0 Total: 40/42 tasks** (2 blocked pending backoffice)

---

## Phase 1: Core Domain (tenant-app)

### Workers Module (Priority: Critical)

#### Worker List
- ⬜ Worker list page with data table
- ⬜ Status filter dropdown (multi-select)
- ⬜ Nationality filter dropdown (multi-select)
- ⬜ Job category filter
- ⬜ Availability filter (flexible bookings)
- ⬜ Date range filter (createdAt)
- ⬜ Search by name/CV serial/passport
- ⬜ Sortable columns (name, salary, date, status)
- ⬜ Pagination controls
- ⬜ Bulk actions toolbar
- ⬜ Export to CSV/Excel
- ⬜ Quick status badge display
- ⬜ Worker card/grid view toggle

#### Worker Detail/CV View
- ⬜ CV detail page layout
- ⬜ Identity section (passport, Emirates ID, CV serial)
- ⬜ Personal details section
- ⬜ Photo gallery with primary photo highlight
- ⬜ Video player for introduction video
- ⬜ Skills list with rating display (0-100 bar)
- ⬜ Languages with proficiency badges
- ⬜ Employment history display
- ⬜ Current status badge with color coding
- ⬜ Passport location indicator
- ⬜ Job category display
- ⬜ Salary information
- ⬜ Notes/comments section
- ⬜ Print-friendly CV view
- ⬜ Share CV via link

#### Worker Create/Edit Form
- ⬜ Multi-step form wizard
- ⬜ Step 1: Identity (passport, name EN/AR)
- ⬜ Step 2: Personal details (DOB, nationality, religion, etc.)
- ⬜ Step 3: Job & Pricing (category, salary)
- ⬜ Step 4: Skills & Languages (add/remove)
- ⬜ Step 5: Media upload (photos, videos)
- ⬜ Form validation with error messages
- ⬜ Draft auto-save
- ⬜ Duplicate passport warning
- ⬜ Job category selector with search
- ⬜ Date of birth picker with age calculation

#### State Transition UI
- ⬜ State transition button/dropdown
- ⬜ Valid transitions display
- ⬜ Transition confirmation modal
- ⬜ Reason input field
- ⬜ Related entity selector (contract, medical report)
- ⬜ State history timeline view
- ⬜ State history with user who triggered
- ⬜ Visual state machine diagram

#### Passport Custody Management
- ⬜ Current custody display card
- ⬜ Transfer passport modal
- ⬜ Location selector
- ⬜ Handed to name/entity input
- ⬜ Transfer notes
- ⬜ Custody history timeline
- ⬜ Custody audit trail

#### Skills & Languages Management
- ⬜ Inline skill add/edit
- ⬜ Skill rating slider (0-100)
- ⬜ Skill delete confirmation
- ⬜ Language add modal
- ⬜ Proficiency dropdown (Poor/Fair/Fluent)
- ⬜ Common skills autocomplete
- ⬜ Language autocomplete

#### Media Upload
- ⬜ Drag & drop upload zone
- ⬜ Multi-file upload support
- ⬜ Photo preview grid
- ⬜ Video thumbnail generation
- ⬜ Set primary photo action
- ⬜ Delete media confirmation
- ⬜ Media type selection (Photo/Video/Document)
- ⬜ Upload progress indicator
- ⬜ Image compression before upload

**Worker Module Total: 0/62 tasks**

---

### Clients Module (Priority: Critical)

#### Client List
- ⬜ Client list page with data table
- ⬜ Category filter (Local/Expat/Investor/VIP)
- ⬜ Sponsor file status filter
- ⬜ Verification status filter
- ⬜ Nationality filter
- ⬜ Emirate filter
- ⬜ Date range filter
- ⬜ Search by name/Emirates ID
- ⬜ Sortable columns
- ⬜ Pagination controls
- ⬜ Verification badge display
- ⬜ Block status indicator
- ⬜ Quick actions menu (verify, block)

#### Client Detail View
- ⬜ Client detail page layout
- ⬜ Emirates ID display with copy button
- ⬜ Name display (EN/AR)
- ⬜ Contact information section
- ⬜ Category badge
- ⬜ Sponsor file status
- ⬜ Verification status with date
- ⬜ Block reason display (if blocked)
- ⬜ Documents list (inline)
- ⬜ Discount cards list
- ⬜ Communication log timeline
- ⬜ Notes section
- ⬜ Quick action buttons (verify, block/unblock)

#### Client Create/Edit Form
- ⬜ Registration form layout
- ⬜ Emirates ID input with validation
- ⬜ Name inputs (EN/AR)
- ⬜ Passport input
- ⬜ Nationality selector
- ⬜ Contact fields (phone, email)
- ⬜ Emirate selector
- ⬜ Category override (admin only)
- ⬜ Document upload section
- ⬜ Salary certificate upload
- ⬜ Ejari/tenancy contract upload
- ⬜ Notes textarea
- ⬜ Form validation
- ⬜ Duplicate Emirates ID warning

#### Document Management
- ⬜ Documents list/grid view
- ⬜ Document type badges
- ⬜ Expiry date display with warning
- ⬜ Verification status badge
- ⬜ Add document modal
- ⬜ Document type selector
- ⬜ File upload with preview
- ⬜ Expiry date picker
- ⬜ Verify document action
- ⬜ Delete document confirmation
- ⬜ Download document action
- ⬜ Document viewer (PDF/Image)

#### Verification Workflow
- ⬜ Verification checklist display
- ⬜ Required documents indicator
- ⬜ Verify client button with confirmation
- ⬜ Verification success message
- ⬜ Verification history

#### Block/Unblock Workflow
- ⬜ Block client modal
- ⬜ Block reason input (required)
- ⬜ Block confirmation
- ⬜ Unblock action button
- ⬜ Block history display

#### Communication Logs
- ⬜ Communication log timeline
- ⬜ Channel badges (Phone/WhatsApp/Email/WalkIn)
- ⬜ Direction indicator (Inbound/Outbound)
- ⬜ Add communication modal
- ⬜ Channel selector
- ⬜ Direction toggle
- ⬜ Summary textarea
- ⬜ Pagination for logs

#### Discount Cards
- ⬜ Discount cards list
- ⬜ Card type display
- ⬜ Discount percentage badge
- ⬜ Validity status
- ⬜ Add discount card modal
- ⬜ Remove card action

**Clients Module Total: 0/59 tasks**

---

### Leads Module (Priority: High)

#### Lead List/Pipeline
- ⬜ Lead list page with data table
- ⬜ Kanban board view (pipeline)
- ⬜ Status filter (multi-select)
- ⬜ Source filter
- ⬜ Assigned user filter
- ⬜ Date range filter
- ⬜ Search by contact name/phone/email
- ⬜ Drag & drop in kanban view
- ⬜ Lead count per status
- ⬜ Assigned user avatar display
- ⬜ Quick status change
- ⬜ View toggle (list/kanban)

#### Lead Detail View
- ⬜ Lead detail page layout
- ⬜ Contact information display
- ⬜ Source badge
- ⬜ Status badge with color
- ⬜ Assigned user display
- ⬜ Notes section
- ⬜ Activity timeline
- ⬜ Convert to client button
- ⬜ Edit lead action

#### Lead Create/Edit Form
- ⬜ Create lead form
- ⬜ Source selector
- ⬜ Contact name input
- ⬜ Contact phone input with validation
- ⬜ Contact email input with validation
- ⬜ Notes textarea
- ⬜ Assign to user selector
- ⬜ Form validation
- ⬜ Edit lead form

#### Convert to Client Flow
- ⬜ Convert button on lead detail
- ⬜ Pre-filled client form from lead data
- ⬜ Emirates ID input (required)
- ⬜ Additional client fields
- ⬜ Conversion confirmation
- ⬜ Redirect to new client after conversion

#### Funnel Statistics
- ⬜ Funnel visualization chart
- ⬜ Stage count display
- ⬜ Conversion rate percentage
- ⬜ Date range selector
- ⬜ Source breakdown
- ⬜ Time-based trend chart

**Leads Module Total: 0/38 tasks**

---

## Phase 2: Team & Settings (tenant-app)

### Team Members
- ⬜ Members list page
- ⬜ Role badge display (Owner/Admin/Member)
- ⬜ Avatar display
- ⬜ Join date display
- ⬜ Change role dropdown (admin/owner only)
- ⬜ Remove member action
- ⬜ Self-removal with confirmation
- ⬜ Owner protection (can't remove last owner)

**Team Members Total: 0/8 tasks**

### Invitations
- ⬜ Pending invitations list
- ⬜ Invite member button
- ⬜ Invite modal with email input
- ⬜ Role selector for invitation
- ⬜ Email validation
- ⬜ Invitation sent confirmation
- ⬜ Revoke invitation action
- ⬜ Invitation status badges (pending/expired/accepted)
- ⬜ Resend invitation action
- ⬜ Public invitation accept page
- ⬜ Invitation expiry display

**Invitations Total: 0/11 tasks**

### Roles & Permissions
- ⬜ Roles list page
- ⬜ Role detail view
- ⬜ Permissions checklist display
- ⬜ Create role form
- ⬜ Role name input
- ⬜ Description textarea
- ⬜ Permissions multi-select
- ⬜ Grouped permissions by module
- ⬜ Edit role form
- ⬜ Delete role confirmation
- ⬜ System role indicator (can't edit)
- ⬜ Assign role to user action
- ⬜ User-role mapping view
- ⬜ Remove role from user

**Roles & Permissions Total: 0/14 tasks**

### API Keys
- ⬜ API keys list
- ⬜ Key prefix display (masked)
- ⬜ Expiry date display
- ⬜ Last used timestamp
- ⬜ Request count display
- ⬜ Create API key modal
- ⬜ Key name input
- ⬜ Permissions selection
- ⬜ Expiry date picker
- ⬜ Rate limit configuration
- ⬜ Show secret on creation (copy button)
- ⬜ Revoke key confirmation
- ⬜ Key usage logs view

**API Keys Total: 0/13 tasks**

### Settings
- ⬜ Tenant settings page layout
- ⬜ General settings (name, logo, description)
- ⬜ Logo upload
- ⬜ Website URL
- ⬜ Billing settings link
- ⬜ Webhooks configuration
- ⬜ Add webhook form
- ⬜ Event selection for webhooks
- ⬜ Test webhook action
- ⬜ Delete webhook confirmation
- ⬜ Notification preferences
- ⬜ Email notifications toggle
- ⬜ Danger zone (delete tenant)

**Settings Total: 0/13 tasks**

---

## Phase 3: Backoffice (admin-app) — 🔴 HIGH PRIORITY

> **Why High Priority?** Tenant-app users need to be added to tenants first. Backoffice must be built to unblock tenant-app testing and onboarding.

### Tenants Management (CRITICAL PATH)
- ✅ Tenants list page
- ✅ Create tenant form
- ✅ Status filter (Active/Suspended)
- ✅ Search by name/slug
- ✅ Tenant detail view
- ✅ Tenant type display
- ✅ Subscription status
- ✅ Member count (Members tab with count)
- ✅ Suspend tenant action
- ✅ Reactivate tenant action
- ✅ Force delete tenant (confirmation dialog)
- ✅ Row selection checkboxes (bulk actions UI)
- ⬜ Add user to tenant ← **Next priority**
- ⬜ Assign role to user in tenant

**Tenants Management Total: 12/14 tasks**

### Users Management (CRITICAL PATH)
- 🟡 Users list page (placeholder created, routing works)
- ⬜ Active/Inactive filter
- ⬜ Search by name/email
- ⬜ User detail view
- ⬜ Tenant memberships display
- ⬜ Add user to tenant ← **Shared with Tenants**
- ⬜ Login history
- ⬜ Deactivate user action
- ⬜ Reactivate user action
- ⬜ Create user form (admin)

**Users Management Total: 1/10 tasks (in progress)**

### Audit Logs
- 🟡 Audit logs page (placeholder created, routing works)
- ⬜ Audit events list
- ⬜ Audit logs list (entity changes)
- ⬜ Event name filter
- ⬜ Date range filter
- ⬜ User filter
- ⬜ Entity type filter
- ⬜ Payload viewer (JSON)
- ⬜ Old/new values diff view
- ⬜ Export audit logs

**Audit Logs Total: 1/10 tasks (in progress)**

### Feature Flags
- ⬜ Feature flags list
- ⬜ Enabled/disabled toggle
- ⬜ Percentage rollout slider
- ⬜ Create feature flag form
- ⬜ Flag name input
- ⬜ Description input
- ⬜ Enabled toggle
- ⬜ Percentage input
- ⬜ Edit feature flag
- ⬜ Delete feature flag
- ⬜ Plan-based targeting

**Feature Flags Total: 0/11 tasks**

### Plans & Subscriptions
- ⬜ Plans list view
- ⬜ Plan detail with features
- ⬜ Create/edit plan form
- ⬜ Plan prices management
- ⬜ Plan features management
- ⬜ Feature value types (boolean/numeric/unlimited)
- ⬜ Active subscriptions list
- ⬜ Subscription detail view
- ⬜ Manual subscription actions
- ⬜ Revenue dashboard

**Plans & Subscriptions Total: 0/10 tasks**

---

## Phase 4: Shared Components

### UI Components
- ⬜ Data table component with sorting/filtering
- ⬜ Pagination component
- ⬜ Filter panel component
- ⬜ Multi-select dropdown
- ⬜ Date range picker
- ⬜ Search input with debounce
- ⬜ Status badge component
- ⬜ Avatar component
- ⬜ File upload component
- ⬜ Modal/Dialog component
- ⬜ Toast notifications
- ⬜ Confirmation dialog
- ⬜ Loading skeleton
- ⬜ Empty state component
- ⬜ Error boundary
- ⬜ Breadcrumbs component

**UI Components Total: 0/16 tasks**

### Layout & Navigation
- ⬜ Sidebar navigation
- ⬜ Tenant switcher dropdown
- ⬜ User menu dropdown
- ⬜ Notifications popover
- ⬜ Mobile responsive navigation
- ⬜ Breadcrumb navigation
- ⬜ Page header component

**Layout & Navigation Total: 0/7 tasks**

### API Integration
- ⬜ React Query setup
- ⬜ API client with auth interceptor
- ⬜ Error handling middleware
- ⬜ Retry logic
- ⬜ Cache invalidation patterns
- ⬜ Optimistic updates
- ⬜ Pagination hooks
- ⬜ Filter state management
- ⬜ Sort state management

**API Integration Total: 0/9 tasks**

### Authentication
> **Moved to Phase 0** — Auth is foundational and must be completed first.
> See Phase 0 above for all 42 auth-related tasks.

---

## Summary

| Phase | Module | Not Started | In Progress | Complete | Blocked | Total |
|-------|--------|-------------|-------------|----------|---------|-------|
| **0** | **Authentication** | **0** | **0** | **40** | **2** | **42** |
| **3** | **Tenants (CRITICAL)** | **2** | **0** | **12** | **0** | **14** |
| **3** | **Users (CRITICAL)** | **9** | **1** | **0** | **0** | **10** |
| 1 | Workers | 62 | 0 | 0 | 0 | 62 |
| 1 | Clients | 59 | 0 | 0 | 0 | 59 |
| 1 | Leads | 38 | 0 | 0 | 0 | 38 |
| 2 | Team Members | 8 | 0 | 0 | 0 | 8 |
| 2 | Invitations | 11 | 0 | 0 | 0 | 11 |
| 2 | Roles & Permissions | 14 | 0 | 0 | 0 | 14 |
| 2 | API Keys | 13 | 0 | 0 | 0 | 13 |
| 2 | Settings | 13 | 0 | 0 | 0 | 13 |
| 3 | Audit Logs | 9 | 1 | 0 | 0 | 10 |
| 3 | Feature Flags | 11 | 0 | 0 | 0 | 11 |
| 3 | Plans & Subscriptions | 10 | 0 | 0 | 0 | 10 |
| 4 | UI Components | 16 | 0 | 0 | 0 | 16 |
| 4 | Layout & Navigation | 7 | 0 | 0 | 0 | 7 |
| 4 | API Integration | 9 | 0 | 0 | 0 | 9 |
| | **Total** | **303** | **0** | **40** | **2** | **345** |

---

## Priority Order (Updated Feb 20)

0. **✅ DONE:** Authentication — Keycloak OIDC integration complete
1. **🔴 CRITICAL (Now):** Backoffice Tenant + User Management — unblocks everything!
   - Create tenant, add users to tenant, assign roles
2. **High (After backoffice):** Workers, Clients, Leads modules (core business)
3. **Medium:** Team Members, Invitations, Roles (tenant-app settings)
4. **Lower:** API Keys, Settings, Remaining Backoffice, Shared Components

---

## Notes

- All forms should have proper validation matching backend DTOs
- Use TypeScript types from `@tadhub/api-types` package
- Follow React Query patterns for data fetching
- Implement optimistic updates where possible
- Mobile responsiveness is required for all modules
- RTL support (Arabic) is required for text displays

## Session 2026-02-20: Backend-Frontend Alignment Fix

### Issues Fixed
1. **EF Core Query Error** - `ListUserTenantsAsync` was using Include after Select
2. **Response Format Mismatch** - Frontend used `total`/`perPage`, API used `totalCount`/`pageSize`
3. **Keycloak Configuration** - JWT audience and role mapping fixed
4. **CORS Configuration** - Added `admin.endlessmaker.com` to allowed origins

### Key Changes
- `TenantService.cs` - Fixed EF Core Include order
- `client.ts` - Handle raw API responses (no wrapper)
- `common.ts` - Updated `PaginatedData` type to match API docs
- `TenantsListPage.tsx` - Use correct field names
- Keycloak DB - Added `platform-admin` role and audience mapper

### Deployment Status
- **API:** https://api.endlessmaker.com ✅
- **Backoffice:** https://admin.endlessmaker.com ✅
- **Keycloak:** https://auth.endlessmaker.com ✅

## Session 2026-02-20: Backend-Frontend Alignment Fix

### Issues Fixed
1. **EF Core Query Error** - `ListUserTenantsAsync` was using Include after Select
2. **Response Format Mismatch** - Frontend used `total`/`perPage`, API used `totalCount`/`pageSize`
3. **Keycloak Configuration** - JWT audience and role mapping fixed
4. **CORS Configuration** - Added `admin.endlessmaker.com` to allowed origins

### Key Changes
- `TenantService.cs` - Fixed EF Core Include order
- `client.ts` - Handle raw API responses (no wrapper)
- `common.ts` - Updated `PaginatedData` type to match API docs
- `TenantsListPage.tsx` - Use correct field names
- Keycloak DB - Added `platform-admin` role and audience mapper

### Deployment Status
- **API:** https://api.endlessmaker.com ✅
- **Backoffice:** https://admin.endlessmaker.com ✅
- **Keycloak:** https://auth.endlessmaker.com ✅
