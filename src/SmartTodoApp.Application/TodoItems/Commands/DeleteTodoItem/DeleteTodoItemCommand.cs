using MediatR;

namespace SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;

/// <summary>
/// Command to delete a todo item.
/// </summary>
public record DeleteTodoItemCommand(Guid Id) : IRequest;
