using System.ComponentModel.DataAnnotations;

namespace SmartTodoApp.Shared.Contracts.TodoItems;

/// <summary>
/// Request model for creating a new Todo item.
/// </summary>
public record CreateTodoRequest(
    [Required]
    [StringLength(200, MinimumLength = 3)]
    string Title,
    
    [StringLength(1000)]
    string? Description = null,
    
    DateOnly? DueDate = null
);
