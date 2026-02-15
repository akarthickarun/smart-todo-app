using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Query to fetch todo items with optional status filter.
/// Accepts contract TodoStatus from API layer to keep controllers thin.
/// Handler internally maps to domain TodoStatus for filtering.
/// </summary>
public record GetTodoItemsQuery(TodoStatus? Status = null) : IRequest<List<TodoItemDto>>;
