using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Infrastructure.Persistence;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands.UpdateTodoItem;

public class UpdateTodoItemCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<UpdateTodoItemCommandHandler>> _loggerMock;
    private readonly UpdateTodoItemCommandHandler _handler;

    public UpdateTodoItemCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateTodoItemCommandHandler>>();
        _handler = new UpdateTodoItemCommandHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateTodoItem()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Original Title", "Original Description", null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new UpdateTodoItemCommand(existingTodoItem.Id, "Updated Title", "Updated Description", null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedItem = await _context.TodoItems.FindAsync(existingTodoItem.Id);
        updatedItem.Should().NotBeNull();
        updatedItem!.Title.Should().Be("Updated Title");
        updatedItem.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Title", null, null);

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
        var existingTodoItem = TodoItem.Create("Original Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var command = new UpdateTodoItemCommand(existingTodoItem.Id, "Updated Title", null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updating todo item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("updated successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldLogWarning()
    {
        // Arrange
        var command = new UpdateTodoItemCommand(Guid.NewGuid(), "Title", null, null);

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
    public async Task Handle_UpdateWithDueDate_ShouldUpdateDueDate()
    {
        // Arrange
        var existingTodoItem = TodoItem.Create("Original Title", null, null);
        _context.TodoItems.Add(existingTodoItem);
        await _context.SaveChangesAsync();

        var newDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var command = new UpdateTodoItemCommand(existingTodoItem.Id, "Title", null, newDueDate);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedItem = await _context.TodoItems.FindAsync(existingTodoItem.Id);
        updatedItem!.DueDate.Should().Be(newDueDate);
    }
}
