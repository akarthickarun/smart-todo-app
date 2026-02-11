using MediatR;
using SmartTodoApp.Domain.Enums;

namespace SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

/// <summary>
/// Command to update an existing todo item.
/// </summary>
public record UpdateTodoItemCommand(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateOnly? DueDate
) : IRequest;
