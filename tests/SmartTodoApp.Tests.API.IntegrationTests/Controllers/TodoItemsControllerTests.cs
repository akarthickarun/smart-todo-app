using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTodoApp.API.Controllers;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Tests.API.IntegrationTests.Controllers;

/// <summary>
/// Unit tests for TodoItemsController that verify the controller correctly delegates
/// to MediatR handlers. These are unit tests using mocks, not integration tests.
/// For true integration tests, see Web API integration tests using WebApplicationFactory.
/// </summary>
public class TodoItemsControllerUnitTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TodoItemsController _controller;

    public TodoItemsControllerUnitTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TodoItemsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtActionWithId()
    {
        // Arrange
        var request = new CreateTodoRequest("Test Title", "Test Description", new DateOnly(2030, 1, 1));
        var todoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateTodoItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoId);

        // Act
        var result = await _controller.Create(request, token);

        // Assert
        var createdAt = result.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.ActionName.Should().Be(nameof(TodoItemsController.GetById));
        createdAt.RouteValues.Should().ContainKey("id");
        createdAt.RouteValues!["id"].Should().Be(todoId);
        createdAt.Value.Should().Be(todoId);

        _mediatorMock.Verify(m => m.Send(
                It.Is<CreateTodoItemCommand>(c =>
                    c.Title == request.Title &&
                    c.Description == request.Description &&
                    c.DueDate == request.DueDate),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithDto()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var dto = new TodoItemDto(
            todoId,
            "Test Title",
            "Test Description",
            TodoStatus.Pending,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTodoItemByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetById(todoId, token);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(dto);

        _mediatorMock.Verify(m => m.Send(
                It.Is<GetTodoItemByIdQuery>(q => q.Id == todoId),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithCompletedStatus_ShouldReturnOkWithList()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var list = new List<TodoItemDto>
        {
            new(Guid.NewGuid(), "Completed", null, TodoStatus.Completed, null, DateTime.UtcNow, DateTime.UtcNow)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTodoItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetAll(TodoStatus.Completed, token);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(list);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetTodoItemsQuery>(q => q.Status == TodoStatus.Completed),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnOk()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = new UpdateTodoRequest("Updated Title", "Updated Description", new DateOnly(2031, 2, 2));

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateTodoItemCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(todoId, request, token);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mediatorMock.Verify(m => m.Send(
                It.Is<UpdateTodoItemCommand>(c =>
                    c.Id == todoId &&
                    c.Title == request.Title &&
                    c.Description == request.Description &&
                    c.DueDate == request.DueDate),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteTodoItemCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(todoId, token);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mediatorMock.Verify(m => m.Send(
                It.Is<DeleteTodoItemCommand>(c => c.Id == todoId),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }

    [Fact]
    public async Task MarkComplete_ShouldReturnOk()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MarkTodoItemCompleteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkComplete(todoId, token);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mediatorMock.Verify(m => m.Send(
                It.Is<MarkTodoItemCompleteCommand>(c => c.Id == todoId),
                It.Is<CancellationToken>(t => t == token)),
            Times.Once);
    }
}
