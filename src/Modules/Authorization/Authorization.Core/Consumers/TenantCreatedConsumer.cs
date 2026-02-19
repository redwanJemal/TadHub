using Authorization.Contracts;
using Authorization.Core.Seeds;
using MassTransit;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Events;

namespace Authorization.Core.Consumers;

/// <summary>
/// Consumes TenantCreatedEvent to seed default roles for new tenants.
/// Seeds both platform roles and Tadbeer domain roles.
/// </summary>
public class TenantCreatedConsumer : IConsumer<TenantCreatedEvent>
{
    private readonly IAuthorizationModuleService _authService;
    private readonly TadbeerRoleSeeder _tadbeerRoleSeeder;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TenantCreatedConsumer> _logger;

    public TenantCreatedConsumer(
        IAuthorizationModuleService authService,
        TadbeerRoleSeeder tadbeerRoleSeeder,
        AppDbContext dbContext,
        ILogger<TenantCreatedConsumer> logger)
    {
        _authService = authService;
        _tadbeerRoleSeeder = tadbeerRoleSeeder;
        _dbContext = dbContext;
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

            // Seed Tadbeer domain roles (agency-admin, receptionist, cashier, pro-officer, agent, accountant, viewer)
            await _tadbeerRoleSeeder.SeedRolesAsync(
                _dbContext,
                message.TenantId,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully seeded Tadbeer domain roles for tenant {TenantId}",
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
