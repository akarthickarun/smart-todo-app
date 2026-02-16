using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Infrastructure.Security;
using SmartTodoApp.Shared.Contracts.Auth;

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
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "Email is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "Password is required"
            });
        }

        // For development/testing purposes, accept any credentials
        // In production, this would validate against a user database
        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        // Mock user data - in production this would come from database after validation
        var userId = Guid.NewGuid().ToString();
        var userName = request.Email.Split('@')[0]; // Simple name extraction

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
}
