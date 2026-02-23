using Authorization.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using TadHub.SharedKernel.Events;

namespace Authorization.Core.Consumers;

/// <summary>
/// Consumes TenantCreatedEvent to seed default roles for new tenants.
/// </summary>
public class TenantCreatedConsumer : IConsumer<TenantCreatedEvent>
{
    private readonly IAuthorizationModuleService _authService;
    private readonly ILogger<TenantCreatedConsumer> _logger;

    public TenantCreatedConsumer(
        IAuthorizationModuleService authService,
        ILogger<TenantCreatedConsumer> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TenantCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received TenantCreatedEvent for tenant {TenantId} ({TenantName})",
            message.TenantId, message.Name);

        try
        {
            // Seed platform roles (admin, member, viewer)
            await _authService.SeedDefaultRolesAsync(
                message.TenantId,
                message.CreatedByUserId,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully seeded platform roles for tenant {TenantId}",
                message.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to seed roles for tenant {TenantId}",
                message.TenantId);
            throw; // Re-throw to trigger retry
        }
    }
}
