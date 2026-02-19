using Microsoft.AspNetCore.Http;
using SaasKit.Infrastructure.Sse;

namespace SaasKit.Tests.Unit.Sse;

public class SseConnectionManagerTests
{
    private readonly SseConnectionManager _manager;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId1 = Guid.NewGuid();
    private readonly Guid _userId2 = Guid.NewGuid();

    public SseConnectionManagerTests()
    {
        var logger = Substitute.For<ILogger<SseConnectionManager>>();
        _manager = new SseConnectionManager(logger);
    }

    [Fact]
    public void AddConnection_IncreasesConnectionCount()
    {
        // Arrange
        var connection = CreateConnection(_userId1, _tenantId);

        // Act
        _manager.AddConnection(connection);

        // Assert
        _manager.ConnectionCount.Should().Be(1);
    }

    [Fact]
    public void RemoveConnection_DecreasesConnectionCount()
    {
        // Arrange
        var connection = CreateConnection(_userId1, _tenantId);
        _manager.AddConnection(connection);

        // Act
        _manager.RemoveConnection(connection.ConnectionId);

        // Assert
        _manager.ConnectionCount.Should().Be(0);
    }

    [Fact]
    public void GetConnectionsByUser_ReturnsOnlyUserConnections()
    {
        // Arrange
        var conn1 = CreateConnection(_userId1, _tenantId);
        var conn2 = CreateConnection(_userId1, _tenantId);
        var conn3 = CreateConnection(_userId2, _tenantId);
        
        _manager.AddConnection(conn1);
        _manager.AddConnection(conn2);
        _manager.AddConnection(conn3);

        // Act
        var userConnections = _manager.GetConnectionsByUser(_userId1).ToList();

        // Assert
        userConnections.Should().HaveCount(2);
        userConnections.Should().Contain(conn1);
        userConnections.Should().Contain(conn2);
        userConnections.Should().NotContain(conn3);
    }

    [Fact]
    public void GetConnectionsByTenant_ReturnsOnlyTenantConnections()
    {
        // Arrange
        var tenant2 = Guid.NewGuid();
        var conn1 = CreateConnection(_userId1, _tenantId);
        var conn2 = CreateConnection(_userId2, _tenantId);
        var conn3 = CreateConnection(_userId1, tenant2);
        
        _manager.AddConnection(conn1);
        _manager.AddConnection(conn2);
        _manager.AddConnection(conn3);

        // Act
        var tenantConnections = _manager.GetConnectionsByTenant(_tenantId).ToList();

        // Assert
        tenantConnections.Should().HaveCount(2);
        tenantConnections.Should().Contain(conn1);
        tenantConnections.Should().Contain(conn2);
        tenantConnections.Should().NotContain(conn3);
    }

    [Fact]
    public void GetAllConnections_ReturnsAllConnections()
    {
        // Arrange
        var conn1 = CreateConnection(_userId1, _tenantId);
        var conn2 = CreateConnection(_userId2, _tenantId);
        
        _manager.AddConnection(conn1);
        _manager.AddConnection(conn2);

        // Act
        var allConnections = _manager.GetAllConnections().ToList();

        // Assert
        allConnections.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveConnection_NonExistent_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _manager.RemoveConnection("non-existent");
        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleConnectionsSameUser_TrackedSeparately()
    {
        // Arrange
        var conn1 = CreateConnection(_userId1, _tenantId);
        var conn2 = CreateConnection(_userId1, _tenantId);
        
        _manager.AddConnection(conn1);
        _manager.AddConnection(conn2);

        // Assert
        _manager.ConnectionCount.Should().Be(2);
        _manager.GetConnectionsByUser(_userId1).Should().HaveCount(2);
    }

    private static SseConnection CreateConnection(Guid userId, Guid tenantId)
    {
        var response = Substitute.For<HttpResponse>();
        return new SseConnection(response, userId, tenantId);
    }
}
