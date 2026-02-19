using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using TadHub.Infrastructure.Api;
using TadHub.SharedKernel.Api;

namespace TadHub.Tests.Unit.Api;

public class FilterParserTests
{
    [Fact]
    public void Parse_SingleValueFilter_ReturnsFilterFieldWithEqOperator()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[status]"] = "active"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("status");
        result[0].Operator.Should().Be(FilterOperator.Eq);
        result[0].Values.Should().ContainSingle().Which.Should().Be("active");
    }

    [Fact]
    public void Parse_MultipleValuesForSameFilter_ReturnsFilterFieldWithAllValues()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[status]"] = new StringValues(["active", "pending"])
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("status");
        result[0].Operator.Should().Be(FilterOperator.Eq);
        result[0].Values.Should().BeEquivalentTo(["active", "pending"]);
    }

    [Fact]
    public void Parse_GteOperator_ReturnsFilterFieldWithGteOperator()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[amount][gte]"] = "100"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("amount");
        result[0].Operator.Should().Be(FilterOperator.Gte);
        result[0].Values.Should().ContainSingle().Which.Should().Be("100");
    }

    [Fact]
    public void Parse_ContainsOperator_ReturnsFilterFieldWithContainsOperator()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[name][contains]"] = "acme"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("name");
        result[0].Operator.Should().Be(FilterOperator.Contains);
        result[0].Values.Should().ContainSingle().Which.Should().Be("acme");
    }

    [Fact]
    public void Parse_IsNullOperator_ReturnsFilterFieldWithIsNullOperator()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[deletedAt][isNull]"] = "true"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("deletedAt");
        result[0].Operator.Should().Be(FilterOperator.IsNull);
        result[0].Values.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [InlineData("gt", FilterOperator.Gt)]
    [InlineData("gte", FilterOperator.Gte)]
    [InlineData("lt", FilterOperator.Lt)]
    [InlineData("lte", FilterOperator.Lte)]
    [InlineData("contains", FilterOperator.Contains)]
    [InlineData("startswith", FilterOperator.StartsWith)]
    [InlineData("endswith", FilterOperator.EndsWith)]
    [InlineData("isnull", FilterOperator.IsNull)]
    public void Parse_AllOperators_ReturnsCorrectOperator(string operatorStr, FilterOperator expected)
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            [$"filter[field][{operatorStr}]"] = "value"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Operator.Should().Be(expected);
    }

    [Fact]
    public void Parse_MultipleFilters_ReturnsAllFilterFields()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[status]"] = "active",
            ["filter[amount][gte]"] = "100",
            ["filter[name][contains]"] = "acme"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(f => f.Name == "status" && f.Operator == FilterOperator.Eq);
        result.Should().Contain(f => f.Name == "amount" && f.Operator == FilterOperator.Gte);
        result.Should().Contain(f => f.Name == "name" && f.Operator == FilterOperator.Contains);
    }

    [Fact]
    public void Parse_NonFilterParameters_AreIgnored()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["page"] = "1",
            ["pageSize"] = "10",
            ["sort"] = "-createdAt",
            ["filter[status]"] = "active"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("status");
    }

    [Fact]
    public void Parse_EmptyQuery_ReturnsEmptyList()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>());

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_CaseInsensitiveOperators_ReturnsCorrectOperator()
    {
        // Arrange
        var query = CreateQueryCollection(new Dictionary<string, StringValues>
        {
            ["filter[field][GTE]"] = "100"
        });

        // Act
        var result = FilterParser.Parse(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Operator.Should().Be(FilterOperator.Gte);
    }

    private static IQueryCollection CreateQueryCollection(Dictionary<string, StringValues> values)
    {
        return new QueryCollection(values);
    }
}
