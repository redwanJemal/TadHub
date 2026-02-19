using System.Linq.Expressions;
using SaasKit.Infrastructure.Api;
using SaasKit.SharedKernel.Api;

namespace SaasKit.Tests.Unit.Api;

public class QueryableExtensionsTests
{
    private record TestEntity(int Id, string Name, string Status, decimal Amount, DateTime CreatedAt, DateTime? DeletedAt);

    private static readonly Dictionary<string, Expression<Func<TestEntity, object>>> FieldMap = new()
    {
        ["id"] = x => x.Id,
        ["name"] = x => x.Name,
        ["status"] = x => x.Status,
        ["amount"] = x => x.Amount,
        ["createdAt"] = x => x.CreatedAt,
        ["deletedAt"] = x => x.DeletedAt!
    };

    private static List<TestEntity> GetTestData() =>
    [
        new(1, "Acme Corp", "active", 100m, new DateTime(2024, 1, 1), null),
        new(2, "Beta Inc", "active", 200m, new DateTime(2024, 1, 2), null),
        new(3, "Gamma Ltd", "pending", 150m, new DateTime(2024, 1, 3), null),
        new(4, "Delta Co", "inactive", 50m, new DateTime(2024, 1, 4), new DateTime(2024, 2, 1)),
        new(5, "Epsilon LLC", "active", 300m, new DateTime(2024, 1, 5), null)
    ];

    #region ApplySort Tests

    [Fact]
    public void ApplySort_SingleDescending_SortsCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField> { new("createdAt", Descending: true) };

        // Act
        var result = data.ApplySort(sortFields, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(5);
        result[0].Id.Should().Be(5);
        result[4].Id.Should().Be(1);
    }

    [Fact]
    public void ApplySort_SingleAscending_SortsCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField> { new("name", Descending: false) };

        // Act
        var result = data.ApplySort(sortFields, FieldMap).ToList();

        // Assert
        result[0].Name.Should().Be("Acme Corp");
        result[4].Name.Should().Be("Gamma Ltd");
    }

    [Fact]
    public void ApplySort_MultipleSortFields_SortsCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField>
        {
            new("status", Descending: false),
            new("amount", Descending: true)
        };

        // Act
        var result = data.ApplySort(sortFields, FieldMap).ToList();

        // Assert
        // First by status (active comes first), then by amount descending
        result[0].Status.Should().Be("active");
        result[0].Amount.Should().Be(300m); // Epsilon LLC
        result[1].Amount.Should().Be(200m); // Beta Inc
        result[2].Amount.Should().Be(100m); // Acme Corp
    }

    [Fact]
    public void ApplySort_EmptySortFields_AppliesDefaultCreatedAtDescending()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField>();

        // Act
        var result = data.ApplySort(sortFields, FieldMap).ToList();

        // Assert
        result[0].Id.Should().Be(5); // Most recent
        result[4].Id.Should().Be(1); // Oldest
    }

    [Fact]
    public void ApplySort_UnknownField_SkipsField()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField>
        {
            new("unknownField", Descending: true),
            new("name", Descending: false)
        };

        // Act
        var result = data.ApplySort(sortFields, FieldMap).ToList();

        // Assert
        result[0].Name.Should().Be("Acme Corp");
    }

    [Fact]
    public void ApplySort_WithCustomDefault_AppliesDefault()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var sortFields = new List<SortField>();
        var defaultSort = (Expression: (Expression<Func<TestEntity, object>>)(x => x.Amount), Descending: true);

        // Act
        var result = data.ApplySort(sortFields, FieldMap, defaultSort).ToList();

        // Assert
        result[0].Amount.Should().Be(300m);
        result[4].Amount.Should().Be(50m);
    }

    #endregion

    #region ApplyFilters Tests (In-Memory - Limited)

    // Note: Full filter tests require EF Core provider for EF.Functions.ILike
    // These tests verify the filter building logic works with simple equality

    [Fact]
    public void ApplyFilters_EmptyFilters_ReturnsUnfiltered()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>();

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public void ApplyFilters_UnknownField_SkipsFilter()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "unknownField", Operator = FilterOperator.Eq, Values = ["value"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public void ApplyFilters_SingleEqValue_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "status", Operator = FilterOperator.Eq, Values = ["active"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => x.Status == "active");
    }

    [Fact]
    public void ApplyFilters_MultipleEqValues_FiltersWithOrSemantics()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "status", Operator = FilterOperator.Eq, Values = ["active", "pending"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(4);
        result.Should().OnlyContain(x => x.Status == "active" || x.Status == "pending");
    }

    [Fact]
    public void ApplyFilters_GteOperator_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "amount", Operator = FilterOperator.Gte, Values = ["150"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => x.Amount >= 150m);
    }

    [Fact]
    public void ApplyFilters_LtOperator_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "amount", Operator = FilterOperator.Lt, Values = ["150"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Amount < 150m);
    }

    [Fact]
    public void ApplyFilters_MultipleFilters_CombinesWithAnd()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "status", Operator = FilterOperator.Eq, Values = ["active"] },
            new() { Name = "amount", Operator = FilterOperator.Gte, Values = ["150"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Status == "active" && x.Amount >= 150m);
    }

    [Fact]
    public void ApplyFilters_DateComparison_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData().AsQueryable();
        var filters = new List<FilterField>
        {
            new() { Name = "createdAt", Operator = FilterOperator.Gte, Values = ["2024-01-03"] }
        };

        // Act
        var result = data.ApplyFilters(filters, FieldMap).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(x => x.CreatedAt >= new DateTime(2024, 1, 3));
    }

    #endregion
}
