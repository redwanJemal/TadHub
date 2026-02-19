# _Template Module

This is a template/starter module demonstrating all standard patterns. Copy this folder to create a new bounded context.

## Quick Start (< 30 minutes)

1. **Copy the module:**
   ```bash
   cp -r src/Modules/_Template src/Modules/YourModule
   ```

2. **Rename namespaces:**
   - Replace `_Template` with `YourModule` in all files
   - Update `.csproj` project names

3. **Add project references:**
   ```xml
   <!-- In SaasKit.Api.csproj -->
   <ProjectReference Include="..\Modules\YourModule\YourModule.Core\YourModule.Core.csproj" />
   ```

4. **Register the module in Program.cs:**
   ```csharp
   builder.Services.AddYourModuleModule();
   ```

5. **Customize:**
   - Rename `TemplateEntity` to your domain entity
   - Add/remove properties
   - Update EF configuration
   - Add business logic to service

## File Structure

```
YourModule/
├── YourModule.Contracts/          # Public interfaces & DTOs
│   ├── DTOs/
│   │   └── YourEntityDto.cs
│   └── IYourService.cs
└── YourModule.Core/               # Implementation
    ├── Entities/
    │   └── YourEntity.cs          # EF entity
    ├── Persistence/
    │   └── YourEntityConfiguration.cs
    ├── Services/
    │   └── YourService.cs
    └── YourModuleServiceRegistration.cs
```

## Patterns Demonstrated

### Filtering
```csharp
// Define filterable fields
private static readonly Dictionary<string, Expression<Func<Entity, object>>> Filters = new()
{
    ["name"] = x => x.Name,           // filter[name]=value
    ["isActive"] = x => x.IsActive    // filter[isActive]=true
};

// Apply in query
query.ApplyFilters(qp.Filters, Filters)
```

### Sorting
```csharp
// Define sortable fields
private static readonly Dictionary<string, Expression<Func<Entity, object>>> Sortable = new()
{
    ["name"] = x => x.Name,           // sort=name, sort=-name
    ["createdAt"] = x => x.CreatedAt
};

// Apply in query
query.ApplySort(qp.GetSortFields(), Sortable)
```

### Pagination
```csharp
// Returns PagedList<T> with items, totalCount, page, pageSize
return await query.ToPagedListAsync(qp, ct);
```

### Result Pattern
```csharp
// Success
return Result<T>.Success(value);

// Errors
return Result<T>.NotFound("Message");
return Result<T>.Conflict("Message");
return Result<T>.ValidationError("Message");
```

## Supported Query Parameters

| Parameter | Example | Description |
|-----------|---------|-------------|
| `filter[field]` | `?filter[name]=test` | Exact match |
| `filter[field][contains]` | `?filter[name][contains]=test` | Contains |
| `filter[field][gte]` | `?filter[createdAt][gte]=2026-01-01` | Greater than or equal |
| `filter[field][lte]` | `?filter[createdAt][lte]=2026-12-31` | Less than or equal |
| `sort` | `?sort=-createdAt` | Sort (- for descending) |
| `page` | `?page=2` | Page number (1-indexed) |
| `pageSize` | `?pageSize=25` | Items per page (default 20, max 100) |
