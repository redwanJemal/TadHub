using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace SaasKit.Infrastructure.Api;

/// <summary>
/// Middleware that wraps controller responses in standard envelope format.
/// - Raw objects → ApiResponse&lt;T&gt;
/// - PagedList&lt;T&gt; → ApiPagedResponse&lt;T&gt;
/// - Already wrapped → pass through
/// </summary>
public sealed class ApiResponseWrappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiResponseWrappingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ApiResponseWrappingMiddleware(RequestDelegate next, ILogger<ApiResponseWrappingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip non-API routes
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // Capture the original response body
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // Reset position to read the response
        memoryStream.Position = 0;

        // Skip wrapping for non-success responses, streaming, or already-wrapped responses
        if (ShouldSkipWrapping(context))
        {
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
            return;
        }

        // Read and potentially wrap the response
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            context.Response.Body = originalBodyStream;
            return;
        }

        var wrappedResponse = WrapResponse(responseBody, context);
        
        context.Response.Body = originalBodyStream;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync(wrappedResponse);
    }

    private static bool ShouldSkipWrapping(HttpContext context)
    {
        // Skip non-JSON responses
        var contentType = context.Response.ContentType ?? "";
        if (!contentType.Contains("application/json") && !string.IsNullOrEmpty(contentType))
            return true;

        // Skip error responses (handled by GlobalExceptionHandler)
        if (context.Response.StatusCode >= 400)
            return true;

        // Skip if response indicates streaming
        if (context.Response.Headers.ContainsKey("Transfer-Encoding"))
            return true;

        // Skip specific endpoints
        if (context.Request.Path.StartsWithSegments("/api/v1/events")) // SSE
            return true;

        return false;
    }

    private string WrapResponse(string responseBody, HttpContext context)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Already wrapped (has "data" and "meta" properties)
            if (IsAlreadyWrapped(root))
            {
                return responseBody;
            }

            // Check if it's a PagedList structure
            if (IsPagedList(root))
            {
                return WrapPagedList(root, context);
            }

            // Wrap as ApiResponse
            return WrapAsApiResponse(root, context);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse response body for wrapping");
            return responseBody;
        }
    }

    private static bool IsAlreadyWrapped(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object &&
               root.TryGetProperty("data", out _) &&
               root.TryGetProperty("meta", out _);
    }

    private static bool IsPagedList(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object &&
               root.TryGetProperty("items", out _) &&
               root.TryGetProperty("totalCount", out _) &&
               root.TryGetProperty("page", out _);
    }

    private static string WrapPagedList(JsonElement root, HttpContext context)
    {
        var requestId = GetRequestId(context);
        var items = root.GetProperty("items");
        var page = root.GetProperty("page").GetInt32();
        var pageSize = root.GetProperty("pageSize").GetInt32();
        var totalCount = root.GetProperty("totalCount").GetInt32();
        var totalPages = root.GetProperty("totalPages").GetInt32();
        var hasNextPage = root.GetProperty("hasNextPage").GetBoolean();
        var hasPreviousPage = root.GetProperty("hasPreviousPage").GetBoolean();

        var wrapped = new
        {
            data = items,
            meta = new
            {
                timestamp = DateTimeOffset.UtcNow,
                requestId
            },
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages,
                hasNextPage,
                hasPreviousPage
            }
        };

        return JsonSerializer.Serialize(wrapped, JsonOptions);
    }

    private static string WrapAsApiResponse(JsonElement data, HttpContext context)
    {
        var requestId = GetRequestId(context);

        var wrapped = new
        {
            data,
            meta = new
            {
                timestamp = DateTimeOffset.UtcNow,
                requestId
            }
        };

        return JsonSerializer.Serialize(wrapped, JsonOptions);
    }

    private static string GetRequestId(HttpContext context)
    {
        return context.TraceIdentifier.Length > 12 
            ? context.TraceIdentifier[..12] 
            : context.TraceIdentifier;
    }
}
