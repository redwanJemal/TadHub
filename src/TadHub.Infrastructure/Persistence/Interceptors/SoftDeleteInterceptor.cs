using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that converts hard deletes to soft deletes
/// for entities inheriting from SoftDeletableEntity.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public SoftDeleteInterceptor(IClock clock, ICurrentUser currentUser)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertToSoftDelete(DbContext? context)
    {
        if (context is null)
            return;

        var now = _clock.UtcNow;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;

        foreach (var entry in context.ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = userId;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
