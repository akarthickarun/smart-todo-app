using FluentAssertions;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.UpdateTodoItem;

public class UpdateTodoItemCommandValidatorTests
{
    private readonly UpdateTodoItemCommandValidator _validator;

    public UpdateTodoItemCommandValidatorTests()
    {
        _validator = new UpdateTodoItemCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", "Valid Description", null);

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
        var command = new UpdateTodoItemCommand(Guid.Empty, "Valid Title", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Validate_EmptyOrWhitespaceTitle_ShouldHaveValidationError(string title)
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), title, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Title));
    }

    [Theory]
    [InlineData("  a  ")]  // 1 char after trim
    [InlineData(" ab ")]  // 2 chars after trim
    [InlineData("a")]     // 1 char
    [InlineData("ab")]    // 2 chars
    public void Validate_TitleLessThan3CharsAfterTrim_ShouldHaveValidationError(string title)
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), title, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.Title) && 
            x.ErrorMessage.Contains("at least 3 characters"));
    }

    [Fact]
    public void Validate_TitleExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), longTitle, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.Title) && 
            x.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void Validate_TitleExactly200CharsAfterTrim_ShouldBeValid()
    {
        // Arrange
        var title = "  " + new string('a', 200) + "  ";
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), title, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TitleExactly3CharsAfterTrim_ShouldBeValid()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "  abc  ", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DescriptionExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('a', 1001);
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", longDescription, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.Description) && 
            x.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_DescriptionExactly1000Chars_ShouldBeValid()
    {
        // Arrange
        var description = new string('a', 1000);
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", description, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullDescription_ShouldBeValid()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DueDateInPast_ShouldHaveValidationError()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", null, pastDate);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.DueDate) && 
            x.ErrorMessage.Contains("future"));
    }

    [Fact]
    public void Validate_DueDateToday_ShouldBeValid()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", null, today);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DueDateInFuture_ShouldBeValid()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", null, futureDate);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullDueDate_ShouldBeValid()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Valid Title", "Description", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
