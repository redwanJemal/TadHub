using Microsoft.EntityFrameworkCore;
using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Persistence;

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

        // Combine with existing filter if one exists (e.g., tenant filter)
        var existingFilter = modelBuilder.Entity(entityType).Metadata.GetQueryFilter();
        if (existingFilter is System.Linq.Expressions.LambdaExpression existingLambda)
        {
            // Replace the existing filter's parameter with our parameter, then AND the bodies.
            // This avoids Expression.Invoke which EF Core cannot translate to SQL.
            var rewrittenBody = new ParameterReplacer(existingLambda.Parameters[0], parameter)
                .Visit(existingLambda.Body);
            var combined = System.Linq.Expressions.Expression.AndAlso(rewrittenBody, notDeleted);
            lambda = System.Linq.Expressions.Expression.Lambda(combined, parameter);
        }

        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }

    /// <summary>
    /// Replaces all occurrences of one ParameterExpression with another in an expression tree.
    /// Used to combine query filter expressions without Expression.Invoke.
    /// </summary>
    private sealed class ParameterReplacer : System.Linq.Expressions.ExpressionVisitor
    {
        private readonly System.Linq.Expressions.ParameterExpression _oldParam;
        private readonly System.Linq.Expressions.ParameterExpression _newParam;

        public ParameterReplacer(
            System.Linq.Expressions.ParameterExpression oldParam,
            System.Linq.Expressions.ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        protected override System.Linq.Expressions.Expression VisitParameter(
            System.Linq.Expressions.ParameterExpression node)
            => node == _oldParam ? _newParam : base.VisitParameter(node);
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
