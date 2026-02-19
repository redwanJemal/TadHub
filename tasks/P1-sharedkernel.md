# Phase 1: SharedKernel

Build the foundation types that every module depends on: base entities, interfaces, domain event contracts, API contract types (response envelope, filters, pagination), and common extensions.

**Dependencies:** P0 complete

---

## P1-T01: Create base entity types

### Dependencies
P0-T01

### Files to Create
```
src/SaasKit.SharedKernel/Entities/BaseEntity.cs
src/SaasKit.SharedKernel/Entities/TenantScopedEntity.cs
src/SaasKit.SharedKernel/Entities/SoftDeletableEntity.cs
src/SaasKit.SharedKernel/Entities/IAuditable.cs
```

### Instructions

1. **BaseEntity** - Abstract base for all entities:
   ```csharp
   public abstract class BaseEntity
   {
       public Guid Id { get; set; } = Guid.NewGuid();
       public DateTimeOffset CreatedAt { get; set; }
       public DateTimeOffset UpdatedAt { get; set; }
   }
   ```

2. **TenantScopedEntity** - Base for 90% of entities:
   ```csharp
   public abstract class TenantScopedEntity : BaseEntity
   {
       public Guid TenantId { get; set; }
   }
   ```

3. **SoftDeletableEntity** - For soft delete support:
   ```csharp
   public abstract class SoftDeletableEntity : TenantScopedEntity
   {
       public bool IsDeleted { get; set; } = false;
       public DateTimeOffset? DeletedAt { get; set; }
   }
   ```

4. **IAuditable** - For tracking who created/updated:
   ```csharp
   public interface IAuditable
   {
       Guid? CreatedBy { get; set; }
       Guid? UpdatedBy { get; set; }
   }
   ```

### Tests
- [ ] Unit test: BaseEntity.Id is non-empty Guid
- [ ] Unit test: SoftDeletableEntity defaults IsDeleted to false

### Acceptance Criteria
All module entities can inherit from these base types.

---

## P1-T02: Create core interfaces

### Dependencies
P0-T01

### Files to Create
```
src/SaasKit.SharedKernel/Interfaces/ITenantContext.cs
src/SaasKit.SharedKernel/Interfaces/ICurrentUser.cs
src/SaasKit.SharedKernel/Interfaces/IClock.cs
src/SaasKit.SharedKernel/Interfaces/IUnitOfWork.cs
```

### Instructions

1. **ITenantContext** - Current tenant resolution:
   ```csharp
   public interface ITenantContext
   {
       Guid TenantId { get; }
       string TenantSlug { get; }
       bool IsResolved { get; }
   }
   ```

2. **ICurrentUser** - Current authenticated user:
   ```csharp
   public interface ICurrentUser
   {
       Guid UserId { get; }
       string Email { get; }
       bool IsAuthenticated { get; }
       IReadOnlyList<string> Roles { get; }
   }
   ```

3. **IClock** - Wraps system clock for testability:
   ```csharp
   public interface IClock
   {
       DateTimeOffset UtcNow { get; }
   }
   ```

4. **IUnitOfWork** - Unit of work pattern:
   ```csharp
   public interface IUnitOfWork
   {
       Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
   }
   ```

### Tests
- [ ] Unit test: IClock implementation returns current UTC time

### Acceptance Criteria
Interfaces referenced by all modules. No circular dependencies.

---

## P1-T03: Create domain event base and platform event contracts

### Dependencies
P0-T01

### Files to Create
```
src/SaasKit.SharedKernel/Events/IDomainEvent.cs
src/SaasKit.SharedKernel/Events/TenantCreatedEvent.cs
src/SaasKit.SharedKernel/Events/TenantUpdatedEvent.cs
src/SaasKit.SharedKernel/Events/TenantDeletedEvent.cs
src/SaasKit.SharedKernel/Events/UserCreatedEvent.cs
src/SaasKit.SharedKernel/Events/UserUpdatedEvent.cs
src/SaasKit.SharedKernel/Events/UserDeactivatedEvent.cs
src/SaasKit.SharedKernel/Events/SubscriptionCreatedEvent.cs
src/SaasKit.SharedKernel/Events/SubscriptionChangedEvent.cs
src/SaasKit.SharedKernel/Events/SubscriptionCancelledEvent.cs
src/SaasKit.SharedKernel/Events/PortalCreatedEvent.cs
src/SaasKit.SharedKernel/Events/PortalUserRegisteredEvent.cs
src/SaasKit.SharedKernel/Events/InvitationAcceptedEvent.cs
```

### Instructions

1. **IDomainEvent** - Base interface:
   ```csharp
   public interface IDomainEvent
   {
       Guid EventId { get; }
       DateTimeOffset OccurredAt { get; }
   }
   ```

