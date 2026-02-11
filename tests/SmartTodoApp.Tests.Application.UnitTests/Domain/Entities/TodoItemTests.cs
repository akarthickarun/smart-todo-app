using FluentAssertions;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;

namespace SmartTodoApp.Tests.Application.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for TodoItem domain entity business logic.
/// Verifies factory methods, business rules, and domain behavior.
/// </summary>
public class TodoItemTests
{
    #region Create Factory Method Tests

    [Fact]
    public void Create_WithValidTitleAndDescription_ShouldCreateTodoItem()
    {
        // Arrange
        var title = "Test Todo";
        var description = "Test Description";
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var todoItem = TodoItem.Create(title, description, dueDate);

        // Assert
        todoItem.Should().NotBeNull();
        todoItem.Id.Should().NotBeEmpty();
        todoItem.Title.Should().Be(title);
        todoItem.Description.Should().Be(description);
        todoItem.DueDate.Should().Be(dueDate);
        todoItem.Status.Should().Be(TodoStatus.Pending);
        todoItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todoItem.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithValidTitleOnly_ShouldCreateTodoItemWithNullDescription()
    {
        // Arrange
        var title = "Test Todo";

        // Act
        var todoItem = TodoItem.Create(title, null, null);

        // Assert
        todoItem.Should().NotBeNull();
        todoItem.Title.Should().Be(title);
        todoItem.Description.Should().BeNull();
        todoItem.DueDate.Should().BeNull();
        todoItem.Status.Should().Be(TodoStatus.Pending);
    }

    [Fact]
    public void Create_WithTitleHavingLeadingAndTrailingSpaces_ShouldTrimTitle()
    {
        // Arrange
        var title = "  Test Todo  ";
        var expectedTitle = "Test Todo";

        // Act
        var todoItem = TodoItem.Create(title, null, null);

        // Assert
        todoItem.Title.Should().Be(expectedTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullTitle_ShouldThrowArgumentException(string? title)
    {
        // Act
        var act = () => TodoItem.Create(title!, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*")
            .And.ParamName.Should().Be("title");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var title = new string('a', 201); // Exceeds 200 character limit

        // Act
        var act = () => TodoItem.Create(title, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title must not exceed 200 characters.*")
            .And.ParamName.Should().Be("title");
    }

    [Fact]
    public void Create_WithTitleAtMaxLength_ShouldCreateTodoItem()
    {
        // Arrange
        var title = new string('a', 200); // Exactly 200 characters

        // Act
        var todoItem = TodoItem.Create(title, null, null);

        // Assert
        todoItem.Should().NotBeNull();
        todoItem.Title.Should().HaveLength(200);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Act
        var todoItem1 = TodoItem.Create("Todo 1", null, null);
        var todoItem2 = TodoItem.Create("Todo 2", null, null);

        // Assert
        todoItem1.Id.Should().NotBe(todoItem2.Id);
        todoItem1.Id.Should().NotBeEmpty();
        todoItem2.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldSetStatusToPending()
    {
        // Act
        var todoItem = TodoItem.Create("Test Todo", null, null);

        // Assert
        todoItem.Status.Should().Be(TodoStatus.Pending);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtAndUpdatedAtToSameTime()
    {
        // Act
        var todoItem = TodoItem.Create("Test Todo", null, null);

        // Assert
        todoItem.CreatedAt.Should().Be(todoItem.UpdatedAt);
    }

    #endregion

    #region MarkAsComplete Tests

    [Fact]
    public void MarkAsComplete_WhenPending_ShouldSetStatusToCompleted()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);

        // Act
        todoItem.MarkAsComplete();

        // Assert
        todoItem.Status.Should().Be(TodoStatus.Completed);
    }

    [Fact]
    public void MarkAsComplete_WhenPending_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        var originalUpdatedAt = todoItem.UpdatedAt;
        Thread.Sleep(10); // Ensure time passes

        // Act
        todoItem.MarkAsComplete();

        // Assert
        todoItem.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        todoItem.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsComplete_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        todoItem.MarkAsComplete();

        // Act
        var act = () => todoItem.MarkAsComplete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Todo item is already completed.");
    }

    [Fact]
    public void MarkAsComplete_ShouldNotChangeOtherProperties()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", "Description", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var originalId = todoItem.Id;
        var originalTitle = todoItem.Title;
        var originalDescription = todoItem.Description;
        var originalDueDate = todoItem.DueDate;
        var originalCreatedAt = todoItem.CreatedAt;

        // Act
        todoItem.MarkAsComplete();

        // Assert
        todoItem.Id.Should().Be(originalId);
        todoItem.Title.Should().Be(originalTitle);
        todoItem.Description.Should().Be(originalDescription);
        todoItem.DueDate.Should().Be(originalDueDate);
        todoItem.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var todoItem = TodoItem.Create("Original Title", "Original Description", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        todoItem.UpdateDetails(newTitle, newDescription, newDueDate);

        // Assert
        todoItem.Title.Should().Be(newTitle);
        todoItem.Description.Should().Be(newDescription);
        todoItem.DueDate.Should().Be(newDueDate);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        var originalUpdatedAt = todoItem.UpdatedAt;
        Thread.Sleep(10); // Ensure time passes

        // Act
        todoItem.UpdateDetails("Updated Title", "Updated Description", null);

        // Assert
        todoItem.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        todoItem.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ShouldSetDescriptionToNull()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", "Original Description", null);

        // Act
        todoItem.UpdateDetails("Updated Title", null, null);

        // Assert
        todoItem.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_WithNullDueDate_ShouldSetDueDateToNull()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        // Act
        todoItem.UpdateDetails("Updated Title", null, null);

        // Assert
        todoItem.DueDate.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_WithTitleHavingLeadingAndTrailingSpaces_ShouldTrimTitle()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        var newTitle = "  Updated Title  ";
        var expectedTitle = "Updated Title";

        // Act
        todoItem.UpdateDetails(newTitle, null, null);

        // Assert
        todoItem.Title.Should().Be(expectedTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyOrNullTitle_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);

        // Act
        var act = () => todoItem.UpdateDetails(title!, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*")
            .And.ParamName.Should().Be("title");
    }

    [Fact]
    public void UpdateDetails_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        var newTitle = new string('a', 201); // Exceeds 200 character limit

        // Act
        var act = () => todoItem.UpdateDetails(newTitle, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title must not exceed 200 characters.*")
            .And.ParamName.Should().Be("title");
    }

    [Fact]
    public void UpdateDetails_ShouldNotChangeIdOrCreatedAt()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        var originalId = todoItem.Id;
        var originalCreatedAt = todoItem.CreatedAt;

        // Act
        todoItem.UpdateDetails("Updated Title", "Updated Description", null);

        // Assert
        todoItem.Id.Should().Be(originalId);
        todoItem.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void UpdateDetails_ShouldNotChangeStatus()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Todo", null, null);
        todoItem.MarkAsComplete();
        var originalStatus = todoItem.Status;

        // Act
        todoItem.UpdateDetails("Updated Title", null, null);

        // Assert
        todoItem.Status.Should().Be(originalStatus);
        todoItem.Status.Should().Be(TodoStatus.Completed);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_AndThenUpdate_ShouldMaintainIntegrity()
    {
        // Arrange & Act
        var todoItem = TodoItem.Create("Original", "Desc", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var originalId = todoItem.Id;
        var originalCreatedAt = todoItem.CreatedAt;

        todoItem.UpdateDetails("Updated", "New Desc", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));
        todoItem.MarkAsComplete();

        // Assert
        todoItem.Id.Should().Be(originalId);
        todoItem.CreatedAt.Should().Be(originalCreatedAt);
        todoItem.Title.Should().Be("Updated");
        todoItem.Description.Should().Be("New Desc");
        todoItem.Status.Should().Be(TodoStatus.Completed);
    }

    [Fact]
    public void TodoItem_ShouldHavePrivateConstructor()
    {
        // Assert
        var constructors = typeof(TodoItem).GetConstructors(
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Public);

        var privateConstructor = constructors.FirstOrDefault(c => c.IsPrivate && c.GetParameters().Length == 0);
        
        privateConstructor.Should().NotBeNull("TodoItem should have a parameterless private constructor for EF Core");
    }

    #endregion
}
