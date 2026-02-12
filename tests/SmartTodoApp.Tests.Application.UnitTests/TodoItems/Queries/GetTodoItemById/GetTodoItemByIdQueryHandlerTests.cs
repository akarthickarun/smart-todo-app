using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Mappings;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Infrastructure.Persistence;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;
using ContractTodoStatus = SmartTodoApp.Shared.Contracts.TodoItems.TodoStatus;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Queries.GetTodoItemById;

public class GetTodoItemByIdQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<GetTodoItemByIdQueryHandler>> _loggerMock;
    private readonly GetTodoItemByIdQueryHandler _handler;

    public GetTodoItemByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _loggerMock = new Mock<ILogger<GetTodoItemByIdQueryHandler>>();
        _handler = new GetTodoItemByIdQueryHandler(_context, _mapper, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingTodoItem_ShouldReturnTodoItemDto()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", "Test Description", new DateOnly(2026, 12, 31));
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemByIdQuery(todoItem.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(todoItem.Id);
        result.Title.Should().Be("Test Title");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be((ContractTodoStatus)DomainTodoStatus.Pending);
        result.DueDate.Should().Be(new DateOnly(2026, 12, 31));
        result.CreatedAt.Should().Be(todoItem.CreatedAt);
        result.UpdatedAt.Should().Be(todoItem.UpdatedAt);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetTodoItemByIdQuery(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*TodoItem*not found*");
    }

    [Fact]
    public async Task Handle_ExistingTodoItem_ShouldLogInformationMessage()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemByIdQuery(todoItem.Id);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching todo item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTodoItem_ShouldLogWarningMessage()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetTodoItemByIdQuery(nonExistentId);

        // Act
        try
        {
            await _handler.Handle(query, CancellationToken.None);
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
    public async Task Handle_CompletedTodoItem_ShouldReturnDtoWithCompletedStatus()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", null, null);
        todoItem.MarkAsComplete();
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemByIdQuery(todoItem.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((ContractTodoStatus)DomainTodoStatus.Completed);
    }

    [Fact]
    public async Task Handle_TodoItemWithoutDescriptionAndDueDate_ShouldReturnDtoWithNullValues()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", null, null);
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemByIdQuery(todoItem.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().BeNull();
        result.DueDate.Should().BeNull();
    }
}
