using FluentAssertions;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Queries.GetTodoItems;

public class GetTodoItemsQueryValidatorTests
{
    private readonly GetTodoItemsQueryValidator _validator;

    public GetTodoItemsQueryValidatorTests()
    {
        _validator = new GetTodoItemsQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_WithNullStatus_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetTodoItemsQuery(null);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidQuery_WithPendingStatus_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetTodoItemsQuery(TodoStatus.Pending);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidQuery_WithCompletedStatus_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetTodoItemsQuery(TodoStatus.Completed);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(99)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Validate_InvalidEnumValue_ShouldHaveValidationError(int invalidStatus)
    {
        // Arrange
        var query = new GetTodoItemsQuery((TodoStatus)invalidStatus);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(query.Status) && 
            x.ErrorMessage.Contains("valid TodoStatus value"));
    }

    [Fact]
    public void Validate_InvalidEnumValue_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var query = new GetTodoItemsQuery((TodoStatus)999);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("0 = Pending")
            .And.Contain("1 = Completed");
    }
}
