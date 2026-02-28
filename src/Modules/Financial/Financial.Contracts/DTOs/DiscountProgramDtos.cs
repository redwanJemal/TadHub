using System.ComponentModel.DataAnnotations;

namespace Financial.Contracts.DTOs;

public sealed record DiscountProgramDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal DiscountPercentage { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public bool IsActive { get; init; }
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    public string? Description { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record DiscountProgramListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal DiscountPercentage { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public bool IsActive { get; init; }
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CreateDiscountProgramRequest
{
    [Required] [MaxLength(200)] public string Name { get; init; } = string.Empty;
    [MaxLength(200)] public string? NameAr { get; init; }
    [Required] public string Type { get; init; } = "Custom";
    [Required] [Range(0.01, 100)] public decimal DiscountPercentage { get; init; }
    [Range(0, double.MaxValue)] public decimal? MaxDiscountAmount { get; init; }
    public bool IsActive { get; init; } = true;
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    [MaxLength(1000)] public string? Description { get; init; }
}

public sealed record UpdateDiscountProgramRequest
{
    [MaxLength(200)] public string? Name { get; init; }
    [MaxLength(200)] public string? NameAr { get; init; }
    [Range(0.01, 100)] public decimal? DiscountPercentage { get; init; }
    [Range(0, double.MaxValue)] public decimal? MaxDiscountAmount { get; init; }
    public bool? IsActive { get; init; }
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    [MaxLength(1000)] public string? Description { get; init; }
}
