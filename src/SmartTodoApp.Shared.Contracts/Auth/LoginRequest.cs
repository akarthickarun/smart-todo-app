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
    // Note: MinLength(1) is intentionally weak for development/testing purposes.
    // In production, use stronger validation (e.g., MinLength(8)) and complexity requirements.
    string Password
);
