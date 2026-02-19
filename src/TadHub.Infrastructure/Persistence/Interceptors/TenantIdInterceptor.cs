using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets TenantId on new entities
/// and prevents cross-tenant writes.
/// </summary>
public sealed class TenantIdInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantIdInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTenantIds(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTenantIds(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantIds(DbContext? context)
    {
        if (context is null || !_tenantContext.IsResolved)
            return;

        var currentTenantId = _tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries<TenantScopedEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Auto-set TenantId for new entities
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = currentTenantId;
                    }
                    else if (entry.Entity.TenantId != currentTenantId)
                    {
                        // Prevent creating entities for a different tenant
                        throw new InvalidOperationException(
                            $"Cannot create entity for tenant {entry.Entity.TenantId} while operating as tenant {currentTenantId}.");
                    }
                    break;

                case EntityState.Modified:
                    // Prevent modifying entities from a different tenant
                    if (entry.Entity.TenantId != currentTenantId)
                    {
                        throw new InvalidOperationException(
                            $"Cannot modify entity belonging to tenant {entry.Entity.TenantId} while operating as tenant {currentTenantId}.");
                    }
                    
                    // Prevent changing TenantId
                    if (entry.OriginalValues.GetValue<Guid>(nameof(TenantScopedEntity.TenantId)) != entry.Entity.TenantId)
                    {
                        throw new InvalidOperationException("Cannot change the TenantId of an entity.");
                    }
                    break;

                case EntityState.Deleted:
                    // Prevent deleting entities from a different tenant
                    if (entry.Entity.TenantId != currentTenantId)
                    {
                        throw new InvalidOperationException(
                            $"Cannot delete entity belonging to tenant {entry.Entity.TenantId} while operating as tenant {currentTenantId}.");
                    }
                    break;
            }
        }
    }
}
