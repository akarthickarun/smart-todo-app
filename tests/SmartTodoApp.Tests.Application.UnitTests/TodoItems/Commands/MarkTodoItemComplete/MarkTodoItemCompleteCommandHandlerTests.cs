using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;
using SmartTodoApp.Infrastructure.Persistence;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.MarkTodoItemComplete;

public class MarkTodoItemCompleteCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<MarkTodoItemCompleteCommandHandler>> _loggerMock;
    private readonly MarkTodoItemCompleteCommandHandler _handler;

    public MarkTodoItemCompleteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<MarkTodoItemCompleteCommandHandler>>();
        _handler = new MarkTodoItemCompleteCommandHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldMarkTodoItemAsComplete()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new MarkTodoItemCompleteCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedItem = await _context.TodoItems.FindAsync(existingTodoItem.Id);
        updatedItem.Should().NotBeNull();
        updatedItem!.Status.Should().Be(TodoStatus.Completed);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new MarkTodoItemCompleteCommand(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*TodoItem*not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldLogInformationMessages()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new MarkTodoItemCompleteCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Marking todo item as complete")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("marked as complete successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldLogWarning()
    {
        // Arrange
        var command = new MarkTodoItemCompleteCommand(Guid.NewGuid());

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyCompletedItem_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();
        
        // Mark it as complete first
        existingTodoItem.MarkAsComplete();
        await _context.SaveChangesAsync();

        var command = new MarkTodoItemCompleteCommand(existingTodoItem.Id);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already completed*");
    }
}
