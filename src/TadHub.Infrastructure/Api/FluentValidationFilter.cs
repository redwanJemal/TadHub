using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Action filter that short-circuits invalid requests with RFC 9457 error response.
/// Runs before controller action if ModelState is invalid.
/// </summary>
public sealed class FluentValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => ToCamelCase(kvp.Key),
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var error = ApiError.Validation(errors, context.HttpContext.Request.Path);
        
        context.Result = new ObjectResult(error)
        {
            StatusCode = 422,
            ContentTypes = { "application/problem+json" }
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No-op
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
