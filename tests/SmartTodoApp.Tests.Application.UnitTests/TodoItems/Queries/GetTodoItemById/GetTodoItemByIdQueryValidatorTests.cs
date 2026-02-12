using FluentAssertions;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Queries.GetTodoItemById;

public class GetTodoItemByIdQueryValidatorTests
{
    private readonly GetTodoItemByIdQueryValidator _validator;

    public GetTodoItemByIdQueryValidatorTests()
    {
        _validator = new GetTodoItemByIdQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetTodoItemByIdQuery(Guid.NewGuid());

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        var query = new GetTodoItemByIdQuery(Guid.Empty);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(query.Id));
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveCorrectErrorMessage()
    {
        // Arrange
        var query = new GetTodoItemByIdQuery(Guid.Empty);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Id is required and must not be empty");
    }
}
