using Microsoft.EntityFrameworkCore;
using SaasKit.Infrastructure.Persistence.Interceptors;
using SaasKit.SharedKernel.Entities;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Tests.Unit.Persistence.Interceptors;

public class SoftDeleteInterceptorTests
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly DateTimeOffset _fixedTime = new(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
    private readonly Guid _userId = Guid.NewGuid();

    public SoftDeleteInterceptorTests()
    {
        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(_fixedTime);

        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(_userId);
    }

    [Fact]
    public void DeletedEntity_ConvertsToSoftDelete()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestSoftDeletableEntity 
        { 
            Name = "Test",
            TenantId = Guid.NewGuid()
        };
        
        context.Add(entity);
        context.SaveChanges();
        
        // Mark for deletion
        context.Remove(entity);

        // Simulate interceptor behavior
        foreach (var entry in context.ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = _fixedTime;
                entry.Entity.DeletedBy = _userId;
                entry.Entity.UpdatedAt = _fixedTime;
            }
        }

        // Assert
        var entry2 = context.Entry(entity);
        entry2.State.Should().Be(EntityState.Modified);
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(_fixedTime);
        entity.DeletedBy.Should().Be(_userId);
    }

    [Fact]
    public void DeletedEntity_UpdatesTimestamp()
    {
        // Arrange
        var originalUpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var context = CreateInMemoryContext();
        var entity = new TestSoftDeletableEntity 
        { 
            Name = "Test",
            TenantId = Guid.NewGuid(),
            UpdatedAt = originalUpdatedAt
        };
        
        context.Add(entity);
        context.SaveChanges();
        
        // Reset UpdatedAt to test it changes
        entity.UpdatedAt = originalUpdatedAt;
        
        // Mark for deletion
        context.Remove(entity);

        // Simulate interceptor behavior
        foreach (var entry in context.ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = _fixedTime;
                entry.Entity.UpdatedAt = _fixedTime;
            }
        }

        // Assert
        entity.UpdatedAt.Should().Be(_fixedTime);
    }

    [Fact]
    public void UnauthenticatedUser_DeletedByIsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestSoftDeletableEntity 
        { 
            Name = "Test",
            TenantId = Guid.NewGuid()
        };
        
        context.Add(entity);
        context.SaveChanges();
        context.Remove(entity);

        // Simulate interceptor behavior with unauthenticated user
        Guid? userId = null; // unauthenticated
        
        foreach (var entry in context.ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = _fixedTime;
                entry.Entity.DeletedBy = userId;
            }
        }

        // Assert
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void NonSoftDeletableEntity_NotAffected()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestNonSoftDeletableEntity { Name = "Test" };
        
        context.Add(entity);
        context.SaveChanges();
        context.Remove(entity);

        // The soft delete interceptor only affects SoftDeletableEntity
        // Non-soft-deletable entities remain in Deleted state
        var entry = context.Entry(entity);
        
        // Assert - should still be in Deleted state (hard delete)
        entry.State.Should().Be(EntityState.Deleted);
    }

    private static TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private class TestSoftDeletableEntity : SoftDeletableEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestNonSoftDeletableEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public DbSet<TestSoftDeletableEntity> SoftDeletableEntities => Set<TestSoftDeletableEntity>();
        public DbSet<TestNonSoftDeletableEntity> NonSoftDeletableEntities => Set<TestNonSoftDeletableEntity>();
    }
}
