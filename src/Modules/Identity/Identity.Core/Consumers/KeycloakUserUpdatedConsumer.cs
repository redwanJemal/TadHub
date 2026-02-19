using Identity.Contracts;
using Identity.Contracts.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Core.Consumers;

/// <summary>
/// Consumes user update events from Keycloak.
/// Updates the UserProfile when Keycloak user data changes.
/// </summary>
public class KeycloakUserUpdatedConsumer : IConsumer<KeycloakUserUpdatedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<KeycloakUserUpdatedConsumer> _logger;

    public KeycloakUserUpdatedConsumer(
        IIdentityService identityService,
        ILogger<KeycloakUserUpdatedConsumer> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<KeycloakUserUpdatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received Keycloak user updated event for {Email} (KC ID: {KeycloakId})",
            message.Email, message.UserId);

        // Find existing user
        var existing = await _identityService.GetByKeycloakIdAsync(message.UserId, context.CancellationToken);
        if (!existing.IsSuccess)
        {
            _logger.LogWarning(
                "User profile not found for Keycloak ID {KeycloakId}, creating new profile",
                message.UserId);

            // Create if doesn't exist (handles edge cases)
            var createRequest = new CreateUserProfileRequest
            {
                KeycloakId = message.UserId,
                Email = message.Email,
                FirstName = message.FirstName,
                LastName = message.LastName
            };

            await _identityService.CreateAsync(createRequest, context.CancellationToken);
            return;
        }

        // Update existing user
        var updateRequest = new UpdateUserProfileRequest
        {
            FirstName = message.FirstName,
            LastName = message.LastName
        };

        var result = await _identityService.UpdateAsync(
            existing.Value!.Id,
            updateRequest,
            context.CancellationToken);

        // Handle enabled/disabled status
        if (!message.Enabled && existing.Value!.IsActive)
        {
            await _identityService.DeactivateAsync(existing.Value!.Id, context.CancellationToken);
            _logger.LogInformation(
                "Deactivated user {UserId} because Keycloak user was disabled",
                existing.Value!.Id);
        }
        else if (message.Enabled && !existing.Value!.IsActive)
        {
            await _identityService.ReactivateAsync(existing.Value!.Id, context.CancellationToken);
            _logger.LogInformation(
                "Reactivated user {UserId} because Keycloak user was re-enabled",
                existing.Value!.Id);
        }

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Updated user profile {UserId} from Keycloak event",
                result.Value!.Id);
        }
    }
}
