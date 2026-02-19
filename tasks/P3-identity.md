# Phase 3: Identity Module

First module. Syncs users from Keycloak, maintains UserProfile, provides ICurrentUser. Establishes the pattern all other modules follow.

**Dependencies:** P2 complete

---

## P3-T01: Create Identity.Contracts

### Dependencies
P1-T03

### Files to Create
```
src/Modules/Identity/Identity.Contracts/IIdentityService.cs
src/Modules/Identity/Identity.Contracts/DTOs/UserProfileDto.cs
src/Modules/Identity/Identity.Contracts/DTOs/CreateUserProfileRequest.cs
src/Modules/Identity/Identity.Contracts/DTOs/UpdateUserProfileRequest.cs
src/Modules/Identity/Identity.Contracts/DTOs/AdminUserDto.cs
```

### Instructions

```csharp
public interface IIdentityService
{
    Task<Result<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserProfileDto>> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<UserProfileDto>> CreateAsync(CreateUserProfileRequest request, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<PagedList<UserProfileDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);
}
```

ListAsync accepts QueryParameters so the controller passes parsed filters/sort/pagination directly.

### Tests
- [ ] Contract test: Interface compiles with expected signatures

### Acceptance Criteria
Other modules reference Identity.Contracts to call IIdentityService.

---

## P3-T02: Create Identity entities and EF configuration

### Dependencies
P2-T04, P3-T01

### Files to Create
```
src/Modules/Identity/Identity.Core/Entities/UserProfile.cs
src/Modules/Identity/Identity.Core/Entities/AdminUser.cs
src/Modules/Identity/Identity.Core/Persistence/UserProfileConfiguration.cs
src/Modules/Identity/Identity.Core/Persistence/AdminUserConfiguration.cs
```

### Instructions

1. **UserProfile** (NOT TenantScoped - global entity):
   ```csharp
   public class UserProfile : BaseEntity
   {
       public string KeycloakId { get; set; }  // unique
       public string Email { get; set; }       // unique
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public string? AvatarUrl { get; set; }
       public string? Phone { get; set; }
       public string Locale { get; set; } = "en";
       public Guid? DefaultTenantId { get; set; }
       public bool IsActive { get; set; } = true;
       public DateTimeOffset? LastLoginAt { get; set; }
   }
   ```

2. **AdminUser**:
   ```csharp
   public class AdminUser : BaseEntity
   {
       public Guid UserId { get; set; }
       public UserProfile User { get; set; }
       public bool IsSuperAdmin { get; set; }
   }
   ```

3. Migration: `dotnet ef migrations add InitIdentity`

### Tests
- [ ] Integration: Insert UserProfile, query by KeycloakId, verify returned

### Acceptance Criteria
UserProfile and AdminUser tables exist.

---

## P3-T03: Implement IdentityService with filter/sort support

### Dependencies
P3-T02, P2-T02

### Files to Create
```
src/Modules/Identity/Identity.Core/Services/IdentityService.cs
```

### Instructions

```csharp
public class IdentityService : IIdentityService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IClock _clock;
    
    // Filterable fields map
    private static readonly Dictionary<string, Expression<Func<UserProfile, object>>> FilterableFields = new()
    {
        ["email"] = x => x.Email,
        ["firstName"] = x => x.FirstName,
        ["lastName"] = x => x.LastName,
        ["isActive"] = x => x.IsActive,
        ["createdAt"] = x => x.CreatedAt
    };
    
    // Sortable fields map (same + more)
    private static readonly Dictionary<string, Expression<Func<UserProfile, object>>> SortableFields = new()
    {
        ["email"] = x => x.Email,
        ["firstName"] = x => x.FirstName,
        ["lastName"] = x => x.LastName,
        ["createdAt"] = x => x.CreatedAt,
        ["lastLoginAt"] = x => x.LastLoginAt
    };
    
    public async Task<PagedList<UserProfileDto>> ListAsync(QueryParameters qp, CancellationToken ct)
    {
        return await _db.UserProfiles
            .ApplyFilters(qp.GetFilters(), FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields)
            .Select(x => new UserProfileDto { ... })
            .ToPagedListAsync(qp.Page, qp.PageSize, ct);
    }
    
    public async Task<Result<UserProfileDto>> CreateAsync(CreateUserProfileRequest request, CancellationToken ct)
    {
        var entity = new UserProfile { ... };
        _db.UserProfiles.Add(entity);
        await _db.SaveChangesAsync(ct);
        
        await _publisher.Publish(new UserCreatedEvent(...), ct);
        
        return Result<UserProfileDto>.Success(new UserProfileDto { ... });
    }
}
```

