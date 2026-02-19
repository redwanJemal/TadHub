using Microsoft.AspNetCore.Mvc.ModelBinding;
using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Model binder for QueryParameters that handles pagination, sorting, filtering, and field selection.
/// </summary>
public class QueryParametersModelBinder : IModelBinder
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MinPage = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var query = bindingContext.HttpContext.Request.Query;

        // Parse pagination
        var page = ParseInt(query["page"], DefaultPage);
        var pageSize = ParseInt(query["pageSize"], DefaultPageSize);
        
        // Also support "per_page" and "limit" as aliases
        if (!query.ContainsKey("pageSize"))
        {
            if (query.ContainsKey("per_page"))
                pageSize = ParseInt(query["per_page"], DefaultPageSize);
            else if (query.ContainsKey("limit"))
                pageSize = ParseInt(query["limit"], DefaultPageSize);
        }

        // Clamp values
        page = Math.Max(MinPage, page);
        pageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);

        // Parse other parameters
        var sort = query["sort"].FirstOrDefault();
        var fields = query["fields"].FirstOrDefault();
        var include = query["include"].FirstOrDefault();
        var search = query["search"].FirstOrDefault() ?? query["q"].FirstOrDefault();

        // Parse filters
        var filters = FilterParser.Parse(query);

        var result = new QueryParameters
        {
            Page = page,
            PageSize = pageSize,
            Sort = sort,
            Fields = fields,
            Include = include,
            Search = search,
            Filters = filters
        };

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}
