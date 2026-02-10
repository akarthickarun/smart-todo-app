namespace SmartTodoApp.Shared.Contracts.TodoItems;

/// <summary>
/// Read model DTO for a Todo item.
/// </summary>
public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    int Status,
    DateOnly? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
