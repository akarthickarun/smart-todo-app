using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Infrastructure.Security;
using SmartTodoApp.Shared.Contracts.Auth;
using System.Security.Cryptography;
using System.Text;

namespace SmartTodoApp.API.Controllers;

/// <summary>
/// Handles authentication operations including login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        TokenGenerator tokenGenerator,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token for authenticated requests</returns>
    /// <response code="200">Returns the JWT token</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="401">If the credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Model validation is handled automatically by data annotations
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // For development/testing purposes, accept any credentials
        // In production, this would validate against a user database
        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        // Generate deterministic userId based on email for consistency across logins
        var userId = GenerateDeterministicUserId(request.Email);
        
        // Extract username from email safely
        var userName = ExtractUserNameFromEmail(request.Email);

        // Generate JWT token
        var token = _tokenGenerator.GenerateToken(userId, request.Email, userName);

        // Get expiry minutes from configuration
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

        _logger.LogInformation("Token generated successfully for user: {Email}", request.Email);

        return Ok(new LoginResponse(
            Token: token,
            TokenType: "Bearer",
            ExpiresIn: expiryMinutes * 60, // Convert minutes to seconds
            UserId: userId,
            Email: request.Email,
            Name: userName
        ));
    }

    /// <summary>
    /// Generates a deterministic GUID from an email address for consistent user identification
    /// </summary>
    private static string GenerateDeterministicUserId(string email)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        
        // Take first 16 bytes to create a GUID
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        
        return new Guid(guidBytes).ToString();
    }

    /// <summary>
    /// Safely extracts username from email address
    /// </summary>
    private static string ExtractUserNameFromEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email.Substring(0, atIndex) : email;
    }
}
