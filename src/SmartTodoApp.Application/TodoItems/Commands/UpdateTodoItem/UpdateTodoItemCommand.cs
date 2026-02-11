using MediatR;

namespace SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

/// <summary>
/// Command to update an existing todo item.
/// </summary>
public record UpdateTodoItemCommand(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate
) : IRequest;
