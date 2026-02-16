using System.ComponentModel.DataAnnotations;

namespace SmartTodoApp.Shared.Contracts.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(1, ErrorMessage = "Password cannot be empty")]
    string Password
);
