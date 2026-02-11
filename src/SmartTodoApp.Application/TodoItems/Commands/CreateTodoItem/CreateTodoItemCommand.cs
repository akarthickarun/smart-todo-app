using MediatR;

namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

/// <summary>
/// Command to create a new todo item.
/// </summary>
public record CreateTodoItemCommand(
    string Title,
    string? Description,
    DateOnly? DueDate
) : IRequest<Guid>;
