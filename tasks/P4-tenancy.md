# Phase 4: Tenancy Module

Multi-tenancy backbone. Manages tenants, memberships, invitations, tenant resolution middleware.

**Dependencies:** P3 complete

---

## P4-T01: Create Tenancy entities and EF configuration

### Dependencies
P1-T03, P2-T04

### Files to Create
```
src/Modules/Tenancy/Tenancy.Contracts/ITenantService.cs
src/Modules/Tenancy/Tenancy.Contracts/DTOs/*.cs
src/Modules/Tenancy/Tenancy.Core/Entities/Tenant.cs
src/Modules/Tenancy/Tenancy.Core/Entities/TenantUser.cs
src/Modules/Tenancy/Tenancy.Core/Entities/TenantUserInvitation.cs
src/Modules/Tenancy/Tenancy.Core/Entities/TenantType.cs
src/Modules/Tenancy/Tenancy.Core/Entities/TenantTypeRelationship.cs
src/Modules/Tenancy/Tenancy.Core/Entities/TenantRelationship.cs
src/Modules/Tenancy/Tenancy.Core/Persistence/*.cs
```

### Entities

1. **Tenant** : BaseEntity
   - Name, Slug (unique), Status (active/suspended/deleted)
   - TenantTypeId, Settings (JSONB), Branding

2. **TenantUser** : BaseEntity
   - TenantId, UserId, Role (owner/admin/member), JoinedAt

3. **TenantUserInvitation** : TenantScopedEntity
   - Email, Role, Token, ExpiresAt, AcceptedAt, InvitedByUserId

4. **TenantType** : BaseEntity
   - Name (e.g., Agency, Brand), AllowedChildTypes

5. **TenantTypeRelationship** / **TenantRelationship** for hierarchy

### ITenantService

```csharp
public interface ITenantService
{
    Task<PagedList<TenantDto>> ListUserTenantsAsync(Guid userId, QueryParameters qp, CancellationToken ct);
    Task<PagedList<TenantUserDto>> GetMembersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct);
    Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct);
    Task<Result<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct);
    // Invitation methods...
}
```

Filterable fields for ListAsync: status, type, name (contains), createdAt (range)
Filterable fields for GetMembersAsync: role, email (contains), joinedAt (range)

### Tests
- [ ] Integration: Create tenant, add member, query membership

### Acceptance Criteria
Tenancy tables exist with 6 entities.

---

## P4-T02: Implement TenantService with filters

### Dependencies
P4-T01, P3-T02, P2-T02

### Files to Create
```
src/Modules/Tenancy/Tenancy.Core/Services/TenantService.cs
```

### Instructions

```csharp
public class TenantService : ITenantService
{
    private static readonly Dictionary<string, Expression<Func<Tenant, object>>> TenantFilters = new()
    {
        ["status"] = x => x.Status,
        ["type"] = x => x.TenantTypeId,
        ["name"] = x => x.Name,
        ["createdAt"] = x => x.CreatedAt
    };
    
    private static readonly Dictionary<string, Expression<Func<TenantUser, object>>> MemberFilters = new()
    {
        ["role"] = x => x.Role,
        ["email"] = x => x.User.Email,
        ["joinedAt"] = x => x.JoinedAt
    };
    
    public async Task<PagedList<TenantUserDto>> GetMembersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct)
    {
        return await _db.TenantUsers
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.GetFilters(), MemberFilters)
            .ApplySort(qp.GetSortFields(), MemberSortable)
            .Select(x => new TenantUserDto { ... })
            .ToPagedListAsync(qp.Page, qp.PageSize, ct);
    }
    
    public async Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct)
    {
        var slug = request.Name.ToSlug();
        
        var tenant = new Tenant { Name = request.Name, Slug = slug, ... };
        _db.Tenants.Add(tenant);
        
        // Create owner membership
        var owner = new TenantUser { TenantId = tenant.Id, UserId = _currentUser.UserId, Role = "owner" };
        _db.TenantUsers.Add(owner);
        
        await _db.SaveChangesAsync(ct);
        await _publisher.Publish(new TenantCreatedEvent(...), ct);
        
        return Result<TenantDto>.Success(new TenantDto { ... });
    }
}
```

- `filter[role]=admin&filter[role]=owner` returns admins and owners (OR)
- `filter[email][contains]=gmail` returns Gmail users

### Tests
- [ ] Unit: filter[role]=admin&filter[role]=member returns both roles
- [ ] Unit: RemoveMemberAsync prevents removing last Owner

### Acceptance Criteria
Tenant CRUD and membership with filtering.

---

## P4-T03: Implement Tenant Resolution Middleware

### Dependencies
P4-T02, P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Tenancy/TenantResolutionMiddleware.cs
src/SaasKit.Infrastructure/Tenancy/TenantContext.cs
src/SaasKit.Infrastructure/Tenancy/Resolvers/SubdomainTenantResolver.cs
src/SaasKit.Infrastructure/Tenancy/Resolvers/HeaderTenantResolver.cs
src/SaasKit.Infrastructure/Tenancy/Resolvers/JwtClaimTenantResolver.cs
src/SaasKit.Infrastructure/Tenancy/TenantRequiredAttribute.cs
```

### Instructions

1. **Resolver chain**: subdomain → custom domain → X-Tenant-Id header → JWT tenant_id claim

2. **TenantContext** : ITenantContext (scoped):
   ```csharp
   public class TenantContext : ITenantContext
   {
       public Guid TenantId { get; set; }
       public string TenantSlug { get; set; }
       public bool IsResolved { get; set; }
   }
   ```

3. **[TenantRequired]** attribute for endpoints needing tenant

4. If required and not resolved → 400 ApiError

### Tests
- [ ] Unit: SubdomainResolver extracts 'acme' from 'acme.app.example.com'
- [ ] Integration: Request with X-Tenant-Id resolves correctly
- [ ] Integration: [TenantRequired] without tenant returns 400 RFC 9457

### Acceptance Criteria
Every request scoped to tenant. Missing tenant returns structured 400.

---

## P4-T04: Create Tenancy API and ServiceRegistration

### Dependencies
P4-T02, P4-T03, P2-T03

### Files to Create
```
src/SaasKit.Api/Controllers/TenantsController.cs
src/SaasKit.Api/Controllers/TenantMembersController.cs
src/SaasKit.Api/Controllers/TenantInvitationsController.cs
src/Modules/Tenancy/Tenancy.Core/TenancyServiceRegistration.cs
```

### Instructions

```csharp
[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] QueryParameters qp)
    {
        var result = await _tenantService.ListUserTenantsAsync(_currentUser.UserId, qp);
        return Ok(result);
    }
    
    [HttpGet("{id}/members")]
    [TenantRequired]
    public async Task<IActionResult> GetMembers(Guid id, [FromQuery] QueryParameters qp)
    {
        var result = await _tenantService.GetMembersAsync(id, qp);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var result = await _tenantService.CreateAsync(request);
        if (!result.IsSuccess) return Conflict(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}
```

### Tests
- [ ] Integration: POST creates tenant, GET returns it in envelope
- [ ] Integration: filter[role]=admin&filter[role]=member on members returns both roles
- [ ] Integration: Non-member gets 403

### Acceptance Criteria
Tenancy fully wired with API conventions.

---

## Phase 4 Checklist

- [ ] P4-T01: Tenancy entities created
- [ ] P4-T02: TenantService with filters
- [ ] P4-T03: Tenant Resolution Middleware
- [ ] P4-T04: Tenancy API and ServiceRegistration
- [ ] All endpoints follow API conventions
- [ ] Tests pass
