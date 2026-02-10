using System.ComponentModel.DataAnnotations;

namespace SmartTodoApp.Shared.Contracts.TodoItems;

/// <summary>
/// Request model for updating an existing Todo item.
/// </summary>
public record UpdateTodoRequest(
    [Required]
    [StringLength(200, MinimumLength = 3)]
    string Title,
    
    [StringLength(1000)]
    string? Description = null,
    
    DateOnly? DueDate = null
);
