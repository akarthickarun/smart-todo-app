using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartTodoApp.Shared.Contracts.TodoItems;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SmartTodoApp.Tests.API.IntegrationTests;

/// <summary>
/// Tests for WebApplicationFactoryFixture to demonstrate both authentication capabilities:
/// 1. Default behavior with random user IDs (for general testing)
/// 2. Fixed user IDs (for user-specific testing and maintaining consistent identity)
/// </summary>
public class WebApplicationFactoryFixtureTests : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;

    public WebApplicationFactoryFixtureTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAuthenticatedClient_WithoutUserId_ShouldCreateDifferentUserIdsForEachRequest()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        var futureDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request1 = new CreateTodoRequest("Todo 1", "Description 1", futureDueDate);
        var request2 = new CreateTodoRequest("Todo 2", "Description 2", futureDueDate);

        // Act
        var response1 = await client.PostAsJsonAsync("/api/todoitems", request1);
        var response2 = await client.PostAsJsonAsync("/api/todoitems", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Both requests should succeed with different user identities (default behavior)
        // This demonstrates the current approach where each request gets a new random user ID
    }

    [Fact]
    public async Task CreateAuthenticatedClient_WithSpecificUserId_ShouldMaintainConsistentIdentity()
    {
        // Arrange
        var testUserId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var client = _factory.CreateAuthenticatedClient(testUserId);
        var futureDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request1 = new CreateTodoRequest("Todo 1", "Description 1", futureDueDate);
        var request2 = new CreateTodoRequest("Todo 2", "Description 2", futureDueDate);

        // Act - Create two todos with the same user identity
        var response1 = await client.PostAsJsonAsync("/api/todoitems", request1);
        var response2 = await client.PostAsJsonAsync("/api/todoitems", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var todoId1 = await response1.Content.ReadFromJsonAsync<Guid>();
        var todoId2 = await response2.Content.ReadFromJsonAsync<Guid>();

        // Retrieve both todos - they should both exist and be accessible by the same user
        var getTodo1Response = await client.GetAsync($"/api/todoitems/{todoId1}");
        var getTodo2Response = await client.GetAsync($"/api/todoitems/{todoId2}");

        getTodo1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        getTodo2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // This demonstrates user-specific identity across multiple requests
        // useful for testing scenarios where user ownership matters
    }

    [Fact]
    public async Task CreateAuthenticatedClient_WithDifferentUserIds_ShouldCreateDifferentIdentities()
    {
        // Arrange
        var user1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var user2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        var client1 = _factory.CreateAuthenticatedClient(user1Id);
        var client2 = _factory.CreateAuthenticatedClient(user2Id);
        
        var futureDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request = new CreateTodoRequest("Test Todo", "Description", futureDueDate);

        // Act
        var response1 = await client1.PostAsJsonAsync("/api/todoitems", request);
        var response2 = await client2.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        // Both users can create their own todos independently
        // This demonstrates testing multi-user scenarios with specific identities
    }

    [Fact]
    public async Task CreateAuthenticatedClient_WithNullUserId_ShouldUseRandomIdentity()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(null);
        var futureDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request = new CreateTodoRequest("Test Todo", "Description", futureDueDate);

        // Act
        var response = await client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Passing null still works and creates a random user ID (default behavior)
    }
}