### Tests
- [ ] Unit: CreateAsync publishes UserCreatedEvent
- [ ] Unit: ListAsync with filter[isActive]=true returns only active users
- [ ] Unit: ListAsync with sort=-createdAt returns newest first

### Acceptance Criteria
Full CRUD with filtering/sorting via shared extensions.

---

## P3-T04: Create Keycloak event consumer (user sync)

### Dependencies
P3-T03, P2-T07

### Files to Create
```
src/Modules/Identity/Identity.Core/Consumers/KeycloakUserCreatedConsumer.cs
src/Modules/Identity/Identity.Core/Consumers/KeycloakUserUpdatedConsumer.cs
```

### Instructions

1. Consume Keycloak events from RabbitMQ
2. Create/update UserProfile via IIdentityService
3. Idempotent: check KeycloakId exists before creating

### Tests
- [ ] Unit: Consumer creates UserProfile on new user event
- [ ] Unit: Consumer updates existing on duplicate

### Acceptance Criteria
User profiles auto-sync from Keycloak.

---

## P3-T05: Implement ICurrentUser from JWT claims

### Dependencies
P3-T01

### Files to Create
```
src/SaasKit.Infrastructure/Auth/CurrentUser.cs
src/SaasKit.Infrastructure/Auth/ClaimsPrincipalExtensions.cs
```

### Instructions

```csharp
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Guid UserId => _httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;
    public string Email => _httpContextAccessor.HttpContext?.User.GetEmail() ?? string.Empty;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    public IReadOnlyList<string> Roles => _httpContextAccessor.HttpContext?.User.GetRoles() ?? Array.Empty<string>();
}
```

### Tests
- [ ] Unit: Extracts UserId from sub claim
- [ ] Unit: IsAuthenticated false when no user

### Acceptance Criteria
Any service can inject ICurrentUser.

---

## P3-T06: Create Identity API controllers

### Dependencies
P3-T03, P3-T05, P2-T03

### Files to Create
```
src/SaasKit.Api/Controllers/UsersController.cs
src/SaasKit.Api/Controllers/AdminUsersController.cs
```

### Instructions

```csharp
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _identityService.GetByIdAsync(_currentUser.UserId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
        // Middleware wraps in { data, meta }
    }
    
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] QueryParameters qp)
    {
        var result = await _identityService.ListAsync(qp);
        return Ok(result);
        // Middleware wraps in { data, meta, pagination }
    }
}
```

Controllers return raw DTO/PagedList - middleware wraps in envelope.

### Tests
- [ ] Integration: GET /api/v1/users/me returns { data: {...}, meta: {...} }
- [ ] Integration: GET /api/v1/users?filter[isActive]=true returns only active users
- [ ] Integration: GET /api/v1/users?filter[email][contains]=@gmail returns matching users
- [ ] Integration: GET /api/v1/users?sort=email,-createdAt returns correctly sorted
- [ ] Integration: POST with invalid body returns 422 RFC 9457 format
- [ ] Integration: GET /api/v1/users/me without auth returns 401

### Acceptance Criteria
Identity endpoints follow all API conventions: envelope, filtering, sorting, pagination, error format.

---

## P3-T07: Create Identity module ServiceRegistration

### Dependencies
P3-T03, P3-T04

### Files to Create
```
src/Modules/Identity/Identity.Core/IdentityServiceRegistration.cs
```

### Instructions

```csharp
public static class IdentityServiceRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        
        // FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(IdentityServiceRegistration).Assembly);
        
        return services;
    }
}
```

Call from Program.cs: `builder.Services.AddIdentityModule();`

### Tests
- [ ] Build and run
- [ ] Identity endpoints work

### Acceptance Criteria
Identity module self-contained, one-line registration.

---

## Phase 3 Checklist

- [ ] P3-T01: Identity.Contracts created
- [ ] P3-T02: Identity entities and EF config
- [ ] P3-T03: IdentityService with filter/sort
- [ ] P3-T04: Keycloak event consumer
- [ ] P3-T05: ICurrentUser implementation
- [ ] P3-T06: Identity API controllers
- [ ] P3-T07: ServiceRegistration
- [ ] All endpoints follow API conventions
- [ ] Tests pass
