using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using TadHub.Infrastructure.Api;
using TadHub.SharedKernel.Api;

namespace TadHub.Tests.Unit.Api;

public class QueryParametersModelBinderTests
{
    private readonly QueryParametersModelBinder _binder = new();

    [Fact]
    public async Task BindModelAsync_DefaultValues_ReturnsDefaults()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>());

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        var result = context.Result.Model as QueryParameters;
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task BindModelAsync_WithPagination_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["page"] = "3",
            ["pageSize"] = "50"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Page.Should().Be(3);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task BindModelAsync_PageSizeGreaterThan100_ClampedTo100()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["pageSize"] = "500"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task BindModelAsync_PageLessThan1_ClampedTo1()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["page"] = "0"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Page.Should().Be(1);
    }

    [Fact]
    public async Task BindModelAsync_NegativePage_ClampedTo1()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["page"] = "-5"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Page.Should().Be(1);
    }

    [Fact]
    public async Task BindModelAsync_PageSizeLessThan1_ClampedTo1()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["pageSize"] = "0"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task BindModelAsync_WithPerPageAlias_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["per_page"] = "25"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task BindModelAsync_WithLimitAlias_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["limit"] = "30"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.PageSize.Should().Be(30);
    }

    [Fact]
    public async Task BindModelAsync_PageSizeOverridesAliases_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["pageSize"] = "40",
            ["per_page"] = "25",
            ["limit"] = "30"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.PageSize.Should().Be(40);
    }

    [Fact]
    public async Task BindModelAsync_WithSort_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["sort"] = "-createdAt,name"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Sort.Should().Be("-createdAt,name");
    }

    [Fact]
    public async Task BindModelAsync_WithFields_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["fields"] = "id,name,email"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Fields.Should().Be("id,name,email");
    }

    [Fact]
    public async Task BindModelAsync_WithInclude_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["include"] = "users,roles"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Include.Should().Be("users,roles");
    }

    [Fact]
    public async Task BindModelAsync_WithSearch_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["search"] = "test query"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Search.Should().Be("test query");
    }

    [Fact]
    public async Task BindModelAsync_WithQAlias_ParsesAsSearch()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["q"] = "search term"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Search.Should().Be("search term");
    }

    [Fact]
    public async Task BindModelAsync_WithFilters_ParsesCorrectly()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["filter[status]"] = "active",
            ["filter[amount][gte]"] = "100"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Filters.Should().HaveCount(2);
        result.Filters.Should().Contain(f => f.Name == "status" && f.Operator == FilterOperator.Eq);
        result.Filters.Should().Contain(f => f.Name == "amount" && f.Operator == FilterOperator.Gte);
    }

    [Fact]
    public async Task BindModelAsync_InvalidPageValue_UsesDefault()
    {
        // Arrange
        var context = CreateBindingContext(new Dictionary<string, StringValues>
        {
            ["page"] = "invalid"
        });

        // Act
        await _binder.BindModelAsync(context);

        // Assert
        var result = context.Result.Model as QueryParameters;
        result!.Page.Should().Be(1);
    }

    private static ModelBindingContext CreateBindingContext(Dictionary<string, StringValues> queryValues)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Query = new QueryCollection(queryValues);

        var modelMetadata = new EmptyModelMetadataProvider()
            .GetMetadataForType(typeof(QueryParameters));

        return new DefaultModelBindingContext
        {
            ModelMetadata = modelMetadata,
            ModelName = "queryParameters",
            ModelState = new ModelStateDictionary(),
            ActionContext = new ActionContext
            {
                HttpContext = httpContext
            }
        };
    }
}
