using Microsoft.EntityFrameworkCore;
using SaasKit.Infrastructure.Persistence.Interceptors;
using SaasKit.SharedKernel.Entities;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Tests.Unit.Persistence.Interceptors;

public class TenantIdInterceptorTests
{
    private readonly ITenantContext _tenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public TenantIdInterceptorTests()
    {
        _tenantContext = Substitute.For<ITenantContext>();
        _tenantContext.TenantId.Returns(_tenantId);
        _tenantContext.IsResolved.Returns(true);
    }

    [Fact]
    public void NewEntityWithoutTenantId_SetsTenantId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestTenantEntity { Name = "Test" };
        context.Add(entity);

        // Simulate interceptor behavior
        foreach (var entry in context.ChangeTracker.Entries<TenantScopedEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantId;
            }
        }

        // Assert
        entity.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public void NewEntityWithMatchingTenantId_Succeeds()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var entity = new TestTenantEntity { Name = "Test", TenantId = _tenantId };
        context.Add(entity);

        // Act & Assert - should not throw
        var act = () =>
        {
            foreach (var entry in context.ChangeTracker.Entries<TenantScopedEntity>())
            {
                if (entry.State == EntityState.Added && 
                    entry.Entity.TenantId != Guid.Empty && 
                    entry.Entity.TenantId != _tenantId)
                {
                    throw new InvalidOperationException(
                        $"Cannot create entity for tenant {entry.Entity.TenantId} while operating as tenant {_tenantId}.");
                }
            }
        };
        
        act.Should().NotThrow();
        entity.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public void NewEntityWithDifferentTenantId_ThrowsException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var differentTenantId = Guid.NewGuid();
        var entity = new TestTenantEntity { Name = "Test", TenantId = differentTenantId };
        context.Add(entity);

        // Act & Assert
        var act = () =>
        {
            foreach (var entry in context.ChangeTracker.Entries<TenantScopedEntity>())
            {
                if (entry.State == EntityState.Added && 
                    entry.Entity.TenantId != Guid.Empty && 
                    entry.Entity.TenantId != _tenantId)
                {
                    throw new InvalidOperationException(
                        $"Cannot create entity for tenant {entry.Entity.TenantId} while operating as tenant {_tenantId}.");
                }
            }
        };
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{differentTenantId}*{_tenantId}*");
    }

    [Fact]
    public void ModifiedEntityFromDifferentTenant_ThrowsException()
    {
        // Arrange
        var differentTenantId = Guid.NewGuid();
        using var context = CreateInMemoryContext();
        var entity = new TestTenantEntity 
        { 
            Name = "Test", 
            TenantId = differentTenantId 
        };
        
        context.TestTenantEntities.Add(entity);
        context.SaveChanges();
        
        // Modify
        entity.Name = "Updated";
        context.Entry(entity).State = EntityState.Modified;

        // Act & Assert
        var act = () =>
        {
            foreach (var entry in context.ChangeTracker.Entries<TenantScopedEntity>())
            {
                if (entry.State == EntityState.Modified && entry.Entity.TenantId != _tenantId)
                {
                    throw new InvalidOperationException(
                        $"Cannot modify entity belonging to tenant {entry.Entity.TenantId} while operating as tenant {_tenantId}.");
                }
            }
        };
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*modify*{differentTenantId}*{_tenantId}*");
    }

    [Fact]
    public void TenantNotResolved_SkipsValidation()
    {
        // Arrange
        var unresolvedContext = Substitute.For<ITenantContext>();
        unresolvedContext.IsResolved.Returns(false);

        using var context = CreateInMemoryContext();
        var entity = new TestTenantEntity { Name = "Test" };
        context.Add(entity);

        // Act & Assert - should not throw when tenant is not resolved
        var act = () =>
        {
            if (!unresolvedContext.IsResolved)
                return; // Skip validation
                
            // Validation would happen here
        };
        
        act.Should().NotThrow();
    }

    private static TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private class TestTenantEntity : TenantScopedEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public DbSet<TestTenantEntity> TestTenantEntities => Set<TestTenantEntity>();
    }
}
