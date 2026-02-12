using AutoMapper;
using FluentAssertions;
using SmartTodoApp.Application.Common.Mappings;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;
using SmartTodoApp.Shared.Contracts.TodoItems;
using Xunit;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;
using ContractTodoStatus = SmartTodoApp.Shared.Contracts.TodoItems.TodoStatus;

namespace SmartTodoApp.Tests.Application.UnitTests.Common.Mappings;

/// <summary>
/// Unit tests for AutoMapper MappingProfile configuration.
/// </summary>
public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingProfile_ShouldHaveValidConfiguration()
    {
        // Act & Assert
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void TodoItem_ShouldMapToTodoItemDto()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", "Test Description", new DateOnly(2026, 12, 31));
        todoItem.MarkAsComplete();

        // Act
        var dto = _mapper.Map<TodoItemDto>(todoItem);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(todoItem.Id);
        dto.Title.Should().Be("Test Title");
        dto.Description.Should().Be("Test Description");
        dto.Status.Should().Be((ContractTodoStatus)DomainTodoStatus.Completed);
        dto.DueDate.Should().Be(new DateOnly(2026, 12, 31));
        dto.CreatedAt.Should().Be(todoItem.CreatedAt);
        dto.UpdatedAt.Should().Be(todoItem.UpdatedAt);
    }

    [Fact]
    public void TodoItem_WithoutDescriptionAndDueDate_ShouldMapToTodoItemDto()
    {
        // Arrange
        var todoItem = TodoItem.Create("Test Title", null, null);

        // Act
        var dto = _mapper.Map<TodoItemDto>(todoItem);

        // Assert
        dto.Should().NotBeNull();
        dto.Title.Should().Be("Test Title");
        dto.Description.Should().BeNull();
        dto.Status.Should().Be((ContractTodoStatus)DomainTodoStatus.Pending);
        dto.DueDate.Should().BeNull();
    }

    [Fact]
    public void TodoItem_WithPendingStatus_ShouldMapCorrectly()
    {
        // Arrange
        var todoItem = TodoItem.Create("Pending Todo", "This is pending", null);

        // Act
        var dto = _mapper.Map<TodoItemDto>(todoItem);

        // Assert
        dto.Status.Should().Be((ContractTodoStatus)DomainTodoStatus.Pending);
        dto.Title.Should().Be("Pending Todo");
    }

    [Fact]
    public void CreateTodoRequest_ShouldMapToCreateTodoItemCommand()
    {
        // Arrange
        var request = new CreateTodoRequest(
            Title: "Test Title",
            Description: "Test Description",
            DueDate: new DateOnly(2026, 12, 31));

        // Act
        var command = _mapper.Map<SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem.CreateTodoItemCommand>(request);

        // Assert
        command.Should().NotBeNull();
        command.Title.Should().Be("Test Title");
        command.Description.Should().Be("Test Description");
        command.DueDate.Should().Be(new DateOnly(2026, 12, 31));
    }

    [Fact]
    public void CreateTodoRequest_WithoutOptionalFields_ShouldMapToCreateTodoItemCommand()
    {
        // Arrange
        var request = new CreateTodoRequest(
            Title: "Test Title",
            Description: null,
            DueDate: null);

        // Act
        var command = _mapper.Map<SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem.CreateTodoItemCommand>(request);

        // Assert
        command.Should().NotBeNull();
        command.Title.Should().Be("Test Title");
        command.Description.Should().BeNull();
        command.DueDate.Should().BeNull();
    }
}
