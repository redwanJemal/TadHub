using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using SaasKit.SharedKernel.Api;

namespace SaasKit.Infrastructure.Api;

/// <summary>
/// Parses bracket notation filters from query string.
/// Supports: filter[field]=value, filter[field][operator]=value
/// </summary>
public static partial class FilterParser
{
    // Matches: filter[fieldName] or filter[fieldName][operator]
    [GeneratedRegex(@"^filter\[([^\]]+)\](?:\[([^\]]+)\])?$", RegexOptions.Compiled)]
    private static partial Regex FilterKeyPattern();

    private static readonly Dictionary<string, FilterOperator> OperatorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eq"] = FilterOperator.Eq,
        ["gt"] = FilterOperator.Gt,
        ["gte"] = FilterOperator.Gte,
        ["lt"] = FilterOperator.Lt,
        ["lte"] = FilterOperator.Lte,
        ["contains"] = FilterOperator.Contains,
        ["startswith"] = FilterOperator.StartsWith,
        ["endswith"] = FilterOperator.EndsWith,
        ["isnull"] = FilterOperator.IsNull
    };

    /// <summary>
    /// Parses filter parameters from query collection.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - filter[status]=active&amp;filter[status]=pending → FilterField { Name="status", Values=["active","pending"] }
    /// - filter[amount][gte]=100 → FilterField { Name="amount", Operator=Gte, Values=["100"] }
    /// - filter[name][contains]=acme → FilterField { Name="name", Operator=Contains, Values=["acme"] }
    /// - filter[deletedAt][isNull]=true → FilterField { Name="deletedAt", Operator=IsNull, Values=["true"] }
    /// </remarks>
    public static List<FilterField> Parse(IQueryCollection query)
    {
        var filterGroups = new Dictionary<(string Name, FilterOperator Op), List<string>>();

        foreach (var key in query.Keys)
        {
            var match = FilterKeyPattern().Match(key);
            if (!match.Success)
                continue;

            var fieldName = match.Groups[1].Value;
            var operatorStr = match.Groups[2].Success ? match.Groups[2].Value : "eq";
            
            if (!OperatorMap.TryGetValue(operatorStr, out var filterOp))
                filterOp = FilterOperator.Eq;

            var groupKey = (fieldName, filterOp);
            
            if (!filterGroups.TryGetValue(groupKey, out var values))
            {
                values = [];
                filterGroups[groupKey] = values;
            }

            // Add all values for this key (supports multiple: filter[status]=active&filter[status]=pending)
            var queryValues = query[key];
            foreach (var value in queryValues)
            {
                if (!string.IsNullOrEmpty(value))
                    values.Add(value);
            }
        }

        return filterGroups
            .Where(g => g.Value.Count > 0)
            .Select(g => new FilterField
            {
                Name = g.Key.Name,
                Operator = g.Key.Op,
                Values = g.Value
            })
            .ToList();
    }
}
