using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTodoApp.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTodoApp.Infrastructure.Security;

/// <summary>
/// Utility for generating JWT tokens for authentication.
/// Used for development/testing purposes to generate valid Bearer tokens.
/// </summary>
public class TokenGenerator : ITokenGenerator
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public TokenGenerator(string key, string issuer, string audience, int expiryMinutes)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
        _expiryMinutes = expiryMinutes;
    }

    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="userId">The user ID (typically a GUID or string)</param>
    /// <param name="email">The user's email</param>
    /// <param name="name">The user's name</param>
    /// <returns>A valid JWT token string</returns>
    public string GenerateToken(string userId, string email, string name)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Factory method to create a TokenGenerator from configuration.
    /// </summary>
    public static TokenGenerator FromConfiguration(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured");
        
        if (!int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes))
        {
            expiryMinutes = 60; // Default to 60 minutes
        }

        return new TokenGenerator(key, issuer, audience, expiryMinutes);
    }
}
