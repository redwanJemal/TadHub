namespace Identity.Core.Consumers;

/// <summary>
/// Event received from Keycloak when a user is created.
/// This is typically sent via Keycloak event listener or webhook.
/// </summary>
public sealed record KeycloakUserCreatedEvent
{
    /// <summary>
    /// Keycloak user ID (sub claim).
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; init; }

    /// <summary>
    /// Unix timestamp of when the user was created in Keycloak.
    /// </summary>
    public long? CreatedTimestamp { get; init; }
}

/// <summary>
/// Event received from Keycloak when a user is updated.
/// </summary>
public sealed record KeycloakUserUpdatedEvent
{
    /// <summary>
    /// Keycloak user ID (sub claim).
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; init; }

    /// <summary>
    /// Whether the user is enabled in Keycloak.
    /// </summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>
/// Event received from Keycloak when a user is deleted.
/// </summary>
public sealed record KeycloakUserDeletedEvent
{
    /// <summary>
    /// Keycloak user ID (sub claim).
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}
