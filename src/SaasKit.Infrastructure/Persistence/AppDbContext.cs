using Microsoft.EntityFrameworkCore;
using SaasKit.SharedKernel.Entities;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Persistence;

/// <summary>
/// Main application DbContext with multi-tenancy support via global query filters.
/// Implements IUnitOfWork for the unit of work pattern.
/// Entity configurations are auto-discovered from module assemblies.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) 
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from all module assemblies
        ApplyConfigurations(modelBuilder);

        // Apply global query filters for multi-tenancy and soft delete
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private static void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        // Apply from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Scan for module assemblies containing entity configurations
        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.Contains(".Core") == true ||
                        a.GetName().Name?.Contains(".Infrastructure") == true);

        foreach (var assembly in moduleAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Get all entity types that inherit from our base classes
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Apply tenant filter for TenantScopedEntity and its descendants
            if (typeof(TenantScopedEntity).IsAssignableFrom(clrType))
            {
                ApplyTenantFilter(modelBuilder, clrType);
            }

            // Apply soft delete filter for SoftDeletableEntity and its descendants
            if (typeof(SoftDeletableEntity).IsAssignableFrom(clrType))
            {
                ApplySoftDeleteFilter(modelBuilder, clrType);
            }
        }
    }

    private void ApplyTenantFilter(ModelBuilder modelBuilder, Type entityType)
    {
        // Use expression to create: e => e.TenantId == _tenantContext.TenantId
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantScopedEntity.TenantId));
        
        // Create a closure that captures _tenantContext
        var tenantContextConstant = System.Linq.Expressions.Expression.Constant(_tenantContext);
        var tenantContextTenantId = System.Linq.Expressions.Expression.Property(tenantContextConstant, nameof(ITenantContext.TenantId));
        
        var equals = System.Linq.Expressions.Expression.Equal(tenantIdProperty, tenantContextTenantId);
        var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder, Type entityType)
    {
        // Use expression to create: e => !e.IsDeleted
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted));
        var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
        var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

        // Combine with existing filter if one exists
        var existingFilter = modelBuilder.Entity(entityType).Metadata.GetQueryFilter();
        if (existingFilter != null)
        {
            // Combine: existingFilter && !e.IsDeleted
            var body = System.Linq.Expressions.Expression.AndAlso(
                System.Linq.Expressions.Expression.Invoke(existingFilter, parameter),
                notDeleted);
            lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
        }

        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Use snake_case naming convention for PostgreSQL
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    /// <summary>
    /// Saves changes and returns the number of affected entities.
    /// Domain events are dispatched by the SaveChangesInterceptor if configured.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
