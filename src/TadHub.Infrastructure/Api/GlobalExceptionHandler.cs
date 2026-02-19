using DataAnnotationsValidationException = System.ComponentModel.DataAnnotations.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.SharedKernel.Api;

namespace TadHub.Infrastructure.Api;

/// <summary>
/// Global exception handler implementing .NET 9 IExceptionHandler.
/// Maps exceptions to RFC 9457 Problem Details format.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error) = MapException(exception, httpContext.Request.Path);

        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);
        return true;
    }

    private (int StatusCode, ApiError Error) MapException(Exception exception, string path)
    {
        return exception switch
        {
            FluentValidationException validationEx => (422, MapValidationException(validationEx, path)),
            DataAnnotationsValidationException validationEx => (422, MapDataAnnotationValidation(validationEx, path)),
            KeyNotFoundException => (404, ApiError.NotFound("The requested resource was not found.", path)),
            UnauthorizedAccessException => (403, ApiError.Forbidden()),
            InvalidOperationException ex => (400, ApiError.BadRequest(ex.Message, path)),
            ArgumentException ex => (400, ApiError.BadRequest(ex.Message, path)),
            OperationCanceledException => (499, CreateError(499, "Client Closed Request", "The client closed the request before the server could respond.", path)),
            _ => (500, CreateInternalError(exception, path))
        };
    }

    private static ApiError MapValidationException(FluentValidationException ex, string path)
    {
        var errors = ex.Errors
            .GroupBy(e => ToCamelCase(e.PropertyName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return ApiError.Validation(errors, path);
    }

    private static ApiError MapDataAnnotationValidation(DataAnnotationsValidationException ex, string path)
    {
        var errors = new Dictionary<string, string[]>();
        
        if (ex.ValidationResult?.MemberNames != null)
        {
            foreach (var member in ex.ValidationResult.MemberNames)
            {
                errors[ToCamelCase(member)] = [ex.ValidationResult.ErrorMessage ?? "Validation failed"];
            }
        }
        else
        {
            errors[""] = [ex.Message];
        }

        return ApiError.Validation(errors, path);
    }

    private ApiError CreateInternalError(Exception exception, string path)
    {
        if (_environment.IsDevelopment())
        {
            return new ApiError
            {
                Type = "https://api.saaskit.dev/errors/internal-error",
                Title = "Internal Server Error",
                Status = 500,
                Detail = exception.Message,
                Instance = path,
                RequestId = Guid.NewGuid().ToString("N")[..12]
            };
        }

        return ApiError.Internal();
    }

    private static ApiError CreateError(int status, string title, string detail, string path) => new()
    {
        Type = $"https://api.saaskit.dev/errors/{title.ToLowerInvariant().Replace(" ", "-")}",
        Title = title,
        Status = status,
        Detail = detail,
        Instance = path,
        RequestId = Guid.NewGuid().ToString("N")[..12]
    };

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
