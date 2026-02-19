using SaasKit.Infrastructure.Caching;
using SaasKit.SharedKernel.Interfaces;
using StackExchange.Redis;

namespace SaasKit.Tests.Unit.Caching;

public class RedisCacheServiceTests
{
    private readonly ITenantContext _tenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public RedisCacheServiceTests()
    {
        _tenantContext = Substitute.For<ITenantContext>();
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns(_tenantId);
    }

    [Fact]
    public void BuildKey_WithModuleEntityId_FormatsCorrectly()
    {
        // Arrange
        var service = CreateService();

        // Act
        var key = service.BuildKey("identity", "user", "123");

        // Assert
        key.Should().Be("identity:user:123");
    }

    [Fact]
    public void BuildKey_WithNestedKeys_FormatsCorrectly()
    {
        // Arrange
        var service = CreateService();

        // Act
        var key = service.BuildKey("tenancy", "settings", "email-config");

        // Assert
        key.Should().Be("tenancy:settings:email-config");
    }

    [Fact]
    public void BuildTenantKey_WhenTenantResolved_PrefixesWithTenantId()
    {
        // Arrange
        var service = CreateService();

        // Act
        var key = service.BuildTenantKey("users:list");

        // Assert
        key.Should().Be($"{_tenantId}:users:list");
    }

    [Fact]
    public void BuildTenantKey_WhenTenantNotResolved_UsesGlobalPrefix()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(false);
        var service = CreateService();

        // Act
        var key = service.BuildTenantKey("settings:global");

        // Assert
        key.Should().Be("global:settings:global");
    }

    [Fact]
    public void BuildTenantKey_WithComplexKey_FormatsCorrectly()
    {
        // Arrange
        var service = CreateService();
        var entityId = Guid.NewGuid();

        // Act
        var moduleKey = service.BuildKey("identity", "user", entityId.ToString());
        var fullKey = service.BuildTenantKey(moduleKey);

        // Assert
        fullKey.Should().Be($"{_tenantId}:identity:user:{entityId}");
    }

    [Fact]
    public void BuildTenantKey_DifferentTenants_ProducesDifferentKeys()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        
        _tenantContext.TenantId.Returns(tenant1);
        var service = CreateService();
        var key1 = service.BuildTenantKey("users:list");

        _tenantContext.TenantId.Returns(tenant2);
        var key2 = service.BuildTenantKey("users:list");

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().StartWith($"{tenant1}:");
        key2.Should().StartWith($"{tenant2}:");
    }

    private RedisCacheService CreateService()
    {
        var cache = Substitute.For<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
        var redis = Substitute.For<IConnectionMultiplexer>();
        var logger = Substitute.For<ILogger<RedisCacheService>>();
        
        return new RedisCacheService(cache, redis, _tenantContext, logger);
    }
}
