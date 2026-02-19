using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Extension methods for applying filters to IQueryable.
/// </summary>
public static class QueryableFilterExtensions
{
    /// <summary>
    /// Applies filter fields to the queryable using a field mapping.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to filter.</param>
    /// <param name="filters">List of filter fields from query parameters.</param>
    /// <param name="fieldMap">Maps filter field names to entity property expressions.</param>
    /// <returns>Filtered queryable.</returns>
    /// <remarks>
    /// - Multiple values for same field: OR (IN semantics)
    /// - Multiple fields: AND
    /// - Unknown fields are ignored (validate upstream if needed)
    /// </remarks>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        List<FilterField> filters,
        Dictionary<string, Expression<Func<T, object>>> fieldMap)
    {
        if (filters.Count == 0)
            return query;

        foreach (var filter in filters)
        {
            // Skip unknown fields
            if (!fieldMap.TryGetValue(filter.Name, out var propertyExpression))
                continue;

            var predicate = BuildFilterExpression(propertyExpression, filter);
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
        }

        return query;
    }

    private static Expression<Func<T, bool>>? BuildFilterExpression<T>(
        Expression<Func<T, object>> propertyExpression,
        FilterField filter)
    {
        if (filter.Values.Count == 0 && filter.Operator != FilterOperator.IsNull)
            return null;

        var parameter = propertyExpression.Parameters[0];
        var memberExpression = GetMemberExpression(propertyExpression.Body);
        
        if (memberExpression == null)
            return null;

        var propertyType = GetUnderlyingType(memberExpression.Type);
        
        return filter.Operator switch
        {
            FilterOperator.Eq => BuildEqualityExpression<T>(parameter, memberExpression, filter.Values, propertyType),
            FilterOperator.Gt => BuildComparisonExpression<T>(parameter, memberExpression, filter.Values[0], propertyType, ExpressionType.GreaterThan),
            FilterOperator.Gte => BuildComparisonExpression<T>(parameter, memberExpression, filter.Values[0], propertyType, ExpressionType.GreaterThanOrEqual),
            FilterOperator.Lt => BuildComparisonExpression<T>(parameter, memberExpression, filter.Values[0], propertyType, ExpressionType.LessThan),
            FilterOperator.Lte => BuildComparisonExpression<T>(parameter, memberExpression, filter.Values[0], propertyType, ExpressionType.LessThanOrEqual),
            FilterOperator.Contains => BuildStringExpression<T>(parameter, memberExpression, filter.Values[0], StringMatchType.Contains),
            FilterOperator.StartsWith => BuildStringExpression<T>(parameter, memberExpression, filter.Values[0], StringMatchType.StartsWith),
            FilterOperator.EndsWith => BuildStringExpression<T>(parameter, memberExpression, filter.Values[0], StringMatchType.EndsWith),
            FilterOperator.IsNull => BuildIsNullExpression<T>(parameter, memberExpression, filter.Values),
            _ => null
        };
    }

    private static Expression<Func<T, bool>> BuildEqualityExpression<T>(
        ParameterExpression parameter,
        MemberExpression memberExpression,
        List<string> values,
        Type propertyType)
    {
        if (values.Count == 1)
        {
            // Single value: simple equality
            var constantValue = ConvertValue(values[0], propertyType);
            var constant = Expression.Constant(constantValue, memberExpression.Type);
            var equality = Expression.Equal(memberExpression, constant);
            return Expression.Lambda<Func<T, bool>>(equality, parameter);
        }

        // Multiple values: IN (OR) semantics
        Expression? combined = null;
        foreach (var value in values)
        {
            var constantValue = ConvertValue(value, propertyType);
            var constant = Expression.Constant(constantValue, memberExpression.Type);
            var equality = Expression.Equal(memberExpression, constant);
            combined = combined == null ? equality : Expression.OrElse(combined, equality);
        }

        return Expression.Lambda<Func<T, bool>>(combined!, parameter);
    }

    private static Expression<Func<T, bool>> BuildComparisonExpression<T>(
        ParameterExpression parameter,
        MemberExpression memberExpression,
        string value,
        Type propertyType,
        ExpressionType comparisonType)
    {
        var constantValue = ConvertValue(value, propertyType);
        var constant = Expression.Constant(constantValue, memberExpression.Type);
        var comparison = Expression.MakeBinary(comparisonType, memberExpression, constant);
        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression<Func<T, bool>> BuildStringExpression<T>(
        ParameterExpression parameter,
        MemberExpression memberExpression,
        string value,
        StringMatchType matchType)
    {
        // Use EF.Functions.ILike for case-insensitive matching in PostgreSQL
        var efFunctions = typeof(NpgsqlDbFunctionsExtensions);
        var pattern = matchType switch
        {
            StringMatchType.Contains => $"%{EscapeLikePattern(value)}%",
            StringMatchType.StartsWith => $"{EscapeLikePattern(value)}%",
            StringMatchType.EndsWith => $"%{EscapeLikePattern(value)}",
            _ => value
        };

        // EF.Functions.ILike(property, pattern)
        var iLikeMethod = efFunctions.GetMethod(
            "ILike",
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

        var efFunctionsProperty = typeof(EF).GetProperty(nameof(EF.Functions))!;
        var efFunctionsInstance = Expression.Property(null, efFunctionsProperty);
        
        var call = Expression.Call(
            iLikeMethod,
            efFunctionsInstance,
            memberExpression,
            Expression.Constant(pattern));

        return Expression.Lambda<Func<T, bool>>(call, parameter);
    }

    private static Expression<Func<T, bool>> BuildIsNullExpression<T>(
        ParameterExpression parameter,
        MemberExpression memberExpression,
        List<string> values)
    {
        var isNullCheck = values.Count > 0 && 
            (values[0].Equals("true", StringComparison.OrdinalIgnoreCase) || values[0] == "1");

        var nullConstant = Expression.Constant(null, memberExpression.Type);
        var comparison = isNullCheck
            ? Expression.Equal(memberExpression, nullConstant)
            : Expression.NotEqual(memberExpression, nullConstant);

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static MemberExpression? GetMemberExpression(Expression expression)
    {
        return expression switch
        {
            MemberExpression member => member,
            UnaryExpression { Operand: MemberExpression member } => member,
            _ => null
        };
    }

    private static Type GetUnderlyingType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(Guid))
            return Guid.Parse(value);

        if (targetType == typeof(DateTime))
            return DateTime.Parse(value);

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value);

        if (targetType == typeof(DateOnly))
            return DateOnly.Parse(value);

        if (targetType == typeof(bool))
            return bool.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, ignoreCase: true);

        return Convert.ChangeType(value, targetType);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private enum StringMatchType
    {
        Contains,
        StartsWith,
        EndsWith
    }
}
