using Identity.Contracts;
using Identity.Contracts.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Core.Consumers;

/// <summary>
/// Consumes user creation events from Keycloak.
/// Creates a UserProfile for each new Keycloak user.
/// </summary>
public class KeycloakUserCreatedConsumer : IConsumer<KeycloakUserCreatedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<KeycloakUserCreatedConsumer> _logger;

    public KeycloakUserCreatedConsumer(
        IIdentityService identityService,
        ILogger<KeycloakUserCreatedConsumer> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<KeycloakUserCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received Keycloak user created event for {Email} (KC ID: {KeycloakId})",
            message.Email, message.UserId);

        // Check if user already exists (idempotency)
        var existing = await _identityService.GetByKeycloakIdAsync(message.UserId, context.CancellationToken);
        if (existing.IsSuccess)
        {
            _logger.LogDebug(
                "User profile already exists for Keycloak ID {KeycloakId}, skipping creation",
                message.UserId);
            return;
        }

        // Create new user profile
        var request = new CreateUserProfileRequest
        {
            KeycloakId = message.UserId,
            Email = message.Email,
            FirstName = message.FirstName,
            LastName = message.LastName
        };

        var result = await _identityService.CreateAsync(request, context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Created user profile {UserId} for Keycloak user {KeycloakId}",
                result.Value!.Id, message.UserId);
        }
        else
        {
            _logger.LogWarning(
                "Failed to create user profile for Keycloak user {KeycloakId}: {Error}",
                message.UserId, result.Error);
        }
    }
}
