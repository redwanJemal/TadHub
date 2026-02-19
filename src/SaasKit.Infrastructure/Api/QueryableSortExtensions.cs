using System.Linq.Expressions;
using SaasKit.SharedKernel.Api;

namespace SaasKit.Infrastructure.Api;

/// <summary>
/// Extension methods for applying sorting to IQueryable.
/// </summary>
public static class QueryableSortExtensions
{
    /// <summary>
    /// Applies sort fields to the queryable using a field mapping.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to sort.</param>
    /// <param name="sortFields">List of sort fields from query parameters.</param>
    /// <param name="fieldMap">Maps sort field names to entity property expressions.</param>
    /// <param name="defaultSort">Default sort expression if no sort fields provided. Defaults to CreatedAt descending if available.</param>
    /// <returns>Sorted queryable.</returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        List<SortField> sortFields,
        Dictionary<string, Expression<Func<T, object>>> fieldMap,
        (Expression<Func<T, object>> Expression, bool Descending)? defaultSort = null)
    {
        // If no sort fields, apply default sort
        if (sortFields.Count == 0)
        {
            if (defaultSort.HasValue)
            {
                return defaultSort.Value.Descending
                    ? query.OrderByDescending(defaultSort.Value.Expression)
                    : query.OrderBy(defaultSort.Value.Expression);
            }

            // Try to find createdAt in fieldMap as default
            if (fieldMap.TryGetValue("createdAt", out var createdAtExpr))
            {
                return query.OrderByDescending(createdAtExpr);
            }

            return query;
        }

        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var sortField in sortFields)
        {
            // Skip unknown fields
            if (!fieldMap.TryGetValue(sortField.Name, out var propertyExpression))
                continue;

            if (orderedQuery == null)
            {
                // First sort field: use OrderBy/OrderByDescending
                orderedQuery = sortField.Descending
                    ? query.OrderByDescending(propertyExpression)
                    : query.OrderBy(propertyExpression);
            }
            else
            {
                // Subsequent fields: use ThenBy/ThenByDescending
                orderedQuery = sortField.Descending
                    ? orderedQuery.ThenByDescending(propertyExpression)
                    : orderedQuery.ThenBy(propertyExpression);
            }
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Applies sort fields using property names parsed from QueryParameters.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        QueryParameters queryParams,
        Dictionary<string, Expression<Func<T, object>>> fieldMap,
        (Expression<Func<T, object>> Expression, bool Descending)? defaultSort = null)
    {
        var sortFields = queryParams.GetSortFields();
        return query.ApplySort(sortFields, fieldMap, defaultSort);
    }
}
