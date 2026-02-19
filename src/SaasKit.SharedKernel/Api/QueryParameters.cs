namespace SaasKit.SharedKernel.Api;

/// <summary>
/// Query parameters for list endpoints.
/// Supports pagination, sorting, filtering, field selection, and includes.
/// </summary>
public class QueryParameters
{
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>
    /// Current page number (1-indexed). Minimum: 1.
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = Math.Max(1, value);
    }

    /// <summary>
    /// Number of items per page. Min: 1, Max: 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>
    /// Maximum allowed page size. Can be overridden for specific endpoints.
    /// </summary>
    public virtual int MaxPageSize => 100;

    /// <summary>
    /// Sort specification. Format: "-createdAt,name" (- prefix for descending).
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// Comma-separated list of fields to include in response.
    /// </summary>
    public string? Fields { get; set; }

    /// <summary>
    /// Comma-separated list of related resources to include.
    /// </summary>
    public string? Include { get; set; }

    /// <summary>
    /// Full-text search query.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Parsed filter conditions.
    /// </summary>
    public List<FilterField> Filters { get; set; } = [];

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Parses the Sort string into SortField objects.
    /// </summary>
    public List<SortField> GetSortFields()
    {
        if (string.IsNullOrWhiteSpace(Sort))
            return [];

        return Sort.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                var trimmed = s.Trim();
                var descending = trimmed.StartsWith('-');
                var name = descending ? trimmed[1..] : trimmed;
                return new SortField(name, descending);
            })
            .ToList();
    }

    /// <summary>
    /// Parses the Fields string into a list of field names.
    /// </summary>
    public List<string> GetFieldsList()
    {
        if (string.IsNullOrWhiteSpace(Fields))
            return [];

        return Fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();
    }

    /// <summary>
    /// Parses the Include string into a list of relation names.
    /// </summary>
    public List<string> GetIncludeList()
    {
        if (string.IsNullOrWhiteSpace(Include))
            return [];

        return Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .ToList();
    }
}

/// <summary>
/// Represents a single sort field.
/// </summary>
/// <param name="Name">The field name to sort by.</param>
/// <param name="Descending">True for descending order, false for ascending.</param>
public sealed record SortField(string Name, bool Descending);

/// <summary>
/// Represents a single filter condition.
/// </summary>
public sealed class FilterField
{
    /// <summary>
    /// The field name to filter on.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The filter operator.
    /// </summary>
    public FilterOperator Operator { get; init; } = FilterOperator.Eq;

    /// <summary>
    /// The filter values (multiple values for IN/OR semantics).
    /// </summary>
    public List<string> Values { get; init; } = [];
}

/// <summary>
/// Filter operators for query parameters.
/// </summary>
public enum FilterOperator
{
    /// <summary>Equality (default). Multiple values = IN/OR.</summary>
    Eq,
    /// <summary>Greater than.</summary>
    Gt,
    /// <summary>Greater than or equal.</summary>
    Gte,
    /// <summary>Less than.</summary>
    Lt,
    /// <summary>Less than or equal.</summary>
    Lte,
    /// <summary>Contains (ILIKE %value%).</summary>
    Contains,
    /// <summary>Starts with (ILIKE value%).</summary>
    StartsWith,
    /// <summary>Ends with (ILIKE %value).</summary>
    EndsWith,
    /// <summary>Is null check.</summary>
    IsNull
}
