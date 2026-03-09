using System.ComponentModel.DataAnnotations;

namespace Notification.Contracts.DTOs;

public sealed record NotificationTemplateDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string TitleAr { get; init; } = string.Empty;
    public string BodyEn { get; init; } = string.Empty;
    public string BodyAr { get; init; } = string.Empty;
    public string DefaultPriority { get; init; } = "normal";
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record NotificationTemplateListDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string TitleAr { get; init; } = string.Empty;
    public string DefaultPriority { get; init; } = "normal";
    public bool IsActive { get; init; }
}

public sealed record CreateNotificationTemplateRequest
{
    [Required]
    [MaxLength(64)]
    public string EventType { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string TitleEn { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string TitleAr { get; init; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string BodyEn { get; init; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string BodyAr { get; init; } = string.Empty;

    public string DefaultPriority { get; init; } = "normal";
}

public sealed record UpdateNotificationTemplateRequest
{
    [MaxLength(256)]
    public string? TitleEn { get; init; }

    [MaxLength(256)]
    public string? TitleAr { get; init; }

    [MaxLength(4000)]
    public string? BodyEn { get; init; }

    [MaxLength(4000)]
    public string? BodyAr { get; init; }

    public string? DefaultPriority { get; init; }
    public bool? IsActive { get; init; }
}
