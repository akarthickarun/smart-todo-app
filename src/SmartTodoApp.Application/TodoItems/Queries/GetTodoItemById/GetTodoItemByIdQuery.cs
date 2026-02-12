using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

/// <summary>
/// Query to fetch a todo item by its ID.
/// </summary>
public record GetTodoItemByIdQuery(Guid Id) : IRequest<TodoItemDto>;
