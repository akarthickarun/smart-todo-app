using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.API.Controllers;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Shared.Contracts.Auth;

namespace SmartTodoApp.Tests.API.IntegrationTests.Controllers.Auth;

/// <summary>
/// Unit tests for AuthController to verify login endpoint behavior
/// </summary>
public class AuthControllerUnitTests
{
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerUnitTests()
    {
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        // Setup configuration using in-memory configuration
        var configData = new Dictionary<string, string?>
        {
            { "Jwt:ExpiryMinutes", "60" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _controller = new AuthController(
            _tokenGeneratorMock.Object,
            configuration,
            _loggerMock.Object);
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as LoginResponse;
        response.Should().NotBeNull();
        response!.Token.Should().Be(expectedToken);
        response.TokenType.Should().Be("Bearer");
        response.Email.Should().Be(request.Email);
        response.ExpiresIn.Should().Be(3600); // 60 minutes * 60 seconds
    }

    [Fact]
    public void Login_ValidCredentials_ShouldCallTokenGeneratorWithCorrectParameters()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        _controller.Login(request);

        // Assert
        _tokenGeneratorMock.Verify(
            x => x.GenerateToken(
                It.IsAny<string>(),  // userId (deterministic GUID)
                request.Email,
                "test"),  // username extracted from email
            Times.Once);
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnDeterministicUserId()
    {
        // Arrange
        var email = "test@example.com";
        var request1 = new LoginRequest(email, "password123");
        var request2 = new LoginRequest(email, "different-password");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        var result1 = _controller.Login(request1);
        var result2 = _controller.Login(request2);

        // Assert
        var response1 = (result1.Result as OkObjectResult)!.Value as LoginResponse;
        var response2 = (result2.Result as OkObjectResult)!.Value as LoginResponse;

        response1!.UserId.Should().Be(response2!.UserId, 
            "same email should generate same userId regardless of password");
    }

    [Fact]
    public void Login_DifferentEmails_ShouldReturnDifferentUserIds()
    {
        // Arrange
        var request1 = new LoginRequest("user1@example.com", "password123");
        var request2 = new LoginRequest("user2@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        var result1 = _controller.Login(request1);
        var result2 = _controller.Login(request2);

        // Assert
        var response1 = (result1.Result as OkObjectResult)!.Value as LoginResponse;
        var response2 = (result2.Result as OkObjectResult)!.Value as LoginResponse;

        response1!.UserId.Should().NotBe(response2!.UserId, 
            "different emails should generate different userIds");
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnCorrectUserName()
    {
        // Arrange
        var request = new LoginRequest("john.doe@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;

        response!.Name.Should().Be("john.doe", 
            "name should be extracted from email local part");
    }

    [Fact]
    public void Login_EmailWithoutAtSign_ShouldHandleGracefully()
    {
        // Note: This test validates the edge case handling mentioned in comments
        // In practice, the EmailAddress validation would catch this earlier
        // but the code includes defensive programming for this scenario
        
        // Arrange
        var request = new LoginRequest("invalid-email", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // This test would normally fail validation, but we're testing the logic
        // if it somehow bypasses validation. The method should handle it gracefully.
        
        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;

        // Should return the full email as name if @ not found
        response!.Name.Should().Be("invalid-email");
    }

    [Fact]
    public void Login_ValidCredentials_ShouldLogInformationMessages()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        _controller.Login(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login attempt")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token generated successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnValidGuidAsUserId()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var expectedToken = "test-jwt-token";
        
        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;

        Guid.TryParse(response!.UserId, out _).Should().BeTrue(
            "userId should be a valid GUID");
    }
}
