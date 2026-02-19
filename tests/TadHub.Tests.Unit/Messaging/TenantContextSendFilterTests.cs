using MassTransit;
using TadHub.Infrastructure.Messaging.Filters;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Tests.Unit.Messaging;

// Public test message for NSubstitute mocking
public record SendTestMessage(string Data);

public class TenantContextSendFilterTests
{
    private readonly ITenantContext _tenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public TenantContextSendFilterTests()
    {
        _tenantContext = Substitute.For<ITenantContext>();
    }

    [Fact]
    public async Task Send_WhenTenantResolved_AddsTenantIdHeader()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns(_tenantId);
        
        var filter = new TenantContextSendFilter<SendTestMessage>(_tenantContext);
        var context = Substitute.For<SendContext<SendTestMessage>>();
        var headers = Substitute.For<SendHeaders>();
        context.Headers.Returns(headers);
        var next = Substitute.For<IPipe<SendContext<SendTestMessage>>>();

        // Act
        await filter.Send(context, next);

        // Assert - verify the header was set with correct key and tenant ID
        headers.Received(1).Set(
            TenantContextSendFilter<SendTestMessage>.TenantIdHeader,
            _tenantId.ToString());
        await next.Received(1).Send(context);
    }

    [Fact]
    public async Task Send_WhenTenantNotResolved_DoesNotAddHeader()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(false);
        
        var filter = new TenantContextSendFilter<SendTestMessage>(_tenantContext);
        var context = Substitute.For<SendContext<SendTestMessage>>();
        var headers = Substitute.For<SendHeaders>();
        context.Headers.Returns(headers);
        var next = Substitute.For<IPipe<SendContext<SendTestMessage>>>();

        // Act
        await filter.Send(context, next);

        // Assert
        headers.DidNotReceive().Set(
            Arg.Any<string>(),
            Arg.Any<object>());
        await next.Received(1).Send(context);
    }

    [Fact]
    public async Task Send_AlwaysCallsNext()
    {
        // Arrange
        _tenantContext.IsResolved.Returns(true);
        _tenantContext.TenantId.Returns(_tenantId);
        
        var filter = new TenantContextSendFilter<SendTestMessage>(_tenantContext);
        var context = Substitute.For<SendContext<SendTestMessage>>();
        var headers = Substitute.For<SendHeaders>();
        context.Headers.Returns(headers);
        var next = Substitute.For<IPipe<SendContext<SendTestMessage>>>();

        // Act
        await filter.Send(context, next);

        // Assert
        await next.Received(1).Send(context);
    }

    [Fact]
    public void Probe_CreatesFilterScope()
    {
        // Arrange
        var filter = new TenantContextSendFilter<SendTestMessage>(_tenantContext);
        var probeContext = Substitute.For<ProbeContext>();

        // Act
        filter.Probe(probeContext);

        // Assert
        probeContext.Received(1).CreateFilterScope("tenantContextSend");
    }
}
