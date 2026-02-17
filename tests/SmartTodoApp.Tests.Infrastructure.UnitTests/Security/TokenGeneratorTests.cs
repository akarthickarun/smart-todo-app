using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SmartTodoApp.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartTodoApp.Tests.Infrastructure.UnitTests.Security;

/// <summary>
/// Unit tests for TokenGenerator to verify token generation and claim creation
/// </summary>
public class TokenGeneratorTests
{
    private const string TestKey = "test-secret-key-min-32-characters-long-for-jwt";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    private const int TestExpiryMinutes = 60;

    [Fact]
    public void GenerateToken_ValidParameters_ShouldReturnNonEmptyToken()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ValidParameters_ShouldReturnValidJwtToken()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_ValidParameters_ShouldIncludeCorrectClaims()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == name);
    }

    [Fact]
    public void GenerateToken_ValidParameters_ShouldIncludeCorrectIssuer()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(TestIssuer);
    }

    [Fact]
    public void GenerateToken_ValidParameters_ShouldIncludeCorrectAudience()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Audiences.Should().Contain(TestAudience);
    }

    [Fact]
    public void GenerateToken_ValidParameters_ShouldSetCorrectExpiry()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = tokenGenerator.GenerateToken(userId, email, name);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiry = beforeGeneration.AddMinutes(TestExpiryMinutes);
        var maxExpiry = afterGeneration.AddMinutes(TestExpiryMinutes);
        
        // Use precision tolerance for time comparison
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(null, "issuer", "audience", 60)]
    [InlineData("key", null, "audience", 60)]
    [InlineData("key", "issuer", null, 60)]
    public void Constructor_NullParameters_ShouldThrowArgumentNullException(
        string? key, string? issuer, string? audience, int expiryMinutes)
    {
        // Act
        Action act = () => new TokenGenerator(key!, issuer!, audience!, expiryMinutes);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromConfiguration_ValidConfiguration_ShouldCreateTokenGenerator()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:Key", TestKey },
            { "Jwt:Issuer", TestIssuer },
            { "Jwt:Audience", TestAudience },
            { "Jwt:ExpiryMinutes", TestExpiryMinutes.ToString() }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var tokenGenerator = TokenGenerator.FromConfiguration(configuration);

        // Assert
        tokenGenerator.Should().NotBeNull();
        
        // Verify it can generate a token
        var token = tokenGenerator.GenerateToken("test-id", "test@example.com", "Test User");
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void FromConfiguration_MissingKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:Issuer", TestIssuer },
            { "Jwt:Audience", TestAudience },
            { "Jwt:ExpiryMinutes", TestExpiryMinutes.ToString() }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        Action act = () => TokenGenerator.FromConfiguration(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Key*");
    }

    [Fact]
    public void FromConfiguration_MissingExpiryMinutes_ShouldUseDefault()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:Key", TestKey },
            { "Jwt:Issuer", TestIssuer },
            { "Jwt:Audience", TestAudience }
            // ExpiryMinutes not provided
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var tokenGenerator = TokenGenerator.FromConfiguration(configuration);

        // Assert
        tokenGenerator.Should().NotBeNull();
        
        // Verify token is generated with default expiry (60 minutes)
        var token = tokenGenerator.GenerateToken("test-id", "test@example.com", "Test User");
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_MultipleCalls_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var name = "Test User";

        // Act
        var token1 = tokenGenerator.GenerateToken(userId, email, name);
        Thread.Sleep(1000); // Ensure different timestamps
        var token2 = tokenGenerator.GenerateToken(userId, email, name);

        // Assert
        token1.Should().NotBe(token2, "tokens should be different due to different expiry times");
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var tokenGenerator = new TokenGenerator(TestKey, TestIssuer, TestAudience, TestExpiryMinutes);
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        // Act
        var token1 = tokenGenerator.GenerateToken(userId1, "user1@example.com", "User One");
        var token2 = tokenGenerator.GenerateToken(userId2, "user2@example.com", "User Two");

        // Assert
        token1.Should().NotBe(token2);
        
        // Verify claims are different
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);
        
        var claim1 = jwtToken1.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var claim2 = jwtToken2.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        
        claim1.Should().NotBe(claim2);
    }
}
