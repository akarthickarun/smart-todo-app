namespace SmartTodoApp.Shared.Contracts.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);
