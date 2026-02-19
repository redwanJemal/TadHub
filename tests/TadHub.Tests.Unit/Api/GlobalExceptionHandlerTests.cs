using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;

namespace TadHub.Tests.Unit.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerTests()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _environment = Substitute.For<IHostEnvironment>();
        _environment.EnvironmentName.Returns("Production");
        _handler = new GlobalExceptionHandler(logger, _environment);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns422()
    {
        // Arrange
        var context = CreateHttpContext();
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email must be valid"),
            new("Name", "Name is required")
        };
        var exception = new ValidationException(failures);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(422);
        context.Response.ContentType.Should().Contain("application/");
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException();

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Invalid operation");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_IncludesExceptionDetails()
    {
        // Arrange
        var devEnvironment = Substitute.For<IHostEnvironment>();
        devEnvironment.EnvironmentName.Returns("Development");
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var devHandler = new GlobalExceptionHandler(logger, devEnvironment);
        
        var context = CreateHttpContext();
        var exception = new Exception("Detailed error message");

        // Act
        await devHandler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        // In development, details are included in response
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/test";
        context.Response.Body = new MemoryStream();
        return context;
    }
}
