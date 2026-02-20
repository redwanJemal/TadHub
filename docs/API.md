# TadHub API Documentation

**Version:** 1.0.0  
**Base URL:** `https://api.tadhub.com/api/v1`  
**Authentication:** Bearer Token (Keycloak JWT)

## Table of Contents

1. [Authentication](#authentication)
2. [Standard Response Formats](#standard-response-formats)
3. [Query Parameters](#query-parameters)
4. [Tadbeer Domain](#tadbeer-domain)
   - [Workers](#workers)
   - [Clients](#clients)
   - [Leads](#leads)
5. [Tenant Management](#tenant-management)
   - [Tenants](#tenants)
   - [Tenant Members](#tenant-members)
   - [Tenant Invitations](#tenant-invitations)
   - [Roles](#roles)
6. [Platform](#platform)
   - [Users](#users)
   - [Permissions](#permissions)
   - [API Keys](#api-keys)
   - [Plans](#plans)
   - [Subscriptions](#subscriptions)
   - [Credits](#credits)
   - [Notifications](#notifications)
   - [Audit](#audit)
   - [Webhooks](#webhooks)
   - [Analytics](#analytics)
   - [Feature Flags](#feature-flags)
   - [Portals](#portals)

---

## Authentication

All endpoints (except public ones) require a valid JWT token from Keycloak.

### Headers
```http
Authorization: Bearer <jwt_token>
X-Tenant-Id: <tenant_uuid>  # Required for tenant-scoped endpoints
```

### API Key Authentication
API keys can be used for programmatic access:
```http
X-Api-Key: <api_key>
```

---

## Standard Response Formats

### Success Response (Single Item)
```json
{
  "id": "uuid",
  "...": "fields"
}
```

### Success Response (List with Pagination)
```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Error Response
```json
{
  "error": "ERROR_CODE",
  "message": "Human readable error message"
}
```

### Common Error Codes
| Code | HTTP Status | Description |
|------|-------------|-------------|
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource already exists or state conflict |
| `VALIDATION_ERROR` | 400 | Input validation failed |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `UNAUTHORIZED` | 401 | Missing or invalid authentication |

---

## Query Parameters

### Pagination
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | number | 1 | Page number (1-indexed) |
| `pageSize` | number | 20 | Items per page (max: 100) |

### Sorting
```
?sort=createdAt:desc
?sort=-createdAt,name  # Multiple fields, - prefix = descending
```

### Filtering
```
?filter[status]=active
?filter[status]=active,pending  # Multiple values (OR)
?filter[createdAt][gte]=2024-01-01
?filter[createdAt][lt]=2024-12-31
?filter[isActive]=true
?filter[name][contains]=John
```

**Operators:** `eq`, `gt`, `gte`, `lt`, `lte`, `contains`, `startsWith`, `endsWith`, `isNull`

### Includes (Relations)
```
?include=skills,languages,media
```

### Search
```
?search=keyword
```

---

## Tadbeer Domain

### Workers

#### Create Worker
```http
POST /workers
```

**Permission:** `workers.create`

**Request:**
```typescript
interface CreateWorkerRequest {
  passportNumber: string;           // Required, unique per tenant
  emiratesId?: string;
  cvSerial?: string;
  fullNameEn: string;               // Required
  fullNameAr: string;               // Required
  nationality: string;              // Required
  dateOfBirth: string;              // ISO date (YYYY-MM-DD)
  gender: string;                   // "Female" | "Male"
  religion: string;
  maritalStatus: string;
  numberOfChildren?: number;
  education: string;
  yearsOfExperience?: number;
  jobCategoryId: string;            // UUID
  monthlyBaseSalary: number;
  isAvailableForFlexible: boolean;
  photoUrl?: string;
  videoUrl?: string;
  skills?: CreateWorkerSkillRequest[];
  languages?: CreateWorkerLanguageRequest[];
  notes?: string;
}

interface CreateWorkerSkillRequest {
  skillName: string;
  rating: number;                   // 0-100
}

interface CreateWorkerLanguageRequest {
  language: string;
  proficiency: string;              // "Poor" | "Fair" | "Fluent"
}
```

**Response:** `201 Created` → `WorkerDto`

---

#### Get Worker by ID
```http
GET /workers/{id}
```

**Permission:** `workers.view`

**Query Parameters:**
| Parameter | Description |
|-----------|-------------|
| `include` | `skills`, `languages`, `media`, `jobCategory` |

**Response:** `200 OK` → `WorkerDto`

```typescript
interface WorkerDto {
  id: string;
  cvSerial: string;
  passportNumber: string;
  emiratesId?: string;
  fullNameEn: string;
  fullNameAr: string;
  nationality: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  religion: string;
  maritalStatus: string;
  numberOfChildren?: number;
  education: string;
  currentStatus: string;
  passportLocation: string;
  isAvailableForFlexible: boolean;
  jobCategory?: JobCategoryRefDto;
  monthlyBaseSalary: number;
  yearsOfExperience?: number;
  photoUrl?: string;
  videoUrl?: string;
  skills?: WorkerSkillDto[];        // null = not included
  languages?: WorkerLanguageDto[];
  media?: WorkerMediaDto[];
  notes?: string;
  sharedFromTenantId?: string;
  createdAt: string;
  updatedAt: string;
}

interface WorkerRefDto {
  id: string;
  fullNameEn: string;
  cvSerial: string;
  nationality: string;
  status: string;
  photoUrl?: string;
}

interface WorkerSkillDto {
  id: string;
  skillName: string;
  rating: number;
}

interface WorkerLanguageDto {
  id: string;
  language: string;
  proficiency: string;
}

interface WorkerMediaDto {
  id: string;
  mediaType: string;
  fileUrl: string;
  isPrimary: boolean;
  uploadedAt: string;
}

interface JobCategoryRefDto {
  id: string;
  name: string;
  moHRECode: string;
}
```

---

#### Update Worker
```http
PATCH /workers/{id}
```

**Permission:** `workers.update`

**Request:**
```typescript
interface UpdateWorkerRequest {
  fullNameEn?: string;
  fullNameAr?: string;
  emiratesId?: string;
  religion?: string;
  maritalStatus?: string;
  numberOfChildren?: number;
  education?: string;
  yearsOfExperience?: number;
  jobCategoryId?: string;
  monthlyBaseSalary?: number;
  isAvailableForFlexible?: boolean;
  photoUrl?: string;
  videoUrl?: string;
  notes?: string;
}
```

**Response:** `200 OK` → `WorkerDto`

---

#### List Workers
```http
GET /workers
```

**Permission:** `workers.view`

**Filter Parameters:**
| Filter | Type | Description |
|--------|------|-------------|
| `filter[status]` | string[] | Worker statuses |
| `filter[nationality]` | string[] | Nationalities |
| `filter[jobCategoryId]` | string | Job category UUID |
| `filter[passportLocation]` | string | Passport location |
| `filter[isAvailableForFlexible]` | boolean | Flexible availability |
| `filter[createdAt][gte]` | date | Created after |
| `filter[createdAt][lt]` | date | Created before |

**Sort Options:** `createdAt`, `monthlyBaseSalary`, `fullNameEn`

**Response:** `200 OK` → `PagedList<WorkerDto>`

---

#### Transition Worker State
```http
POST /workers/{id}/transition
```

**Permission:** `workers.manage`

**Request:**
```typescript
interface WorkerStateTransitionRequest {
  targetState: string;
  reason?: string;
  relatedEntityId?: string;         // e.g., contractId for "Hired"
}
```

**Valid States:** `Draft`, `InTraining`, `ReadyForMarket`, `Reserved`, `Hired`, `OnLeave`, `Terminated`, `MedicallyUnfit`, `Absconded`, `Deported`

**Response:** `200 OK` → `WorkerDto`

---

#### Get Valid Transitions
```http
GET /workers/{id}/valid-transitions
```

**Permission:** `workers.view`

**Response:** `200 OK` → `string[]`

---

#### Get Worker State History
```http
GET /workers/{id}/history
```

**Permission:** `workers.view`

**Response:** `200 OK` → `PagedList<WorkerStateHistoryDto>`

```typescript
interface WorkerStateHistoryDto {
  id: string;
  fromStatus: string;
  toStatus: string;
  reason?: string;
  triggeredByUserId?: string;
  relatedEntityId?: string;
  occurredAt: string;
}
```

---

#### Get Passport Custody
```http
GET /workers/{id}/passport-custody
```

**Permission:** `workers.passport.view`

**Response:** `200 OK` → `PassportCustodyDto`

```typescript
interface PassportCustodyDto {
  id: string;
  location: string;
  handedToName?: string;
  handedToEntityId?: string;
  handedAt?: string;
  receivedAt?: string;
  notes?: string;
}
```

---

#### Get Passport Custody History
```http
GET /workers/{id}/passport-custody/history
```

**Permission:** `workers.passport.view`

**Response:** `200 OK` → `PagedList<PassportCustodyDto>`

---

#### Transfer Passport
```http
POST /workers/{id}/passport-transfer
```

**Permission:** `workers.passport.manage`

**Request:**
```typescript
interface TransferPassportRequest {
  location: string;
  handedToName?: string;
  handedToEntityId?: string;
  notes?: string;
}
```

**Response:** `201 Created` → `PassportCustodyDto`

---

#### Add/Update Worker Skill
```http
PUT /workers/{id}/skills/{skillName}
```

**Permission:** `workers.update`

**Request Body:** `number` (rating 0-100)

**Response:** `200 OK` → `WorkerSkillDto`

---

#### Remove Worker Skill
```http
DELETE /workers/{id}/skills/{skillName}
```

**Permission:** `workers.update`

**Response:** `204 No Content`

---

#### Add/Update Worker Language
```http
PUT /workers/{id}/languages/{language}
```

**Permission:** `workers.update`

**Request Body:** `string` (proficiency: "Poor", "Fair", "Fluent")

**Response:** `200 OK` → `WorkerLanguageDto`

---

#### Remove Worker Language
```http
DELETE /workers/{id}/languages/{language}
```

**Permission:** `workers.update`

**Response:** `204 No Content`

---

#### Add Worker Media
```http
POST /workers/{id}/media
```

**Permission:** `workers.update`

**Request:**
```typescript
interface AddWorkerMediaRequest {
  mediaType: string;               // "Photo" | "Video" | "Document"
  fileUrl: string;
  isPrimary: boolean;
}
```

**Response:** `201 Created` → `WorkerMediaDto`

---

#### Remove Worker Media
```http
DELETE /workers/{id}/media/{mediaId}
```

**Permission:** `workers.update`

**Response:** `204 No Content`

---

#### Set Primary Media
```http
POST /workers/{id}/media/{mediaId}/set-primary
```

**Permission:** `workers.update`

**Response:** `200 OK` → `WorkerMediaDto`

---

### Clients

#### Register Client
```http
POST /clients
```

**Permission:** `clients.create`

**Request:**
```typescript
interface CreateClientRequest {
  emiratesId: string;               // Required
  fullNameEn: string;               // Required
  fullNameAr: string;               // Required
  passportNumber?: string;
  nationality: string;              // Required
  phone?: string;
  email?: string;
  emirate?: string;
  categoryOverride?: string;        // Requires clients.manage
  salaryCertificateUrl?: string;
  ejariUrl?: string;
  notes?: string;
}
```

**Response:** `201 Created` → `ClientDto`

---

#### Get Client by ID
```http
GET /clients/{id}
```

**Permission:** `clients.view`

**Query Parameters:**
| Parameter | Description |
|-----------|-------------|
| `include` | `documents`, `discountCards` |

**Response:** `200 OK` → `ClientDto`

```typescript
interface ClientDto {
  id: string;
  emiratesId: string;
  fullNameEn: string;
  fullNameAr: string;
  passportNumber?: string;
  nationality: string;
  category: string;                 // "Local" | "Expat" | "Investor" | "VIP"
  phone?: string;
  email?: string;
  sponsorFileStatus: string;        // "Open" | "Pending" | "Active" | "Blocked"
  emirate?: string;
  isVerified: boolean;
  verifiedAt?: string;
  blockedReason?: string;
  notes?: string;
  createdAt: string;
  documents?: ClientDocumentDto[];
  discountCards?: DiscountCardDto[];
}

interface ClientRefDto {
  id: string;
  name: string;
  emiratesId: string;
  category: string;
}

interface ClientDocumentDto {
  id: string;
  documentType: string;
  fileUrl: string;
  fileName: string;
  expiresAt?: string;
  isVerified: boolean;
  uploadedAt: string;
}

interface DiscountCardDto {
  id: string;
  cardType: string;
  cardNumber: string;
  discountPercentage: number;
  validUntil?: string;
}
```

---

#### Update Client
```http
PATCH /clients/{id}
```

**Permission:** `clients.update`

**Request:**
```typescript
interface UpdateClientRequest {
  fullNameEn?: string;
  fullNameAr?: string;
  passportNumber?: string;
  nationality?: string;
  phone?: string;
  email?: string;
  emirate?: string;
  notes?: string;
}
```

**Response:** `200 OK` → `ClientDto`

---

#### Verify Client
```http
POST /clients/{id}/verify
```

**Permission:** `clients.manage`

**Response:** `200 OK` → `ClientDto`

---

#### Block Client
```http
POST /clients/{id}/block
```

**Permission:** `clients.manage`

**Request:**
```typescript
interface BlockClientRequest {
  reason: string;
}
```

**Response:** `200 OK` → `ClientDto`

---

#### Unblock Client
```http
POST /clients/{id}/unblock
```

**Permission:** `clients.manage`

**Response:** `200 OK` → `ClientDto`

---

#### List Clients
```http
GET /clients
```

**Permission:** `clients.view`

**Filter Parameters:**
| Filter | Type | Description |
|--------|------|-------------|
| `filter[category]` | string[] | Client categories |
| `filter[sponsorFileStatus]` | string | File status |
| `filter[isVerified]` | boolean | Verification status |
| `filter[nationality]` | string | Nationality |
| `filter[emirate]` | string | Emirate |
| `filter[createdAt][gte]` | date | Created after |

**Response:** `200 OK` → `PagedList<ClientDto>`

---

#### Search Clients
```http
GET /clients/search
```

**Permission:** `clients.view`

**Query Parameters:**
| Parameter | Description |
|-----------|-------------|
| `q` | Search query (min 2 chars) |

**Response:** `200 OK` → `PagedList<ClientRefDto>`

---

#### Get Client Documents
```http
GET /clients/{id}/documents
```

**Permission:** `clients.view`

**Response:** `200 OK` → `ClientDocumentDto[]`

---

#### Add Document
```http
POST /clients/{id}/documents
```

**Permission:** `clients.update`

**Request:**
```typescript
interface AddDocumentRequest {
  documentType: string;             // "EmiratesId" | "Passport" | "SalaryCertificate" | "EjariContract" | "TenancyContract" | "Other"
  fileUrl: string;
  fileName: string;
  expiresAt?: string;
}
```

**Response:** `201 Created` → `ClientDocumentDto`

---

#### Verify Document
```http
POST /clients/{id}/documents/{documentId}/verify
```

**Permission:** `clients.manage`

**Response:** `200 OK` → `ClientDocumentDto`

---

#### Delete Document
```http
DELETE /clients/{id}/documents/{documentId}
```

**Permission:** `clients.manage`

**Response:** `204 No Content`

---

#### Get Communications
```http
GET /clients/{id}/communications
```

**Permission:** `clients.view`

**Response:** `200 OK` → `PagedList<CommunicationLogDto>`

```typescript
interface CommunicationLogDto {
  id: string;
  channel: string;                  // "Phone" | "WhatsApp" | "Email" | "WalkIn"
  direction: string;                // "Inbound" | "Outbound"
  summary: string;
  loggedBy?: UserRefDto;
  occurredAt: string;
}
```

---

#### Add Communication
```http
POST /clients/{id}/communications
```

**Permission:** `clients.update`

**Request:**
```typescript
interface AddCommunicationRequest {
  channel: string;
  direction: string;
  summary: string;
}
```

**Response:** `201 Created` → `CommunicationLogDto`

---

#### Get Discount Cards
```http
GET /clients/{id}/discount-cards
```

**Permission:** `clients.view`

**Response:** `200 OK` → `DiscountCardDto[]`

---

#### Add Discount Card
```http
POST /clients/{id}/discount-cards
```

**Permission:** `clients.update`

**Request:**
```typescript
interface AddDiscountCardRequest {
  cardType: string;
  cardNumber: string;
  discountPercentage: number;
  validUntil?: string;
}
```

**Response:** `201 Created` → `DiscountCardDto`

---

### Leads

#### Create Lead
```http
POST /leads
```

**Permission:** `leads.create`

**Request:**
```typescript
interface CreateLeadRequest {
  source: string;                   // "WalkIn" | "Phone" | "Online" | "Referral" | "SocialMedia"
  contactName?: string;
  contactPhone?: string;
  contactEmail?: string;
  notes?: string;
  assignedToUserId?: string;
}
```

**Response:** `201 Created` → `LeadDto`

---

#### Get Lead by ID
```http
GET /leads/{id}
```

**Permission:** `leads.view`

**Query Parameters:**
| Parameter | Description |
|-----------|-------------|
| `include` | `client` (for converted leads) |

**Response:** `200 OK` → `LeadDto`

```typescript
interface LeadDto {
  id: string;
  client?: ClientRefDto;            // Only for converted leads
  source: string;
  status: string;                   // "New" | "Contacted" | "Qualified" | "Converted" | "Lost"
  notes?: string;
  assignedTo?: UserRefDto;
  contactName?: string;
  contactPhone?: string;
  contactEmail?: string;
  createdAt: string;
  updatedAt: string;
}

interface LeadRefDto {
  id: string;
  status: string;
  source: string;
}

interface UserRefDto {
  id: string;
  name: string;
  email?: string;
}
```

---

#### Update Lead
```http
PATCH /leads/{id}
```

**Permission:** `leads.update`

**Request:**
```typescript
interface UpdateLeadRequest {
  status?: string;
  notes?: string;
  assignedToUserId?: string;
  contactName?: string;
  contactPhone?: string;
  contactEmail?: string;
}
```

**Response:** `200 OK` → `LeadDto`

---

#### Convert Lead to Client
```http
POST /leads/{id}/convert
```

**Permissions:** `leads.manage`, `clients.create`

**Request:**
```typescript
interface ConvertLeadRequest {
  client: CreateClientRequest;
}
```

**Response:** `200 OK` → `ClientDto`

---

#### List Leads
```http
GET /leads
```

**Permission:** `leads.view`

**Filter Parameters:**
| Filter | Type | Description |
|--------|------|-------------|
| `filter[status]` | string[] | Lead statuses |
| `filter[source]` | string | Lead source |
| `filter[assignedToUserId]` | string | Assigned agent |
| `filter[createdAt][gte]` | date | Created after |

**Response:** `200 OK` → `PagedList<LeadDto>`

---

#### Get Lead Funnel Stats
```http
GET /leads/stats/funnel
```

**Permission:** `leads.view`

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `from` | datetime | Start date (optional) |
| `to` | datetime | End date (optional) |

**Response:** `200 OK` → `LeadFunnelStats`

```typescript
interface LeadFunnelStats {
  new: number;
  contacted: number;
  qualified: number;
  converted: number;
  lost: number;
  conversionRate: number;
}
```

---

## Tenant Management

### Tenants

#### List User's Tenants
```http
GET /tenants
```

**Response:** `200 OK` → `PagedList<TenantDto>`

```typescript
interface TenantDto {
  id: string;
  name: string;
  slug: string;
  status: TenantStatus;             // "Active" | "Suspended" | "Deleted"
  tenantTypeId?: string;
  tenantTypeName?: string;
  logoUrl?: string;
  description?: string;
  website?: string;
  createdAt: string;
  updatedAt: string;
}

enum TenantStatus {
  Active = 0,
  Suspended = 1,
  Deleted = 2
}
```

---

#### Get Tenant by ID
```http
GET /tenants/{id}
```

**Requirements:** Must be a tenant member

**Response:** `200 OK` → `TenantDto`

---

#### Get Tenant by Slug
```http
GET /tenants/by-slug/{slug}
```

**Requirements:** Must be a tenant member

**Response:** `200 OK` → `TenantDto`

---

#### Create Tenant
```http
POST /tenants
```

**Request:**
```typescript
interface CreateTenantRequest {
  name: string;                     // Required, max 200 chars
  slug?: string;                    // Auto-generated if not provided
  tenantTypeId?: string;
  logoUrl?: string;
  description?: string;             // Max 1000 chars
  website?: string;
}
```

**Response:** `201 Created` → `TenantDto`

---

#### Update Tenant
```http
PATCH /tenants/{id}
```

**Requirements:** Admin or Owner role

**Request:**
```typescript
interface UpdateTenantRequest {
  name?: string;
  tenantTypeId?: string;
  logoUrl?: string;
  description?: string;
  website?: string;
}
```

**Response:** `200 OK` → `TenantDto`

---

#### Suspend Tenant
```http
POST /tenants/{id}/suspend
```

**Role:** `platform-admin`

**Response:** `204 No Content`

---

#### Reactivate Tenant
```http
POST /tenants/{id}/reactivate
```

**Role:** `platform-admin`

**Response:** `204 No Content`

---

#### Delete Tenant
```http
DELETE /tenants/{id}
```

**Requirements:** Owner role only

**Response:** `204 No Content`

---

### Tenant Members

#### List Members
```http
GET /tenants/{tenantId}/members
```

**Response:** `200 OK` → `PagedList<TenantUserDto>`

```typescript
interface TenantUserDto {
  id: string;
  tenantId: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  role: TenantRole;                 // "Member" | "Admin" | "Owner"
  joinedAt: string;
}

enum TenantRole {
  Member = 0,
  Admin = 1,
  Owner = 2
}
```

---

#### Get Member
```http
GET /tenants/{tenantId}/members/{userId}
```

**Response:** `200 OK` → `TenantUserDto`

---

#### Update Member Role
```http
PATCH /tenants/{tenantId}/members/{userId}
```

**Requirements:** Admin or Owner role

**Request:**
```typescript
interface UpdateMemberRoleRequest {
  role: TenantRole;
}
```

**Response:** `200 OK` → `TenantUserDto`

---

#### Remove Member
```http
DELETE /tenants/{tenantId}/members/{userId}
```

**Requirements:** Admin/Owner role or self-removal

**Response:** `204 No Content`

---

### Tenant Invitations

#### List Invitations
```http
GET /tenants/{tenantId}/invitations
```

**Requirements:** Admin or Owner role

**Response:** `200 OK` → `PagedList<TenantInvitationDto>`

```typescript
interface TenantInvitationDto {
  id: string;
  tenantId: string;
  tenantName: string;
  email: string;
  role: TenantRole;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  invitedByName: string;
  isExpired: boolean;
  isAccepted: boolean;
  createdAt: string;
}
```

---

#### Create Invitation
```http
POST /tenants/{tenantId}/invitations
```

**Requirements:** Admin or Owner role

**Request:**
```typescript
interface InviteMemberRequest {
  email: string;
  role: TenantRole;
}
```

**Response:** `201 Created` → `TenantInvitationDto`

---

#### Revoke Invitation
```http
DELETE /tenants/{tenantId}/invitations/{invitationId}
```

**Requirements:** Admin or Owner role

**Response:** `204 No Content`

---

#### Get Invitation by Token (Public)
```http
GET /invitations/{token}
```

**Response:** `200 OK` → `TenantInvitationDto`

---

#### Accept Invitation
```http
POST /invitations/{token}/accept
```

**Requirements:** Authenticated user

**Response:** `200 OK` → `TenantUserDto`

---

### Roles

#### List Roles
```http
GET /tenants/{tenantId}/roles
```

**Permission:** `roles.view`

**Response:** `200 OK` → `PagedList<RoleDto>`

```typescript
interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  isDefault: boolean;
  isSystem: boolean;
  displayOrder: number;
  permissions: PermissionDto[];
  createdAt: string;
  updatedAt: string;
}

interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  module: string;
  displayOrder: number;
}
```

---

#### Get Role
```http
GET /tenants/{tenantId}/roles/{roleId}
```

**Permission:** `roles.view`

**Response:** `200 OK` → `RoleDto`

---

#### Create Role
```http
POST /tenants/{tenantId}/roles
```

**Permission:** `roles.manage`

**Request:**
```typescript
interface CreateRoleRequest {
  name: string;
  description?: string;
  permissionIds: string[];
}
```

**Response:** `201 Created` → `RoleDto`

---

#### Update Role
```http
PATCH /tenants/{tenantId}/roles/{roleId}
```

**Permission:** `roles.manage`

**Request:**
```typescript
interface UpdateRoleRequest {
  name?: string;
  description?: string;
  permissionIds?: string[];
}
```

**Response:** `200 OK` → `RoleDto`

---

#### Delete Role
```http
DELETE /tenants/{tenantId}/roles/{roleId}
```

**Permission:** `roles.delete`

**Response:** `204 No Content`

---

#### Get My Roles
```http
GET /tenants/{tenantId}/roles/my-roles
```

**Response:** `200 OK` → `RoleDto[]`

---

#### Assign Role
```http
POST /tenants/{tenantId}/roles/assign
```

**Permission:** `roles.assign`

**Request:**
```typescript
interface AssignRoleRequest {
  userId: string;
  roleId: string;
}
```

**Response:** `200 OK` → `UserRoleDto`

```typescript
interface UserRoleDto {
  id: string;
  tenantId: string;
  userId: string;
  userEmail: string;
  userName: string;
  roleId: string;
  roleName: string;
  assignedAt: string;
  assignedByUserId?: string;
}
```

---

#### Remove Role from User
```http
DELETE /tenants/{tenantId}/roles/users/{userId}/roles/{roleId}
```

**Permission:** `roles.assign`

**Response:** `204 No Content`

---

## Platform

### Users

#### Get Current User
```http
GET /users/me
```

**Notes:** JIT provisioning - creates profile if doesn't exist

**Response:** `200 OK` → `UserProfileDto`

```typescript
interface UserProfileDto {
  id: string;
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  phone?: string;
  locale: string;
  defaultTenantId?: string;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt: string;
}
```

---

#### Update Current User
```http
PATCH /users/me
```

**Request:**
```typescript
interface UpdateUserProfileRequest {
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  phone?: string;
  locale?: string;
  defaultTenantId?: string;
}
```

**Response:** `200 OK` → `UserProfileDto`

---

#### List Users (Admin)
```http
GET /users
```

**Roles:** `platform-admin`, `tenant-admin`

**Response:** `200 OK` → `PagedList<UserProfileDto>`

---

#### Get User by ID (Admin)
```http
GET /users/{id}
```

**Roles:** `platform-admin`, `tenant-admin`

**Response:** `200 OK` → `UserProfileDto`

---

#### Create User (Admin)
```http
POST /users
```

**Role:** `platform-admin`

**Request:**
```typescript
interface CreateUserProfileRequest {
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  phone?: string;
  locale?: string;
  defaultTenantId?: string;
}
```

**Response:** `201 Created` → `UserProfileDto`

---

#### Update User (Admin)
```http
PATCH /users/{id}
```

**Role:** `platform-admin`

**Response:** `200 OK` → `UserProfileDto`

---

#### Deactivate User
```http
POST /users/{id}/deactivate
```

**Role:** `platform-admin`

**Response:** `204 No Content`

---

#### Reactivate User
```http
POST /users/{id}/reactivate
```

**Role:** `platform-admin`

**Response:** `204 No Content`

---

### Permissions

#### List Permissions
```http
GET /permissions
```

**Response:** `200 OK` → `PagedList<PermissionDto>`

---

#### Get Permission
```http
GET /permissions/{id}
```

**Response:** `200 OK` → `PermissionDto`

---

### API Keys

#### List API Keys
```http
GET /tenants/{tenantId}/api-keys
```

**Permission:** `api.view`

**Response:** `200 OK` → `PagedList<ApiKeyDto>`

```typescript
interface ApiKeyDto {
  id: string;
  tenantId: string;
  name: string;
  prefix: string;
  permissions?: string[];
  expiresAt?: string;
  isActive: boolean;
  lastUsedAt?: string;
  requestCount: number;
  rateLimitPerMinute?: number;
  createdAt: string;
}

interface ApiKeyWithSecretDto extends ApiKeyDto {
  secret: string;                   // Only returned on creation
}
```

---

#### Get API Key
```http
GET /tenants/{tenantId}/api-keys/{apiKeyId}
```

**Permission:** `api.view`

**Response:** `200 OK` → `ApiKeyDto`

---

#### Create API Key
```http
POST /tenants/{tenantId}/api-keys
```

**Permission:** `api.manage`

**Request:**
```typescript
interface CreateApiKeyRequest {
  name: string;
  permissions?: string[];
  expiresAt?: string;
  rateLimitPerMinute?: number;
}
```

**Response:** `201 Created` → `ApiKeyWithSecretDto`

---

#### Revoke API Key
```http
DELETE /tenants/{tenantId}/api-keys/{apiKeyId}
```

**Permission:** `api.manage`

**Response:** `204 No Content`

---

#### Get API Key Logs
```http
GET /tenants/{tenantId}/api-keys/{apiKeyId}/logs
```

**Permission:** `api.view`

**Response:** `200 OK` → `PagedList<ApiKeyLogDto>`

```typescript
interface ApiKeyLogDto {
  id: string;
  apiKeyId: string;
  endpoint: string;
  method: string;
  statusCode: number;
  durationMs: number;
  ipAddress?: string;
  createdAt: string;
}
```

---

### Plans

#### List Plans (Public)
```http
GET /plans
```

**Filter:** `filter[isActive]=true`  
**Sort:** `displayOrder`

**Response:** `200 OK` → `PagedList<PlanDto>`

```typescript
interface PlanDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  isActive: boolean;
  isDefault: boolean;
  displayOrder: number;
  prices: PlanPriceDto[];
  features: PlanFeatureDto[];
  createdAt: string;
}

interface PlanPriceDto {
  id: string;
  planId: string;
  amount: number;                   // In cents
  currency: string;
  interval: string;                 // "month" | "year"
  intervalCount: number;
  trialDays: number;
  isActive: boolean;
  formattedPrice: string;           // e.g., "$19.99/month"
}

interface PlanFeatureDto {
  id: string;
  key: string;
  name: string;
  description?: string;
  valueType: string;                // "boolean" | "numeric"
  booleanValue?: boolean;
  numericValue?: number;
  isUnlimited: boolean;
  displayOrder: number;
  displayValue: string;
}
```

---

#### Get Plan by ID
```http
GET /plans/{planId}
```

**Response:** `200 OK` → `PlanDto`

---

#### Get Plan by Slug
```http
GET /plans/by-slug/{slug}
```

**Response:** `200 OK` → `PlanDto`

---

### Subscriptions

#### Get Subscription
```http
GET /tenants/{tenantId}/subscription
```

**Permission:** `billing.view`

**Response:** `200 OK` → `TenantSubscriptionDto`

```typescript
interface TenantSubscriptionDto {
  id: string;
  tenantId: string;
  planId: string;
  planName: string;
  planSlug: string;
  planPriceId: string;
  status: string;                   // "active" | "trialing" | "canceled" | "past_due"
  currentPeriodStart: string;
  currentPeriodEnd: string;
  trialEnd?: string;
  canceledAt?: string;
  cancelAtPeriodEnd: boolean;
  createdAt: string;
  isTrialing: boolean;
  isActive: boolean;
}
```

---

#### Create Checkout Session
```http
POST /tenants/{tenantId}/subscription/checkout
```

**Permission:** `billing.manage`

**Request:**
```typescript
interface CreateCheckoutRequest {
  planId: string;
  planPriceId: string;
  successUrl?: string;
  cancelUrl?: string;
}
```

**Response:** `200 OK` → `CheckoutSessionDto`

```typescript
interface CheckoutSessionDto {
  id: string;
  stripeSessionId: string;
  status: string;
  url?: string;
  expiresAt: string;
}
```

---

#### Cancel Subscription
```http
POST /tenants/{tenantId}/subscription/cancel
```

**Permission:** `billing.manage`

**Request:**
```typescript
interface CancelSubscriptionRequest {
  cancelImmediately?: boolean;      // Default: false (cancel at period end)
}
```

**Response:** `200 OK` → `TenantSubscriptionDto`

---

#### Resume Subscription
```http
POST /tenants/{tenantId}/subscription/resume
```

**Permission:** `billing.manage`

**Response:** `200 OK` → `TenantSubscriptionDto`

---

### Credits

#### Get Credit Balance
```http
GET /tenants/{tenantId}/credits/balance
```

**Permission:** `billing.view`

**Response:** `200 OK` → `CreditBalanceDto`

```typescript
interface CreditBalanceDto {
  tenantId: string;
  balance: number;
  expiringWithin30Days: number;
  nextExpiration?: string;
}
```

---

#### Get Credit History
```http
GET /tenants/{tenantId}/credits/history
```

**Permission:** `billing.view`

**Filter:** `filter[type]=purchase|spend|bonus`

**Response:** `200 OK` → `PagedList<CreditDto>`

```typescript
interface CreditDto {
  id: string;
  tenantId: string;
  type: string;                     // "purchase" | "spend" | "bonus" | "refund"
  amount: number;
  balance: number;
  description: string;
  referenceId?: string;
  referenceType?: string;
  userId?: string;
  expiresAt?: string;
  createdAt: string;
}
```

---

#### Add Credits
```http
POST /tenants/{tenantId}/credits/add
```

**Permission:** `billing.manage`

**Request:**
```typescript
interface AddCreditsRequest {
  amount: number;
  type?: string;                    // Default: "bonus"
  description: string;
  expiresAt?: string;
}
```

**Response:** `200 OK` → `CreditDto`

---

#### Spend Credits
```http
POST /tenants/{tenantId}/credits/spend
```

**Permission:** `billing.manage`

**Request:**
```typescript
interface SpendCreditsRequest {
  amount: number;
  description: string;
  referenceId?: string;
  referenceType?: string;
}
```

**Response:** `200 OK` → `CreditDto`

---

### Notifications

#### Get Notifications
```http
GET /tenants/{tenantId}/notifications
```

**Filter:** `filter[isRead]=false`, `filter[type]=info|warning|error`  
**Sort:** `-createdAt`

**Response:** `200 OK` → `PagedList<NotificationDto>`

```typescript
interface NotificationDto {
  id: string;
  tenantId: string;
  userId: string;
  title: string;
  body: string;
  type: string;                     // "info" | "warning" | "error" | "success"
  link?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}
```

---

#### Get Notification
```http
GET /tenants/{tenantId}/notifications/{notificationId}
```

**Response:** `200 OK` → `NotificationDto`

---

#### Get Unread Count
```http
GET /tenants/{tenantId}/notifications/unread-count
```

**Response:** `200 OK` → `UnreadCountDto`

```typescript
interface UnreadCountDto {
  count: number;
}
```

---

#### Mark as Read
```http
POST /tenants/{tenantId}/notifications/{notificationId}/read
```

**Response:** `200 OK` → `NotificationDto`

---

#### Mark All as Read
```http
POST /tenants/{tenantId}/notifications/read-all
```

**Response:** `200 OK` → `{ markedAsRead: number }`

---

#### Delete Notification
```http
DELETE /tenants/{tenantId}/notifications/{notificationId}
```

**Response:** `204 No Content`

---

### Audit

#### Get Audit Events
```http
GET /tenants/{tenantId}/audit/events
```

**Permission:** `analytics.view`

**Response:** `200 OK` → `PagedList<AuditEventDto>`

```typescript
interface AuditEventDto {
  id: string;
  eventName: string;
  payload?: string;
  userId?: string;
  createdAt: string;
}
```

---

#### Get Audit Logs
```http
GET /tenants/{tenantId}/audit/logs
```

**Permission:** `analytics.view`

**Response:** `200 OK` → `PagedList<AuditLogDto>`

```typescript
interface AuditLogDto {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  oldValues?: string;
  newValues?: string;
  userId?: string;
  createdAt: string;
}
```

---

### Webhooks

#### List Webhooks
```http
GET /tenants/{tenantId}/webhooks
```

**Permission:** `settings.view`

**Response:** `200 OK` → `PagedList<WebhookDto>`

```typescript
interface WebhookDto {
  id: string;
  url: string;
  events?: string[];
  isActive: boolean;
  lastTriggeredAt?: string;
  failureCount: number;
  createdAt: string;
}
```

---

#### Create Webhook
```http
POST /tenants/{tenantId}/webhooks
```

**Permission:** `settings.manage`

**Request:**
```typescript
interface CreateWebhookRequest {
  url: string;
  events?: string[];
}
```

**Response:** `201 Created` → `WebhookDto`

---

#### Delete Webhook
```http
DELETE /tenants/{tenantId}/webhooks/{webhookId}
```

**Permission:** `settings.manage`

**Response:** `204 No Content`

---

### Analytics

#### Get Page Views
```http
GET /tenants/{tenantId}/analytics/pageviews
```

**Permission:** `analytics.view`

**Response:** `200 OK` → `PagedList<PageViewDto>`

```typescript
interface PageViewDto {
  id: string;
  url: string;
  userId?: string;
  sessionId: string;
  referrer?: string;
  createdAt: string;
}
```

---

#### Get Events
```http
GET /tenants/{tenantId}/analytics/events
```

**Permission:** `analytics.view`

**Response:** `200 OK` → `PagedList<AnalyticsEventDto>`

```typescript
interface AnalyticsEventDto {
  id: string;
  name: string;
  properties?: string;
  userId?: string;
  createdAt: string;
}
```

---

#### Get Daily Stats
```http
GET /tenants/{tenantId}/analytics/daily
```

**Permission:** `analytics.view`

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `from` | date | Start date (default: 30 days ago) |
| `to` | date | End date (default: today) |

**Response:** `200 OK` → `DailyStatsDto[]`

```typescript
interface DailyStatsDto {
  date: string;
  pageViews: number;
  uniqueVisitors: number;
  sessions: number;
  events: number;
}
```

---

#### Track Event
```http
POST /tenants/{tenantId}/analytics/track
```

**Request:**
```typescript
interface TrackEventRequest {
  name: string;
  properties?: Record<string, any>;
  sessionId?: string;
}
```

**Response:** `204 No Content`

---

#### Track Page View
```http
POST /tenants/{tenantId}/analytics/pageview
```

**Request:**
```typescript
interface TrackPageViewRequest {
  url: string;
  sessionId: string;
  referrer?: string;
  userAgent?: string;
}
```

**Response:** `204 No Content`

---

### Feature Flags

#### List Feature Flags
```http
GET /feature-flags
```

**Response:** `200 OK` → `PagedList<FeatureFlagDto>`

```typescript
interface FeatureFlagDto {
  id: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  percentage?: number;
  createdAt: string;
}
```

---

#### Get Feature Flag
```http
GET /feature-flags/{name}
```

**Response:** `200 OK` → `FeatureFlagDto`

---

#### Create Feature Flag
```http
POST /feature-flags
```

**Request:**
```typescript
interface CreateFeatureFlagRequest {
  name: string;
  description?: string;
  isEnabled: boolean;
  percentage?: number;
}
```

**Response:** `201 Created` → `FeatureFlagDto`

---

#### Update Feature Flag
```http
PUT /feature-flags/{flagId}
```

**Request:**
```typescript
interface UpdateFeatureFlagRequest {
  description?: string;
  isEnabled?: boolean;
  percentage?: number;
}
```

**Response:** `200 OK` → `FeatureFlagDto`

---

#### Delete Feature Flag
```http
DELETE /feature-flags/{flagId}
```

**Response:** `204 No Content`

---

#### Evaluate Feature Flag
```http
GET /feature-flags/{name}/evaluate
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `planSlug` | string | Plan slug for plan-based evaluation |

**Response:** `200 OK` → `EvaluationResult`

```typescript
interface EvaluationResult {
  flagName: string;
  isEnabled: boolean;
  reason?: string;
}
```

---

### Portals

#### List Portals
```http
GET /tenants/{tenantId}/portals
```

**Permission:** `portal.view`

**Response:** `200 OK` → `PagedList<PortalDto>`

```typescript
interface PortalDto {
  id: string;
  tenantId: string;
  name: string;
  subdomain: string;
  description?: string;
  isActive: boolean;
  primaryColor?: string;
  secondaryColor?: string;
  logoUrl?: string;
  faviconUrl?: string;
  seoTitle?: string;
  seoDescription?: string;
  allowPublicRegistration: boolean;
  requireEmailVerification: boolean;
  enableSso: boolean;
  userCount: number;
  pageCount: number;
  createdAt: string;
  updatedAt: string;
  url: string;
}
```

---

#### Get Portal
```http
GET /tenants/{tenantId}/portals/{portalId}
```

**Permission:** `portal.view`

**Response:** `200 OK` → `PortalDto`

---

#### Create Portal
```http
POST /tenants/{tenantId}/portals
```

**Permission:** `portal.manage`

**Request:**
```typescript
interface CreatePortalRequest {
  name: string;
  subdomain: string;
  description?: string;
  primaryColor?: string;
  secondaryColor?: string;
  allowPublicRegistration?: boolean;
}
```

**Response:** `201 Created` → `PortalDto`

---

#### Update Portal
```http
PATCH /tenants/{tenantId}/portals/{portalId}
```

**Permission:** `portal.manage`

**Request:**
```typescript
interface UpdatePortalRequest {
  name?: string;
  description?: string;
  isActive?: boolean;
  primaryColor?: string;
  secondaryColor?: string;
  logoUrl?: string;
  faviconUrl?: string;
  seoTitle?: string;
  seoDescription?: string;
  allowPublicRegistration?: boolean;
  requireEmailVerification?: boolean;
}
```

**Response:** `200 OK` → `PortalDto`

---

#### Delete Portal
```http
DELETE /tenants/{tenantId}/portals/{portalId}
```

**Permission:** `portal.delete`

**Response:** `204 No Content`

---

#### Add Custom Domain
```http
POST /tenants/{tenantId}/portals/{portalId}/domains
```

**Permission:** `portal.manage`

**Request:**
```typescript
interface AddDomainRequest {
  domain: string;
}
```

**Response:** `201 Created` → `PortalDomainDto`

```typescript
interface PortalDomainDto {
  id: string;
  portalId: string;
  domain: string;
  isPrimary: boolean;
  isVerified: boolean;
  verificationToken?: string;
  verifiedAt?: string;
  sslStatus: string;                // "pending" | "active" | "failed"
}
```

---

#### Verify Domain
```http
POST /tenants/{tenantId}/portals/{portalId}/domains/{domainId}/verify
```

**Permission:** `portal.manage`

**Response:** `200 OK` → `PortalDomainDto`

---

#### Remove Domain
```http
DELETE /tenants/{tenantId}/portals/{portalId}/domains/{domainId}
```

**Permission:** `portal.manage`

**Response:** `204 No Content`

---

## Appendix

### Permission List

| Permission | Description |
|------------|-------------|
| `workers.view` | View workers |
| `workers.create` | Create workers |
| `workers.update` | Update workers |
| `workers.manage` | Manage worker states |
| `workers.passport.view` | View passport custody |
| `workers.passport.manage` | Transfer passport custody |
| `clients.view` | View clients |
| `clients.create` | Create clients |
| `clients.update` | Update clients |
| `clients.manage` | Verify/block clients |
| `leads.view` | View leads |
| `leads.create` | Create leads |
| `leads.update` | Update leads |
| `leads.manage` | Convert leads |
| `roles.view` | View roles |
| `roles.manage` | Create/update roles |
| `roles.delete` | Delete roles |
| `roles.assign` | Assign/remove roles |
| `api.view` | View API keys |
| `api.manage` | Create/revoke API keys |
| `billing.view` | View subscription/credits |
| `billing.manage` | Manage billing |
| `analytics.view` | View analytics/audit |
| `settings.view` | View settings |
| `settings.manage` | Manage settings |
| `portal.view` | View portals |
| `portal.manage` | Manage portals |
| `portal.delete` | Delete portals |
