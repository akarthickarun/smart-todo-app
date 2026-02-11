using FluentAssertions;
using SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.MarkTodoItemComplete;

public class MarkTodoItemCompleteCommandValidatorTests
{
    private readonly MarkTodoItemCompleteCommandValidator _validator;

    public MarkTodoItemCompleteCommandValidatorTests()
    {
        _validator = new MarkTodoItemCompleteCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new MarkTodoItemCompleteCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new MarkTodoItemCompleteCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.Id) && 
            x.ErrorMessage.Contains("required"));
    }
}
