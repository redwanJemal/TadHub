using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Parses sort parameter string into SortField objects.
/// </summary>
public static class SortParser
{
    /// <summary>
    /// Parses a sort string into a list of SortField objects.
    /// </summary>
    /// <remarks>
    /// Format: "-createdAt,name" 
    /// - Prefix with '-' for descending order
    /// - Comma-separated for multiple fields
    /// Example: "-createdAt,name" → [{ Name="createdAt", Descending=true }, { Name="name", Descending=false }]
    /// </remarks>
    public static List<SortField> Parse(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return [];

        return sort
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseSortField)
            .Where(f => !string.IsNullOrEmpty(f.Name))
            .ToList();
    }

    private static SortField ParseSortField(string field)
    {
        var trimmed = field.Trim();
        
        if (trimmed.StartsWith('-'))
        {
            return new SortField(trimmed[1..], Descending: true);
        }
        
        if (trimmed.StartsWith('+'))
        {
            return new SortField(trimmed[1..], Descending: false);
        }
        
        return new SortField(trimmed, Descending: false);
    }
}
