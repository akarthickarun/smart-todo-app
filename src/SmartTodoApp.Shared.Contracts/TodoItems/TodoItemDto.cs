namespace SmartTodoApp.Shared.Contracts.TodoItems;

/// <summary>
/// Shared status enumeration for Todo items.
/// </summary>
public enum TodoStatus : int
{
}

/// <summary>
/// Read model DTO for a Todo item.
/// </summary>
public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    TodoStatus Status,
    DateOnly? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
