using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SaasKit.SharedKernel.Entities;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on entities.
/// Sets CreatedAt/UpdatedAt from IClock and CreatedBy/UpdatedBy from ICurrentUser.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public AuditableEntityInterceptor(IClock clock, ICurrentUser currentUser)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        var now = _clock.UtcNow;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    
                    if (entry.Entity is IAuditable auditable)
                    {
                        auditable.CreatedBy = userId;
                        auditable.UpdatedBy = userId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    
                    if (entry.Entity is IAuditable auditableModified)
                    {
                        auditableModified.UpdatedBy = userId;
                    }
                    break;
            }
        }
    }
}
