using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Query to fetch todo items with optional status filter.
/// </summary>
public record GetTodoItemsQuery(TodoStatus? Status = null) : IRequest<List<TodoItemDto>>;
