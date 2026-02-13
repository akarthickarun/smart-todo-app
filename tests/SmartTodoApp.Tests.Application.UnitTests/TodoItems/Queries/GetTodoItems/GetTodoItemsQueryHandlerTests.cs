using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Mappings;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;
using SmartTodoApp.Infrastructure.Persistence;
using ContractTodoStatus = SmartTodoApp.Shared.Contracts.TodoItems.TodoStatus;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Queries.GetTodoItems;

public class GetTodoItemsQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<GetTodoItemsQueryHandler>> _loggerMock;
    private readonly GetTodoItemsQueryHandler _handler;

    public GetTodoItemsQueryHandlerTests()
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

        _loggerMock = new Mock<ILogger<GetTodoItemsQueryHandler>>();
        _handler = new GetTodoItemsQueryHandler(_context, _mapper, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllTodoItemsOrderedByCreatedAtDescending()
    {
        // Arrange
        // Create items with different timestamps to test ordering
        var item1 = TodoItem.Create("First Item", null, null);
        await Task.Delay(10); // Ensure different timestamps
        var item2 = TodoItem.Create("Second Item", null, null);
        await Task.Delay(10);
        var item3 = TodoItem.Create("Third Item", null, null);

        _context.TodoItems.AddRange(item1, item2, item3);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Third Item"); // Most recent first
        result[1].Title.Should().Be("Second Item");
        result[2].Title.Should().Be("First Item"); // Oldest last
        
        // Verify ordering by CreatedAt
        result.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithPendingStatusFilter_ShouldReturnOnlyPendingItems()
    {
        // Arrange
        var pendingItem1 = TodoItem.Create("Pending 1", null, null);
        var pendingItem2 = TodoItem.Create("Pending 2", null, null);
        var completedItem = TodoItem.Create("Completed", null, null);
        completedItem.MarkAsComplete();

        _context.TodoItems.AddRange(pendingItem1, pendingItem2, completedItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(ContractTodoStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Status == ContractTodoStatus.Pending);
    }

    [Fact]
    public async Task Handle_WithCompletedStatusFilter_ShouldReturnOnlyCompletedItems()
    {
        // Arrange
        var pendingItem = TodoItem.Create("Pending", null, null);
        var completedItem1 = TodoItem.Create("Completed 1", null, null);
        completedItem1.MarkAsComplete();
        var completedItem2 = TodoItem.Create("Completed 2", null, null);
        completedItem2.MarkAsComplete();

        _context.TodoItems.AddRange(pendingItem, completedItem1, completedItem2);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(ContractTodoStatus.Completed);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Status == ContractTodoStatus.Completed);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetTodoItemsQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithFilter_ShouldLogInformationWithFilterValue()
    {
        // Arrange
        var query = new GetTodoItemsQuery(ContractTodoStatus.Pending);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching todo items")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogCountOfFetchedItems()
    {
        // Arrange
        var item1 = TodoItem.Create("Item 1", null, null);
        var item2 = TodoItem.Create("Item 2", null, null);
        _context.TodoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetched") && v.ToString()!.Contains("2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMixedStatuses_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var pendingItem = TodoItem.Create("Pending Item", null, null);
        await Task.Delay(10);
        var completedItem = TodoItem.Create("Completed Item", null, null);
        completedItem.MarkAsComplete();
        await Task.Delay(10);
        var anotherPendingItem = TodoItem.Create("Another Pending", null, null);

        _context.TodoItems.AddRange(pendingItem, completedItem, anotherPendingItem);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(x => x.CreatedAt);
    }

    [Fact]
    public async Task Handle_WithPendingFilter_ShouldStillOrderByCreatedAtDescending()
    {
        // Arrange
        var pendingItem1 = TodoItem.Create("Pending 1", null, null);
        await Task.Delay(10);
        var pendingItem2 = TodoItem.Create("Pending 2", null, null);
        await Task.Delay(10);
        var pendingItem3 = TodoItem.Create("Pending 3", null, null);

        _context.TodoItems.AddRange(pendingItem1, pendingItem2, pendingItem3);
        await _context.SaveChangesAsync();

        var query = new GetTodoItemsQuery(ContractTodoStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Pending 3"); // Most recent first
        result[1].Title.Should().Be("Pending 2");
        result[2].Title.Should().Be("Pending 1"); // Oldest last
    }
}
