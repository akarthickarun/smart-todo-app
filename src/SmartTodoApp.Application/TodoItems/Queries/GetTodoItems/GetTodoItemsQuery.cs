using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Query to fetch todo items with optional status filter.
/// Uses domain TodoStatus to avoid unsafe casting in the handler.
/// </summary>
public record GetTodoItemsQuery(DomainTodoStatus? Status = null) : IRequest<List<TodoItemDto>>;
