using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Query to fetch todo items with optional status filter.
/// Uses domain TodoStatus to maintain type safety and avoid casting.
/// API controllers should map from contract TodoStatus to domain TodoStatus when creating this query.
/// AutoMapper handles the reverse mapping from domain to contract in the response DTO.
/// </summary>
public record GetTodoItemsQuery(DomainTodoStatus? Status = null) : IRequest<List<TodoItemDto>>;