2. **Event records** - Each as immutable C# record:
   ```csharp
   public record TenantCreatedEvent(
       Guid EventId,
       DateTimeOffset OccurredAt,
       Guid TenantId,
       string Name,
       string Slug,
       Guid CreatedByUserId
   ) : IDomainEvent;
   
   public record UserCreatedEvent(
       Guid EventId,
       DateTimeOffset OccurredAt,
       Guid UserId,
       string Email,
       Guid? TenantId
   ) : IDomainEvent;
   
   public record SubscriptionChangedEvent(
       Guid EventId,
       DateTimeOffset OccurredAt,
       Guid TenantId,
       string OldPlanId,
       string NewPlanId,
       string ChangeReason
   ) : IDomainEvent;
   ```

3. Events contain only primitives and Guids - no entity references

### Tests
- [ ] Unit test: Each event record serializes/deserializes to JSON without data loss

### Acceptance Criteria
All platform domain events defined. Modules reference them from SharedKernel.

---

## P1-T04: Create Result type, pagination, and string extensions

### Dependencies
P0-T01

### Files to Create
```
src/SaasKit.SharedKernel/Models/Result.cs
src/SaasKit.SharedKernel/Models/PagedList.cs
src/SaasKit.SharedKernel/Extensions/StringExtensions.cs
src/SaasKit.SharedKernel/Extensions/QueryableExtensions.cs
```

### Instructions

1. **Result<T>** - Service return type:
   ```csharp
   public class Result<T>
   {
       public bool IsSuccess { get; }
       public T? Value { get; }
       public string? Error { get; }
       public string? ErrorCode { get; }
       
       private Result(bool isSuccess, T? value, string? error, string? errorCode)
       {
           IsSuccess = isSuccess;
           Value = value;
           Error = error;
           ErrorCode = errorCode;
       }
       
       public static Result<T> Success(T value) => new(true, value, null, null);
       public static Result<T> Failure(string error, string? code = null) => new(false, default, error, code);
   }
   ```

2. **PagedList<T>** - Paginated results:
   ```csharp
   public class PagedList<T>
   {
       public List<T> Items { get; }
       public int TotalCount { get; }
       public int Page { get; }
       public int PageSize { get; }
       public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
       public bool HasNextPage => Page < TotalPages;
       public bool HasPreviousPage => Page > 1;
   }
   ```

3. **StringExtensions**:
   ```csharp
   public static class StringExtensions
   {
       public static string ToSlug(this string value)
       {
           // URL-safe slug: lowercase, replace spaces with hyphens, remove special chars
       }
       
       public static string Truncate(this string value, int maxLength)
       {
           return value.Length <= maxLength ? value : value[..maxLength];
       }
   }
   ```

4. **QueryableExtensions**:
   ```csharp
   public static class QueryableExtensions
   {
       public static async Task<PagedList<T>> ToPagedListAsync<T>(
           this IQueryable<T> query,
           int page,
           int pageSize,
           CancellationToken ct = default)
       {
           var totalCount = await query.CountAsync(ct);
           var items = await query
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync(ct);
           
           return new PagedList<T>(items, totalCount, page, pageSize);
       }
   }
   ```

### Tests
- [ ] Unit test: ToSlug('Hello World!') returns 'hello-world'
- [ ] Unit test: Result.Success(42).IsSuccess is true
- [ ] Unit test: Result.Failure('x').IsSuccess is false
- [ ] Unit test: PagedList correctly computes TotalPages and HasNextPage

### Acceptance Criteria
Result type used throughout all services. PagedList for all list endpoints.

---

## P1-T05: Create API contract types (envelope, filters, sorting, errors)

### Dependencies
P1-T04

### Files to Create
```
src/SaasKit.SharedKernel/Api/ApiResponse.cs
src/SaasKit.SharedKernel/Api/ApiPagedResponse.cs
src/SaasKit.SharedKernel/Api/PaginationMeta.cs
src/SaasKit.SharedKernel/Api/ApiError.cs
src/SaasKit.SharedKernel/Api/QueryParameters.cs
src/SaasKit.SharedKernel/Api/FilterField.cs
src/SaasKit.SharedKernel/Api/FilterOperator.cs
src/SaasKit.SharedKernel/Api/SortField.cs
```

### Instructions

1. **ApiResponse<T>** - Single resource response:
   ```csharp
   public class ApiResponse<T>
   {
       public T Data { get; set; }
       public ApiMeta Meta { get; set; }
       
       public static ApiResponse<T> Ok(T data) => new() { Data = data, Meta = ApiMeta.Create() };
       public static ApiResponse<T> Created(T data) => new() { Data = data, Meta = ApiMeta.Create() };
   }
   
   public class ApiMeta
   {
       public DateTimeOffset Timestamp { get; set; }
       public string RequestId { get; set; }
   }
   ```

