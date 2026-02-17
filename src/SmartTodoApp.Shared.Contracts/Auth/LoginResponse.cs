namespace SmartTodoApp.Shared.Contracts.Auth;

/// <summary>
/// Response model containing JWT token after successful login
/// </summary>
public record LoginResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    string UserId,
    string Email,
    string Name
);
