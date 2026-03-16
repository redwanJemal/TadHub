# TadHub — User Journey Document

This document maps the complete user journey for each role in TadHub, from login to every action available, organized by the natural workflow sequence.

---

## Table of Contents

1. [Owner / Admin](#1-owner--admin)
2. [Sales](#2-sales)
3. [Operations](#3-operations)
4. [Accountant](#4-accountant)
5. [Viewer](#5-viewer)
6. [Driver](#6-driver)
7. [Accommodation Staff](#7-accommodation-staff)
8. [Supplier Portal User](#8-supplier-portal-user)
9. [Cross-Role Workflow (End-to-End)](#9-cross-role-workflow-end-to-end)

---

## 1. Owner / Admin

> **Owner** has full access including destructive operations (delete). **Admin** has the same access minus all delete operations.

### 1.1 Login & Dashboard

1. Log in via Keycloak SSO (`/login` → `/callback`)
2. Land on **Dashboard** (`/`)
   - View KPI cards: Active Workers, Active Contracts, Pending Candidates, Total Clients, Compliance Rate, Expiring Documents
   - Quick Actions: Add Candidate, New Contract, Compliance, Workers
   - Activity feed showing recent system events

### 1.2 Recruitment — Supplier Management

1. Navigate to **Recruitment → Suppliers** (`/suppliers`)
2. View all registered suppliers with contact info and maid counts
3. Click a supplier to see detail page (`/suppliers/:id`) — linked candidates, workers, commissions
4. Create or edit supplier records (Owner/Admin only via `suppliers.manage`)

### 1.3 Recruitment — Candidate Pipeline

1. Navigate to **Recruitment → Candidates** (`/candidates`)
2. View candidate list with status filters (Received, UnderReview, Approved, Rejected)
3. **Register new candidate** (`/candidates/new`) — fill personal info, nationality, languages, skills, location type (Inside/Outside Country), upload passport + photo
4. **Review a candidate** — open detail page (`/candidates/:id`)
   - Change status: Received → UnderReview → Approved (or Rejected with reason)
   - On **Approval**: worker record auto-created via event, candidate appears in inventory
5. Edit candidate profile (`/candidates/:id/edit`) — only while status is Received or UnderReview

### 1.4 Sales — Client Management

1. Navigate to **Sales → Clients** (`/clients`)
2. View, create, and edit client (customer/employer) records
3. Clients are linked to placements and contracts

### 1.5 Sales — Placement Booking

1. Navigate to **Sales → Placements** (`/placements`)
2. View **Placement Board** — Kanban-style columns showing placement pipeline status
3. Toggle board view: All / Outside Country / Inside Country
4. **Create Placement** (`/placements/new`)
   - Select an approved candidate (worker) from inventory
   - Select a client
   - System auto-detects flow type based on candidate location:
     - **Outside Country** → 9-step flow
     - **Inside Country** → 5-step flow (trial-based)
5. View placement detail (`/placements/:id`) — see progress checklist, advance through steps

#### Outside Country Flow (9 Steps)

| Step | Action | System Effect |
|------|--------|---------------|
| 1. Booked | Placement created | Candidate status → Booked |
| 2. ContractCreated | Create 2-year contract with guarantee period | Contract linked to placement |
| 3. EmploymentVisa | Visa application created/linked | Required docs: attested medical, passport, photo |
| 4. TicketArranged | Record flight & travel details | Arrival record auto-created (Scheduled) |
| 5. Arrived | Driver confirms pickup, maid at accommodation | Worker record created, auto check-in |
| 6. Deployed | Maid sent to client | Auto check-out from accommodation, commission to supplier |
| 7. FullPaymentReceived | Customer pays remaining balance | Payment recorded |
| 8. ResidenceVisa | Residence visa application | Only after deployment |
| 9. EmiratesID | National ID processing | Final step → Completed |

#### Inside Country Flow (5 Steps)

| Step | Action | System Effect |
|------|--------|---------------|
| 1. Booked | Placement created | Candidate status → Booked |
| 2. InTrial | 5-day trial created | Trial contract auto-generated, end date = start + 5 days |
| 3. TrialSuccessful | Trial completes successfully | Auto-creates 2-year employment contract |
| 4. StatusChanged | Contract active, maid deployed | Worker status updated |
| 5. Completed | Visa processing done | Placement complete |

*If trial fails*: maid returns to Available status, placement cancelled.

### 1.6 Sales — Trial Management

1. Navigate to **Sales → Trials** (`/trials`)
2. View active and completed trials
3. **Create Trial** (`/trials/new`) — select inside-country placement, set start date
4. End date auto-calculated (start + 5 days)
5. Record outcome: Successful (auto-creates contract) or Failed (maid back to inventory)

### 1.7 Sales — Contract Management

1. Navigate to **Sales → Contracts** (`/contracts`)
2. View all contracts with status filters
3. **Create Contract** (`/contracts/new`) — select client, worker, set dates, guarantee period (6 months / 1 year / 2 years)
4. View contract detail (`/contracts/:id`) — guarantee end date, linked placement, termination info
5. Terminate contract with reason — links to returnee or runaway case

### 1.8 Operations — Arrivals

1. Navigate to **Operations → Arrivals** (`/arrivals`)
2. View scheduled, in-progress, and completed arrivals
3. **Schedule Arrival** (`/arrivals/new`) — flight info, expected date, assign driver
4. Track arrival status: Scheduled → PickedUp → AtAccommodation → (or Cancelled/NoShow)
5. System runs hourly no-show detection — auto-flags overdue arrivals, sends urgent notifications

### 1.9 Operations — Accommodation

1. Navigate to **Operations → Accommodations** (`/accommodations`)
2. View current occupants and daily accommodation list
3. **Check-In** (`/accommodations/check-in`) — assign room/location to arriving maid
4. **Check-Out** — record departure reason (deployed, returned to country, etc.)
5. Auto-events: check-in on arrival confirmation, check-out on deployment/runaway/returnee

### 1.10 Operations — Visa Applications

1. Navigate to **Operations → Visa Applications** (`/visa-applications`)
2. View all visa applications with status filters
3. **Create Visa Application** (`/visa-applications/new`) — select type (Employment / Residence / Emirates ID), link to worker
4. Advance through statuses: DocumentsCollecting → Applied → UnderProcess → Approved → Issued
5. Sequencing enforced: Residence visa requires approved Employment visa

### 1.11 Operations — Compliance & Documents

1. Navigate to **Operations → Compliance** (`/compliance`)
2. View compliance dashboard — rate percentage, expiring documents
3. Access worker document folders — passport, photo, medical, CoC, contracts
4. Track document expiry dates and renewal needs

### 1.12 Cases — Returnees

1. Navigate to **Cases → Returnees** (`/returnees`)
2. View returnee case list with status filters
3. **Create Returnee Case** (`/returnees/new`)
   - Select deployed worker, specify return type:
     - **Return to Office** — maid goes back to inventory (Available with Returnee marker)
     - **Return to Country** — maid exits the system
   - Record return reason and worker condition
4. System auto-calculates refund: `Refund = TotalPaid − (MonthsWorked × MonthlyValue)`
5. **Approve case** → triggers:
   - Credit note invoice for customer refund
   - Supplier debits (commission, visa, ticket, medical, transport) if within guarantee period
   - Worker status → Returnee
   - Auto accommodation check-out (if returning to country)
6. **Settle case** → financial records finalized

### 1.13 Cases — Runaways

1. Navigate to **Cases → Runaways** (`/runaways`)
2. View runaway case list
3. **Report Runaway** (`/runaways/new`)
   - Select deployed worker, record last known location and details
   - Contract immediately terminated on report
   - Worker status → Absconded
4. **Confirm case** → triggers:
   - Supplier debits auto-generated if within guarantee period
   - Auto accommodation check-out
5. Track and record all expenses (visa, ticket, medical, transportation)
6. **Settle & Close** → supplier liability finalized

### 1.14 Finance — Invoices

1. Navigate to **Finance → Invoices** (`/finance/invoices`)
2. View summary cards: Total Revenue, Total Paid, Outstanding, Overdue
3. Filter by status: Draft, Issued, PartiallyPaid, Paid, Overdue, Cancelled, Refunded
4. **Create Invoice** (`/finance/invoices/new`) — link to placement/contract, set line items
5. Manage invoice status transitions
6. Credit note invoices auto-created on returnee approval (for refunds)

### 1.15 Finance — Payments

1. Navigate to **Finance → Payments** (`/finance/payments`)
2. View payment history
3. **Record Payment** (`/finance/payments/record`) — link to invoice, record amount and method
4. Track payment status: Pending → PartiallyPaid → Paid
5. Process refunds when applicable

### 1.16 Finance — Supplier Debits & Payments

1. Navigate to **Finance → Supplier Debits** (`/finance/supplier-debits`)
   - View auto-generated debits from returnee/runaway cases
   - Settle or waive debits
2. Navigate to **Finance → Supplier Payments** (`/finance/supplier-payments`)
   - View commission payments and credits
   - Auto-created on deployment (if commission auto-calc enabled)

### 1.17 Finance — Reports & Settings

1. **Financial Reports** (`/finance/reports`) — revenue analysis, payment trends, supplier liability
2. **Cash Reconciliation** (`/finance/cash-reconciliation`) — reconcile cash payments
3. **Discount Programs** (`/finance/discount-programs`) — manage active discount programs
4. **Financial Settings** (`/finance/settings`) — configure:
   - Commission calculation (fixed amount or percentage)
   - Accommodation daily rates
   - Refund period rules
   - Guarantee period defaults

### 1.18 Admin — Reports Hub

1. Navigate to **Admin → Reports** (`/reports`)
2. Access 10+ operational reports:
   - **Inventory Report** — available maids by nationality/supplier
   - **Deployed Report** — currently deployed workers
   - **Returnee Report** — returnee case summary
   - **Runaway Report** — runaway case summary
   - **Arrivals Report** — arrival records and statuses
   - **Accommodation Daily Report** — current occupancy
   - **Deployment Pipeline** — visual pipeline distribution
   - **Supplier Commissions** — commission totals by supplier
   - **Refund Report** — all refund records
   - **Cost Per Maid Report** — breakdown per maid
3. Filter by date range, export to CSV

### 1.19 Admin — Team Management

1. Navigate to **Admin → Team** (`/team`)
2. View all team members with roles
3. **Invite new member** — assign role (Admin, Sales, Operations, Accountant, Viewer, Driver, Accommodation Staff)
4. **Manage roles** — change member role assignments
5. **Remove member** — revoke access

### 1.20 Admin — Country Packages

1. Navigate to **Admin → Country Packages** (`/country-packages`)
2. View packages with cost breakdowns (11 cost fields)
3. **Create Package** (`/country-packages/new`) — set costs per country:
   - Supplier commission (fixed or percentage), visa fees, ticket, medical, insurance, etc.
4. Set one default package per country — auto-applied at booking time

### 1.21 Admin — Audit & Settings

1. **Audit** (`/audit`) — view complete audit trail of all status changes, user actions, document uploads, financial transactions. Export audit data.
2. **Settings** (`/settings/general`) — general tenant configuration
3. **Notification Settings** (`/settings/notifications`) — manage notification templates (en/ar)
4. **Notification Preferences** (`/notification-preferences`) — configure per-event notification delivery

### 1.22 Owner-Only Actions

The Owner role additionally can:
- **Delete** any entity: candidates, workers, clients, contracts, suppliers, invoices, payments, trials, returnees, runaways, visas, arrivals, documents, packages, roles
- **Delete tenant** (`tenancy.delete`)

---

## 2. Sales

> Recruitment and sales pipeline focus. Can create and edit but cannot manage status transitions or delete.

### 2.1 Login & Dashboard

1. Log in → land on Dashboard
2. View KPI cards and activity feed
3. Quick Actions: Add Candidate (if permitted), Workers

### 2.2 Supplier Browsing

1. Navigate to **Recruitment → Suppliers** (`/suppliers`)
2. View suppliers and their details (read + manage access)
3. Browse supplier maid inventories

### 2.3 Candidate Registration

1. Navigate to **Recruitment → Candidates** (`/candidates`)
2. View candidate list
3. **Create new candidate** (`/candidates/new`) — full registration form
4. **Edit candidate** (`/candidates/:id/edit`) — while in Received/UnderReview status
5. *Cannot* approve/reject candidates — status management requires Admin

### 2.4 Client Management

1. Navigate to **Sales → Clients** (`/clients`)
2. View, create, and edit client records
3. Manage client contact information and requirements

### 2.5 Placement Booking

1. Navigate to **Sales → Placements** (`/placements`)
2. View placement board
3. **Create Placement** (`/placements/new`) — book approved candidate for client
4. View placement progress (`/placements/:id`)
5. *Cannot* advance placement steps — status management requires Admin/Operations

### 2.6 Trial Management

1. Navigate to **Sales → Trials** (`/trials`)
2. View and create trials for inside-country placements
3. Track trial outcomes

### 2.7 Contract Management

1. Navigate to **Sales → Contracts** (`/contracts`)
2. View and create contracts
3. Edit contract details
4. *Cannot* terminate contracts or manage status

### 2.8 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Recruitment (Suppliers, Candidates) | Yes |
| Sales (Workers, Clients, Placements, Trials, Contracts) | Yes |
| Operations | No |
| Finance | No |
| Cases | No |
| Admin | No |

---

## 3. Operations

> Handles arrivals, accommodations, visas, documents, and case management. Read-only access to sales data.

### 3.1 Login & Dashboard

1. Log in → land on Dashboard
2. View KPIs focused on operational metrics

### 3.2 Arrival Coordination

1. Navigate to **Operations → Arrivals** (`/arrivals`)
2. View all arrivals with status filters
3. **Schedule Arrival** (`/arrivals/new`) — enter flight details, assign driver
4. Track arrival status progression
5. Monitor no-show detection alerts

### 3.3 Accommodation Management

1. Navigate to **Operations → Accommodations** (`/accommodations`)
2. View daily occupant list
3. **Check-In** (`/accommodations/check-in`) — assign room to arriving maid
4. **Check-Out** — record departure reason
5. Monitor auto check-in/check-out events from arrivals and placements

### 3.4 Visa Processing

1. Navigate to **Operations → Visa Applications** (`/visa-applications`)
2. View visa applications
3. **Create Visa Application** (`/visa-applications/new`) — select type, link to worker
4. Advance visa through status pipeline
5. Enforce sequencing rules (employment → residence → emirates ID)

### 3.5 Compliance & Documents

1. Navigate to **Operations → Compliance** (`/compliance`)
2. Monitor compliance rates and expiring documents
3. Create and edit document records
4. Upload and manage worker documentation

### 3.6 Worker Status Management

1. Navigate to **Sales → Workers** (`/workers`) — read + edit access
2. View worker profiles and status
3. **Change worker status** — manage status transitions (e.g., Available → InAccommodation → Active)
4. *Cannot* create or delete workers

### 3.7 Case Management — Returnees

1. Navigate to **Cases → Returnees** (`/returnees`)
2. Create, manage, and settle returnee cases
3. Full lifecycle: Created → Approved → Settled

### 3.8 Case Management — Runaways

1. Navigate to **Cases → Runaways** (`/runaways`)
2. Report, manage, and settle runaway cases
3. Full lifecycle: Reported → Confirmed → Settled → Closed

### 3.9 Read-Only Access

Can view but not modify:
- Clients, Contracts, Placements, Suppliers, Candidates
- Reports (view + export)

### 3.10 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Recruitment | No (can view Suppliers/Candidates individually) |
| Sales (Workers view, Clients view, Placements view, Contracts view) | Yes (read-only) |
| Operations (Arrivals, Accommodations, Visas, Compliance) | Yes (full) |
| Cases (Returnees, Runaways) | Yes (full) |
| Finance | No |
| Admin (Reports only) | Yes |

---

## 4. Accountant

> Financial operations with read-only view of business data.

### 4.1 Login & Dashboard

1. Log in → land on Dashboard
2. View KPIs — focus on financial metrics

### 4.2 Invoice Management

1. Navigate to **Finance → Invoices** (`/finance/invoices`)
2. View summary cards: Total Revenue, Total Paid, Outstanding, Overdue
3. **Create Invoice** (`/finance/invoices/new`)
4. Edit invoice details
5. Manage invoice status: Draft → Issued → PartiallyPaid → Paid (or Cancelled/Refunded)

### 4.3 Payment Processing

1. Navigate to **Finance → Payments** (`/finance/payments`)
2. View all payments
3. **Record Payment** (`/finance/payments/record`)
4. Manage payment status
5. Process refunds

### 4.4 Supplier Financial Management

1. **Supplier Debits** (`/finance/supplier-debits`) — view, create, edit, manage status
2. **Supplier Payments** (`/finance/supplier-payments`) — view, create, edit, manage status
3. **Discount Programs** (`/finance/discount-programs`) — view, create, edit

### 4.5 Financial Reporting & Settings

1. **Financial Reports** (`/finance/reports`) — revenue, payment, and liability analysis
2. **Financial Settings** (`/finance/settings`) — configure commission, accommodation rates, refund rules
3. **Cash Reconciliation** (`/finance/cash-reconciliation`)

### 4.6 Analytics & Exports

1. View analytics dashboards
2. Export financial data
3. Access operational reports (read-only)

### 4.7 Read-Only Access

Can view but not modify:
- Workers, Clients, Contracts, Suppliers, Packages

### 4.8 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Finance (Invoices, Payments, Supplier Debits, Reports, Settings) | Yes (full) |
| Admin (Reports) | Yes (view only) |
| Recruitment | No |
| Sales | No (can view individual entities) |
| Operations | No |
| Cases | No |

---

## 5. Viewer

> Read-only access to dashboard and core operational data.

### 5.1 Login & Dashboard

1. Log in → land on Dashboard
2. View all KPI cards and activity feed

### 5.2 Browse Data

1. **Workers** (`/workers`) — view worker list and profiles
2. **Clients** (`/clients`) — view client records
3. **Contracts** (`/contracts`) — view contract list and details
4. **Placements** (`/placements`) — view placement board and details
5. **Arrivals** (`/arrivals`) — view arrival records
6. **Reports** (`/reports`) — view operational reports

### 5.3 No Action Capabilities

- Cannot create, edit, or delete any records
- Cannot manage statuses or approve anything
- Cannot access Finance, Cases, or Admin sections
- All create/edit/delete buttons are hidden

### 5.4 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Workers (view only) | Yes |
| Clients (view only) | Yes |
| Contracts (view only) | Yes |
| Placements (view only) | Yes |
| Arrivals (view only) | Yes |
| Reports (view only) | Yes |
| Everything else | No |

---

## 6. Driver

> Minimal access — only assigned pickups and photo uploads.

### 6.1 Login & Driver Dashboard

1. Log in → land on **Dashboard** (`/`)
2. Navigate to **Driver Dashboard** (`/driver`) — exclusive to this role
3. View **Active Pickups** — cards showing:
   - Maid name and nationality
   - Flight info and arrival time
   - Pickup location
4. View **Completed Pickups** — history of confirmed pickups

### 6.2 Confirm Pickup

1. Select an active pickup from the dashboard
2. Tap **Confirm Pickup** — records timestamp
3. Status updates to PickedUp
4. System notifies operations team

### 6.3 Upload Photos

1. After confirming pickup, upload arrival photo
2. Mobile camera integration for photo capture
3. Photo stored and viewable by operations team

### 6.4 Notifications

1. Navigate to **Notifications** (`/notifications`)
2. View pickup assignments and urgent alerts
3. Mark notifications as read

### 6.5 Access Restrictions

- **Cannot** access any other module (Candidates, Workers, Finance, etc.)
- **Cannot** view other drivers' assignments
- **Cannot** schedule arrivals or manage accommodation

### 6.6 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Driver Dashboard | Yes |
| Notifications | Yes |
| Everything else | No |

---

## 7. Accommodation Staff

> Manages maid housing — check-in, check-out, occupancy tracking.

### 7.1 Login & Dashboard

1. Log in → land on Dashboard
2. View accommodation-relevant KPIs

### 7.2 Daily Accommodation Management

1. Navigate to **Operations → Accommodations** (`/accommodations`)
2. View current **Occupant List** — all checked-in maids with room assignments
3. View accommodation detail pages (`/accommodations/:id`)

### 7.3 Check-In Process

1. Navigate to **Check-In** (`/accommodations/check-in`)
2. Select arriving maid (from arrivals or manual entry)
3. Assign room/location
4. Record check-in — creates AccommodationStay record
5. Note: auto check-in also occurs when arrival status → AtAccommodation

### 7.4 Check-Out Process

1. Select checked-in maid from occupant list
2. Initiate **Check-Out**
3. Select mandatory departure reason:
   - Deployed to customer
   - Returned to country
   - Transferred
   - Other (with notes)
4. Record check-out — updates stay record with departure date and reason

### 7.5 Incoming Arrivals Monitoring

1. Navigate to **Arrivals** (`/arrivals`) — view-only access
2. Monitor scheduled and in-progress arrivals
3. Prepare for incoming maids based on expected arrival times

### 7.6 Worker & Candidate Browsing

1. **Workers** (`/workers`) — view worker profiles and status
2. **Candidates** (`/candidates`) — view candidate information
3. Read-only — cannot create, edit, or manage status

### 7.7 Access Restrictions

- **Cannot** modify arrivals (only view)
- **Cannot** create or edit workers/candidates
- **Cannot** access Finance, Cases, Sales, or Admin
- **Cannot** delete any records

### 7.8 Sidebar Visibility

| Section | Visible |
|---------|---------|
| Dashboard | Yes |
| Accommodations (view + manage) | Yes |
| Arrivals (view only) | Yes |
| Workers (view only) | Yes |
| Candidates (view only) | Yes |
| Notifications | Yes |
| Everything else | No |

---

## 8. Supplier Portal User

> Self-service portal for suppliers to manage their own candidates, track workers, and view commissions.

### 8.1 Login & Supplier Dashboard

1. Log in via supplier portal credentials
2. Land on **Supplier Dashboard** — personalized KPIs:
   - Total candidates submitted
   - Active workers deployed
   - Total commissions earned
   - Pending arrivals

### 8.2 Candidate Management

1. View **My Candidates** — only their own candidates (data isolation enforced)
2. **Register New Candidate** — submit candidate with personal info, documents, nationality
3. Track candidate status: Received → UnderReview → Approved/Rejected
4. *Cannot* approve or reject candidates — requires office Admin

### 8.3 Worker Tracking

1. View **My Workers** — workers originated from their candidates
2. See worker status, deployment location, client assignment
3. Read-only — cannot modify worker records

### 8.4 Commission & Financial View

1. View **My Commissions** — commission payments and pending amounts
2. View supplier debits (if any — from guarantee period returns/runaways)
3. Track payment history

### 8.5 Arrival Monitoring

1. View **My Arrivals** — arrivals for their candidates
2. Track arrival status and ETA
3. **Upload Pre-Travel Photo** — submit maid photo before travel

### 8.6 Document Upload

1. Upload documents for their candidates/workers
2. Passport copies, medical certificates, photos

### 8.7 Access Restrictions

- **Strict data isolation** — can only see their own data
- **Cannot** access other suppliers' data
- **Cannot** approve candidates or manage contracts
- **Cannot** access admin, finance, operations, or case management modules
- **Cannot** book maids for clients

---

## 9. Cross-Role Workflow (End-to-End)

This section shows how roles collaborate through a complete outside-country maid deployment lifecycle.

### Phase 1: Recruitment

| Step | Actor | Action |
|------|-------|--------|
| 1 | **Supplier** | Registers candidate via Supplier Portal with documents |
| 2 | **Admin/Owner** | Reviews candidate, changes status to UnderReview |
| 3 | **Admin/Owner** | Approves candidate → worker auto-created, appears in inventory |

### Phase 2: Sales & Booking

| Step | Actor | Action |
|------|-------|--------|
| 4 | **Sales** | Creates client record |
| 5 | **Sales** | Creates placement — selects approved candidate + client |
| 6 | **Admin** | Creates 2-year contract with guarantee period |
| 7 | **Admin** | Advances placement to EmploymentVisa step |

### Phase 3: Visa & Travel

| Step | Actor | Action |
|------|-------|--------|
| 8 | **Operations** | Creates employment visa application, uploads required documents |
| 9 | **Operations** | Advances visa: Applied → UnderProcess → Approved → Issued |
| 10 | **Admin** | Records ticket/travel details → arrival auto-scheduled |

### Phase 4: Arrival & Accommodation

| Step | Actor | Action |
|------|-------|--------|
| 11 | **Operations** | Assigns driver to scheduled arrival |
| 12 | **Driver** | Confirms pickup at airport, uploads photo |
| 13 | System | Auto check-in at accommodation (MaidAtAccommodationEvent) |
| 14 | **Accommodation Staff** | Verifies room assignment, manages occupant list |

### Phase 5: Deployment & Payment

| Step | Actor | Action |
|------|-------|--------|
| 15 | **Admin** | Advances placement to Deployed — maid sent to client |
| 16 | System | Auto check-out from accommodation, commission created for supplier |
| 17 | **Accountant** | Creates invoice for customer |
| 18 | **Accountant** | Records customer payment → FullPaymentReceived |

### Phase 6: Post-Deployment

| Step | Actor | Action |
|------|-------|--------|
| 19 | **Operations** | Creates residence visa application |
| 20 | **Operations** | Creates Emirates ID application |
| 21 | **Admin** | Marks placement as Completed |

### Phase 7: Exception Handling (If Needed)

**Returnee Path:**
| Step | Actor | Action |
|------|-------|--------|
| R1 | **Operations** | Creates returnee case with reason |
| R2 | **Admin** | Approves case → refund calculated, supplier debits created |
| R3 | **Accountant** | Processes credit note and customer refund |
| R4 | **Operations** | Settles case |

**Runaway Path:**
| Step | Actor | Action |
|------|-------|--------|
| W1 | **Operations** | Reports runaway → contract terminated, worker status → Absconded |
| W2 | **Admin** | Confirms case → supplier debits auto-created if in guarantee |
| W3 | **Accountant** | Settles supplier debits |
| W4 | **Operations** | Closes case |

### Phase 8: Oversight & Reporting

| Step | Actor | Action |
|------|-------|--------|
| 22 | **Admin/Owner** | Reviews audit trail for compliance |
| 23 | **Accountant** | Generates financial reports |
| 24 | **Admin** | Generates operational reports (inventory, pipeline, commissions) |
| 25 | **Viewer** | Browses dashboards and reports for visibility |

---

## Appendix: Permission Matrix Summary

| Capability | Owner | Admin | Sales | Operations | Accountant | Viewer | Driver | Acc. Staff |
|------------|:-----:|:-----:|:-----:|:----------:|:----------:|:------:|:------:|:----------:|
| View Dashboard | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Manage Suppliers | ✓ | ✓ | ✓ | — | — | — | — | — |
| Manage Candidates | ✓ | ✓ | C/E | — | — | — | — | — |
| Approve Candidates | ✓ | ✓ | — | — | — | — | — | — |
| Manage Clients | ✓ | ✓ | ✓ | R | R | R | — | — |
| Manage Workers | ✓ | ✓ | C/E | E/S | R | R | — | R |
| Manage Placements | ✓ | ✓ | ✓ | R | — | R | — | — |
| Manage Trials | ✓ | ✓ | ✓ | R | — | — | — | — |
| Manage Contracts | ✓ | ✓ | C/E | R | R | R | — | — |
| Manage Arrivals | ✓ | ✓ | — | ✓ | — | R | — | R |
| Driver Actions | ✓ | ✓ | — | — | — | — | ✓ | — |
| Manage Accommodation | ✓ | ✓ | — | ✓ | — | — | — | ✓ |
| Manage Visas | ✓ | ✓ | — | ✓ | — | — | — | — |
| Manage Documents | ✓ | ✓ | — | ✓ | — | — | — | — |
| Manage Returnees | ✓ | ✓ | — | ✓ | — | — | — | — |
| Manage Runaways | ✓ | ✓ | — | ✓ | — | — | — | — |
| Manage Invoices | ✓ | ✓ | — | — | ✓ | — | — | — |
| Manage Payments | ✓ | ✓ | — | — | ✓ | — | — | — |
| Manage Supplier Debits | ✓ | ✓ | — | — | ✓ | — | — | — |
| Financial Reports | ✓ | ✓ | — | — | ✓ | — | — | — |
| Operational Reports | ✓ | ✓ | R | R | R | R | — | — |
| Team Management | ✓ | ✓ | — | — | — | — | — | — |
| Audit Logs | ✓ | ✓ | — | — | — | — | — | — |
| Settings | ✓ | ✓ | — | — | — | — | — | — |
| Delete Entities | ✓ | — | — | — | — | — | — | — |

**Legend:** ✓ = Full access, C/E = Create/Edit only, E/S = Edit/Status only, R = Read only, — = No access
