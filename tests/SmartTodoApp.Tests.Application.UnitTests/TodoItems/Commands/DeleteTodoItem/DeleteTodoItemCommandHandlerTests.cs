using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Infrastructure.Persistence;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.DeleteTodoItem;

public class DeleteTodoItemCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<DeleteTodoItemCommandHandler>> _loggerMock;
    private readonly DeleteTodoItemCommandHandler _handler;

    public DeleteTodoItemCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeleteTodoItemCommandHandler>>();
        _handler = new DeleteTodoItemCommandHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteTodoItem()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", "Test Description", null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new DeleteTodoItemCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedItem = await _context.TodoItems.FindAsync(existingTodoItem.Id);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteTodoItemCommand(Guid.NewGuid());

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

        var command = new DeleteTodoItemCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting todo item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("deleted successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldLogWarning()
    {
        // Arrange
        var command = new DeleteTodoItemCommand(Guid.NewGuid());

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
    public async Task Handle_ValidCommand_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        // Clear change tracker to ensure we're testing the save
        _context.ChangeTracker.Clear();

        var command = new DeleteTodoItemCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify by checking the item is actually removed from database
        var itemCount = await _context.TodoItems.CountAsync();
        itemCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DeleteMultipleTimes_ShouldThrowNotFoundExceptionOnSecondAttempt()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new DeleteTodoItemCommand(existingTodoItem.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
