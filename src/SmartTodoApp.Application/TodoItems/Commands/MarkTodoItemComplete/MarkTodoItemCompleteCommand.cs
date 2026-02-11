using MediatR;

namespace SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;

/// <summary>
/// Command to mark a todo item as complete.
/// </summary>
public record MarkTodoItemCompleteCommand(Guid Id) : IRequest;
