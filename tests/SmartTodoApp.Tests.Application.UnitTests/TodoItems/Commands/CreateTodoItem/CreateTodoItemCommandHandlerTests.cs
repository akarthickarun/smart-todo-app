using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.CreateTodoItem;

public class CreateTodoItemCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<CreateTodoItemCommandHandler>> _loggerMock;
    private readonly Mock<DbSet<TodoItem>> _todoItemsDbSetMock;
    private readonly CreateTodoItemCommandHandler _handler;

    public CreateTodoItemCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<CreateTodoItemCommandHandler>>();
        _todoItemsDbSetMock = new Mock<DbSet<TodoItem>>();

        _contextMock.Setup(x => x.TodoItems).Returns(_todoItemsDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new CreateTodoItemCommandHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTodoItem()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Test Title", "Test Description", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _todoItemsDbSetMock.Verify(x => x.Add(It.Is<TodoItem>(t => 
            t.Title == "Test Title" && 
            t.Description == "Test Description")), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithDueDate_ShouldCreateTodoItemWithDueDate()
    {
        // Arrange
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new CreateTodoItemCommand("Test Title", null, dueDate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _todoItemsDbSetMock.Verify(x => x.Add(It.Is<TodoItem>(t => 
            t.Title == "Test Title" && 
            t.DueDate == dueDate)), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldLogInformationMessages()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Test Title", null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating todo item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnGeneratedGuid()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Test Title", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("  Test Title  ", "Test Title")]
    [InlineData("Test Title", "Test Title")]
    public async Task Handle_TitleWithWhitespace_ShouldTrimTitle(string input, string expected)
    {
        // Arrange
        var command = new CreateTodoItemCommand(input, null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _todoItemsDbSetMock.Verify(x => x.Add(It.Is<TodoItem>(t => 
            t.Title == expected)), Times.Once);
    }
}
