# Task 09: Supplier Portal

## Summary
Suppliers currently exist as data entities only. The spec requires suppliers to be users who can log in and self-service: register maids, upload documents, track maid status, and view commission payments.

## Current State
- `Supplier` entity exists (global, not tenant-scoped)
- `TenantSupplier` maps suppliers to tenants
- `Portal` module exists in the codebase but unclear if it's wired for supplier use
- Suppliers cannot log in or interact with the system directly
- Candidates have `TenantSupplierId` to link to supplier

## Required Changes

### 9.1 Supplier User Accounts

- [ ] Allow suppliers to have user accounts linked to their `Supplier` entity
- [ ] Supplier users should be scoped to see only their own data across tenants they're linked to
- [ ] Investigate if existing `Portal` module can be leveraged, or if a new supplier app is needed
- [ ] Supplier role with permissions:
  - `candidates.create` (only their own)
  - `candidates.view` (only their own)
  - `documents.create` (for their candidates)
  - `documents.view` (for their candidates/workers)
  - `workers.view` (only workers sourced by them)
  - `supplier_payments.view` (only their own commissions)
  - `arrivals.view` (only for their workers)
  - `arrivals.upload_pre_travel_photo`
  - `notifications.view`

### 9.2 Supplier Dashboard

- [ ] Overview: total maids registered, approved, deployed, pending
- [ ] Recent notifications
- [ ] Commission summary

### 9.3 Supplier Features

- [ ] **Register maids**: submit candidate form with documents
- [ ] **Upload documents**: passport, photo, medical certificate, CoC
- [ ] **Track maid status**: see current status of each maid they submitted
- [ ] **View commission payments**: see payment history and pending payments
- [ ] **Upload pre-travel photos**: before maid travels (linked to Arrival module)
- [ ] **Receive notifications**: approval, deployment, arrival issues, cost recovery

### 9.4 Data Isolation

- [ ] Supplier users must only see their own candidates/workers
- [ ] API endpoints must filter by supplier ID derived from authenticated user
- [ ] No access to other suppliers' data, financial details, or client information

### 9.5 Frontend

- [ ] Decide: separate app (`web/supplier-app/`) or a role-based view within tenant app
- [ ] Supplier-specific layout and navigation
- [ ] Maid registration form (candidate creation scoped to supplier)
- [ ] Maid status tracking page
- [ ] Commission/payments view
- [ ] Document upload interface
- [ ] Notification center
- [ ] i18n (en, ar)

## Acceptance Criteria

1. Suppliers can register and log in to the system
2. Suppliers can submit new maid registrations with all required documents
3. Suppliers can track the status of each maid they submitted (from registration through deployment)
4. Suppliers can view their commission payments and payment history
5. Suppliers can upload pre-travel photos for maids about to travel
6. Suppliers receive notifications for: maid approval/rejection, deployment, arrival issues, cost recovery demands
7. Suppliers can ONLY see data related to their own maids — no access to other suppliers' data
8. Suppliers cannot access client information, financial reports, or other office operations
9. Supplier portal works on desktop and mobile browsers
10. All supplier actions are audit-logged
