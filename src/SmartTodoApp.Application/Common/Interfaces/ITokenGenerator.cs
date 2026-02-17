namespace SmartTodoApp.Application.Common.Interfaces;

/// <summary>
/// Interface for generating JWT tokens for authentication.
/// Used for development/testing purposes to generate valid Bearer tokens.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="userId">The user ID (typically a GUID or string)</param>
    /// <param name="email">The user's email</param>
    /// <param name="name">The user's name</param>
    /// <returns>A valid JWT token string</returns>
    string GenerateToken(string userId, string email, string name);
}
