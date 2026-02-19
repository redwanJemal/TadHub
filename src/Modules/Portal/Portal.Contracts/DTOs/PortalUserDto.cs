namespace Portal.Contracts.DTOs;

/// <summary>
/// DTO for portal user data.
/// </summary>
public record PortalUserDto
{
    public Guid Id { get; init; }
    public Guid PortalId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public int LoginCount { get; init; }
    public bool HasSso { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public string FullName => string.IsNullOrWhiteSpace(DisplayName)
        ? $"{FirstName} {LastName}".Trim()
        : DisplayName;
}

/// <summary>
/// Request to create a portal user (admin).
/// </summary>
public record CreatePortalUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string? Password { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool SendInvitation { get; init; } = true;
}

/// <summary>
/// Request to update a portal user.
/// </summary>
public record UpdatePortalUserRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public string? PhoneNumber { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Portal user registration request (public).
/// </summary>
public record PortalUserRegistrationRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

/// <summary>
/// Portal user login request.
/// </summary>
public record PortalUserLoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Portal user login response.
/// </summary>
public record PortalUserLoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public PortalUserDto User { get; init; } = null!;
}
