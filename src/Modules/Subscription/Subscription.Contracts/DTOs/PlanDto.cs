namespace Subscription.Contracts.DTOs;

/// <summary>
/// DTO for plan data.
/// </summary>
public record PlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsDefault { get; init; }
    public int DisplayOrder { get; init; }
    public List<PlanPriceDto> Prices { get; init; } = new();
    public List<PlanFeatureDto> Features { get; init; } = new();
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// DTO for plan price data.
/// </summary>
public record PlanPriceDto
{
    public Guid Id { get; init; }
    public Guid PlanId { get; init; }
    public long Amount { get; init; }
    public string Currency { get; init; } = "usd";
    public string Interval { get; init; } = "month";
    public int IntervalCount { get; init; }
    public int TrialDays { get; init; }
    public bool IsActive { get; init; }
    
    /// <summary>
    /// Formatted price (e.g., "$19.99/month").
    /// </summary>
    public string FormattedPrice => FormatPrice();

    private string FormatPrice()
    {
        var symbol = Currency.ToLower() switch
        {
            "usd" => "$",
            "eur" => "€",
            "gbp" => "£",
            _ => Currency.ToUpper() + " "
        };
        var price = Amount / 100m;
        var intervalLabel = Interval == "year" ? "year" : "month";
        return $"{symbol}{price:F2}/{intervalLabel}";
    }
}

/// <summary>
/// DTO for plan feature data.
/// </summary>
public record PlanFeatureDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ValueType { get; init; } = "boolean";
    public bool? BooleanValue { get; init; }
    public long? NumericValue { get; init; }
    public bool IsUnlimited { get; init; }
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Human-readable value.
    /// </summary>
    public string DisplayValue => GetDisplayValue();

    private string GetDisplayValue()
    {
        if (IsUnlimited) return "Unlimited";
        if (ValueType == "boolean") return BooleanValue == true ? "✓" : "✗";
        return NumericValue?.ToString("N0") ?? "—";
    }
}