2. **ApiPagedResponse<T>** - Collection response:
   ```csharp
   public class ApiPagedResponse<T>
   {
       public List<T> Data { get; set; }
       public ApiMeta Meta { get; set; }
       public PaginationMeta Pagination { get; set; }
       
       public static ApiPagedResponse<T> From(PagedList<T> pagedList) => new()
       {
           Data = pagedList.Items,
           Meta = ApiMeta.Create(),
           Pagination = PaginationMeta.From(pagedList)
       };
   }
   ```

3. **PaginationMeta**:
   ```csharp
   public class PaginationMeta
   {
       public int Page { get; set; }
       public int PageSize { get; set; }
       public int TotalCount { get; set; }
       public int TotalPages { get; set; }
       public bool HasNextPage { get; set; }
       public bool HasPreviousPage { get; set; }
   }
   ```

4. **ApiError** - RFC 9457 Problem Details:
   ```csharp
   public class ApiError
   {
       public string Type { get; set; }
       public string Title { get; set; }
       public int Status { get; set; }
       public string Detail { get; set; }
       public string Instance { get; set; }
       public string RequestId { get; set; }
       public Dictionary<string, string[]>? Errors { get; set; }
       
       public static ApiError Validation(Dictionary<string, string[]> errors) => new()
       {
           Type = "https://api.saaskit.dev/errors/validation",
           Title = "Validation Failed",
           Status = 422,
           Errors = errors
       };
       
       public static ApiError NotFound(string detail) => new() { Status = 404, ... };
       public static ApiError Forbidden() => new() { Status = 403, ... };
       public static ApiError Conflict(string detail) => new() { Status = 409, ... };
       public static ApiError Internal() => new() { Status = 500, ... };
   }
   ```

5. **QueryParameters** - Reusable for list endpoints:
   ```csharp
   public class QueryParameters
   {
       public int Page { get; set; } = 1;
       public int PageSize { get; set; } = 20;
       public string? Sort { get; set; }
       public string? Fields { get; set; }
       public string? Include { get; set; }
       public Dictionary<string, FilterField> Filters { get; set; } = new();
       
       public List<SortField> GetSortFields() { ... }
       public List<FilterField> GetFilters() { ... }
   }
   ```

6. **FilterOperator** enum:
   ```csharp
   public enum FilterOperator
   {
       Eq,      // Default
       Gt,
       Gte,
       Lt,
       Lte,
       Contains,
       StartsWith,
       IsNull
   }
   ```

7. **FilterField** and **SortField**:
   ```csharp
   public class FilterField
   {
       public string Name { get; set; }
       public FilterOperator Operator { get; set; }
       public List<string> Values { get; set; }
   }
   
   public class SortField
   {
       public string Name { get; set; }
       public bool Descending { get; set; }
   }
   ```

### Tests
- [ ] Unit test: ApiResponse.Ok(42) serializes to { data: 42, meta: {...} }
- [ ] Unit test: ApiPagedResponse.From(pagedList) produces correct pagination
- [ ] Unit test: ApiError.Validation serializes to RFC 9457 shape
- [ ] Unit test: ApiError.NotFound has status 404
- [ ] Unit test: QueryParameters.GetSortFields() parses '-createdAt,name'
- [ ] Unit test: FilterField with Operator Eq and Values array works

### Acceptance Criteria
All API responses use these envelope types. Controllers never build JSON manually.

---

## P1-T06: Create clock implementation

### Dependencies
P1-T02

### Files to Create
```
src/SaasKit.Infrastructure/Clock/SystemClock.cs
```

### Instructions

1. **SystemClock** - Production implementation:
   ```csharp
   public class SystemClock : IClock
   {
       public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
   }
   ```

2. Register as singleton in DI

3. Create FakeClock for tests:
   ```csharp
   public class FakeClock : IClock
   {
       private DateTimeOffset _now;
       
       public FakeClock(DateTimeOffset? fixedTime = null)
       {
           _now = fixedTime ?? DateTimeOffset.UtcNow;
       }
       
       public DateTimeOffset UtcNow => _now;
       
       public void Advance(TimeSpan duration) => _now = _now.Add(duration);
       public void SetTime(DateTimeOffset time) => _now = time;
   }
   ```

### Tests
- [ ] Unit test: SystemClock.UtcNow within 1 second of DateTimeOffset.UtcNow

### Acceptance Criteria
IClock is injectable everywhere. Tests can freeze time with FakeClock.

---

## Phase 1 Checklist

- [ ] P1-T01: Base entity types created
- [ ] P1-T02: Core interfaces defined
- [ ] P1-T03: Domain events defined
- [ ] P1-T04: Result type and extensions created
- [ ] P1-T05: API contract types created
- [ ] P1-T06: Clock implementation done
- [ ] All SharedKernel types compile
- [ ] Unit tests pass
