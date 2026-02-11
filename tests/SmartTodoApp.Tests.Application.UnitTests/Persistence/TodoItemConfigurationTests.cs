using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;
using SmartTodoApp.Infrastructure.Persistence;

namespace SmartTodoApp.Tests.Application.UnitTests.Persistence;

/// <summary>
/// Tests for TodoItem EF Core model configuration to prevent accidental constraint regressions.
/// Uses SQL Server to test relational-specific configurations like column types and value converters.
/// </summary>
public class TodoItemConfigurationTests
{
    private readonly ApplicationDbContext _context;
    private readonly IEntityType _entityType;

    public TodoItemConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SmartTodoAppTest;Trusted_Connection=true;TrustServerCertificate=true")
            .Options;

        _context = new ApplicationDbContext(options);
        _entityType = _context.Model.FindEntityType(typeof(TodoItem))!;
    }

    [Fact]
    public void TodoItem_ShouldBeMappedToTodoItemsTable()
    {
        // Assert
        _entityType.GetTableName().Should().Be("TodoItems");
    }

    [Fact]
    public void TodoItem_ShouldHaveIdAsPrimaryKey()
    {
        // Arrange
        var primaryKey = _entityType.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void Title_ShouldBeRequired()
    {
        // Arrange
        var titleProperty = _entityType.FindProperty("Title");

        // Assert
        titleProperty.Should().NotBeNull();
        titleProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Title_ShouldHaveMaxLength200()
    {
        // Arrange
        var titleProperty = _entityType.FindProperty("Title");

        // Assert
        titleProperty.Should().NotBeNull();
        titleProperty!.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void Description_ShouldBeNullable()
    {
        // Arrange
        var descriptionProperty = _entityType.FindProperty("Description");

        // Assert
        descriptionProperty.Should().NotBeNull();
        descriptionProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void Description_ShouldHaveMaxLength1000()
    {
        // Arrange
        var descriptionProperty = _entityType.FindProperty("Description");

        // Assert
        descriptionProperty.Should().NotBeNull();
        descriptionProperty!.GetMaxLength().Should().Be(1000);
    }

    [Fact]
    public void Status_ShouldBeRequired()
    {
        // Arrange
        var statusProperty = _entityType.FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        statusProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Status_ShouldHaveDefaultValuePending()
    {
        // Arrange
        var statusProperty = _entityType.FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        var defaultValue = statusProperty!.GetDefaultValue();
        defaultValue.Should().NotBeNull();
        defaultValue.Should().Be(TodoStatus.Pending);
    }

    [Fact]
    public void Status_ShouldBeStoredAsInt()
    {
        // Arrange
        var statusProperty = _entityType.FindProperty("Status");

        // Assert
        statusProperty.Should().NotBeNull();
        statusProperty!.ClrType.Should().Be(typeof(TodoStatus));
        
        // Verify that the Status enum is stored as int in the database
        // In EF Core 10, enums may be stored as int by default without explicit converter
        // Check the type mapping instead
        var typeMapping = statusProperty.FindTypeMapping();
        typeMapping.Should().NotBeNull();
        
        // The provider type should be compatible with int storage
        // Either through value converter or direct type mapping
        var valueConverter = statusProperty.GetValueConverter();
        if (valueConverter != null)
        {
            valueConverter.ProviderClrType.Should().Be(typeof(int));
        }
        else
        {
            // If no explicit converter, EF Core handles enum-to-int conversion automatically
            // Verify that Status is an enum (which EF Core stores as int by default)
            statusProperty.ClrType.IsEnum.Should().BeTrue();
        }
    }

    [Fact]
    public void DueDate_ShouldBeMappedToDateColumnType()
    {
        // Arrange
        var dueDateProperty = _entityType.FindProperty("DueDate");

        // Assert
        dueDateProperty.Should().NotBeNull();
        dueDateProperty!.GetColumnType().Should().Be("date");
    }

    [Fact]
    public void DueDate_ShouldBeNullable()
    {
        // Arrange
        var dueDateProperty = _entityType.FindProperty("DueDate");

        // Assert
        dueDateProperty.Should().NotBeNull();
        dueDateProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void CreatedAt_ShouldBeRequired()
    {
        // Arrange
        var createdAtProperty = _entityType.FindProperty("CreatedAt");

        // Assert
        createdAtProperty.Should().NotBeNull();
        createdAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void UpdatedAt_ShouldBeRequired()
    {
        // Arrange
        var updatedAtProperty = _entityType.FindProperty("UpdatedAt");

        // Assert
        updatedAtProperty.Should().NotBeNull();
        updatedAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void TodoItem_ShouldHaveStatusIndex()
    {
        // Arrange
        var indexes = _entityType.GetIndexes();
        var statusIndex = indexes.FirstOrDefault(i => 
            i.Properties.Count == 1 && 
            i.Properties.First().Name == "Status");

        // Assert
        statusIndex.Should().NotBeNull();
        statusIndex!.GetDatabaseName().Should().Be("IX_TodoItems_Status");
    }

    [Fact]
    public void TodoItem_ShouldHaveCreatedAtIndex()
    {
        // Arrange
        var indexes = _entityType.GetIndexes();
        var createdAtIndex = indexes.FirstOrDefault(i => 
            i.Properties.Count == 1 && 
            i.Properties.First().Name == "CreatedAt");

        // Assert
        createdAtIndex.Should().NotBeNull();
        createdAtIndex!.GetDatabaseName().Should().Be("IX_TodoItems_CreatedAt");
    }

    [Fact]
    public void TodoItem_ShouldHaveExactlyTwoIndexes()
    {
        // Arrange
        var indexes = _entityType.GetIndexes();

        // Assert
        indexes.Should().HaveCount(2);
    }
}
