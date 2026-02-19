using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using SaasKit.Infrastructure.Api;
using SaasKit.SharedKernel.Api;

namespace SaasKit.Tests.Unit.Api;

public class FluentValidationFilterTests
{
    private readonly FluentValidationFilter _filter = new();

    [Fact]
    public void OnActionExecuting_ValidModelState_DoesNotSetResult()
    {
        // Arrange
        var context = CreateActionExecutingContext();

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_InvalidModelState_Returns422()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Name", "Name is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().NotBeNull();
        context.Result.Should().BeOfType<ObjectResult>();
        
        var objectResult = (ObjectResult)context.Result!;
        objectResult.StatusCode.Should().Be(422);
        objectResult.Value.Should().BeOfType<ApiError>();
        
        var error = (ApiError)objectResult.Value!;
        error.Status.Should().Be(422);
        error.Title.Should().Be("Validation Failed");
        error.Errors.Should().ContainKey("email");
        error.Errors.Should().ContainKey("name");
    }

    [Fact]
    public void OnActionExecuting_InvalidModelState_ConvertsToCamelCase()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var error = (ApiError)((ObjectResult)context.Result!).Value!;
        error.Errors.Should().ContainKey("firstName");
        error.Errors.Should().NotContainKey("FirstName");
    }

    [Fact]
    public void OnActionExecuting_MultipleErrorsForSameField_GroupsErrors()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Email", "Email must be valid");

        // Act
        _filter.OnActionExecuting(context);

        // Assert
        var error = (ApiError)((ObjectResult)context.Result!).Value!;
        error.Errors!["email"].Should().HaveCount(2);
        error.Errors["email"].Should().Contain("Email is required");
        error.Errors["email"].Should().Contain("Email must be valid");
    }

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/test";

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }
}
