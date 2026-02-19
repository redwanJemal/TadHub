using SaasKit.Infrastructure.Api;

namespace SaasKit.Tests.Unit.Api;

public class SortParserTests
{
    [Fact]
    public void Parse_SingleDescendingField_ReturnsCorrectSortField()
    {
        // Arrange
        var sort = "-createdAt";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("createdAt");
        result[0].Descending.Should().BeTrue();
    }

    [Fact]
    public void Parse_SingleAscendingField_ReturnsCorrectSortField()
    {
        // Arrange
        var sort = "name";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("name");
        result[0].Descending.Should().BeFalse();
    }

    [Fact]
    public void Parse_ExplicitPlusPrefix_ReturnsAscendingField()
    {
        // Arrange
        var sort = "+name";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("name");
        result[0].Descending.Should().BeFalse();
    }

    [Fact]
    public void Parse_MultipleFields_ReturnsAllSortFields()
    {
        // Arrange
        var sort = "-createdAt,name";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("createdAt");
        result[0].Descending.Should().BeTrue();
        result[1].Name.Should().Be("name");
        result[1].Descending.Should().BeFalse();
    }

    [Fact]
    public void Parse_MultipleFieldsWithMixedOrder_ReturnsCorrectOrder()
    {
        // Arrange
        var sort = "status,-updatedAt,+name";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("status");
        result[0].Descending.Should().BeFalse();
        result[1].Name.Should().Be("updatedAt");
        result[1].Descending.Should().BeTrue();
        result[2].Name.Should().Be("name");
        result[2].Descending.Should().BeFalse();
    }

    [Fact]
    public void Parse_NullSort_ReturnsEmptyList()
    {
        // Act
        var result = SortParser.Parse(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptySort_ReturnsEmptyList()
    {
        // Act
        var result = SortParser.Parse("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhitespaceSort_ReturnsEmptyList()
    {
        // Act
        var result = SortParser.Parse("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_FieldsWithSpaces_TrimsCorrectly()
    {
        // Arrange
        var sort = " -createdAt , name ";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("createdAt");
        result[1].Name.Should().Be("name");
    }

    [Fact]
    public void Parse_EmptyFieldBetweenCommas_SkipsEmptyFields()
    {
        // Arrange
        var sort = "-createdAt,,name";

        // Act
        var result = SortParser.Parse(sort);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("createdAt");
        result[1].Name.Should().Be("name");
    }
}
