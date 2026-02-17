using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SmartTodoApp.Shared.Contracts.Auth;
using SmartTodoApp.Shared.Contracts.TodoItems;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartTodoApp.Tests.API.IntegrationTests.Controllers.Auth;

/// <summary>
/// Integration tests for authentication functionality
/// Tests the actual API endpoints with real HTTP requests
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturn200WithToken()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrWhiteSpace();
        loginResponse.TokenType.Should().Be("Bearer");
        loginResponse.Email.Should().Be(request.Email);
        loginResponse.Name.Should().Be("test");
        loginResponse.ExpiresIn.Should().BeGreaterThan(0);
        
        Guid.TryParse(loginResponse.UserId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_ShouldReturn400()
    {
        // Arrange
        var request = new { Email = "not-an-email", Password = "password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_MissingEmail_ShouldReturn400()
    {
        // Arrange
        var request = new { Password = "password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_MissingPassword_ShouldReturn400()
    {
        // Arrange
        var request = new { Email = "test@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_EmptyPassword_ShouldReturn400()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoItem_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var request = new CreateTodoRequest("Test Todo", "Description", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTodoItem_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        var request = new CreateTodoRequest("Test Todo", "Description", null);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTodoItem_WithValidToken_ShouldReturn201()
    {
        // Arrange
        // Step 1: Login to get a valid token
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Step 2: Create a new client with the token
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Step 3: Attempt to create a todo item
        var createRequest = new CreateTodoRequest("Test Todo", "Description", null);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/todoitems", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var todoId = await response.Content.ReadFromJsonAsync<Guid>();
        todoId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateTodoItem_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var request = new UpdateTodoRequest("Updated Title", "Updated Description", null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{todoId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTodoItem_WithValidToken_ShouldReturn200Or404()
    {
        // Arrange
        // Step 1: Login to get a valid token
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Step 2: Create a new client with the token
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Step 3: Attempt to update a non-existent todo item
        var todoId = Guid.NewGuid();
        var updateRequest = new UpdateTodoRequest("Updated Title", "Updated Description", null);

        // Act
        var response = await authenticatedClient.PutAsJsonAsync($"/api/todoitems/{todoId}", updateRequest);

        // Assert
        // Should be 404 (not found) or 200 (if somehow the GUID exists), but NOT 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoItem_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var todoId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/todoitems/{todoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTodoItem_WithValidToken_ShouldReturn204Or404()
    {
        // Arrange
        // Step 1: Login to get a valid token
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Step 2: Create a new client with the token
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Step 3: Attempt to delete a non-existent todo item
        var todoId = Guid.NewGuid();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/todoitems/{todoId}");

        // Assert
        // Should be 404 (not found) or 204 (if somehow the GUID exists), but NOT 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkComplete_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var todoId = Guid.NewGuid();

        // Act
        var response = await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteWorkflow_CreateReadUpdateDeleteWithAuthentication_ShouldSucceed()
    {
        // This test verifies the complete workflow with authentication

        // Step 1: Login to get a valid token
        var loginRequest = new LoginRequest("workflow@example.com", "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Step 2: Create an authenticated client
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // Step 3: Create a todo item
        var createRequest = new CreateTodoRequest("Workflow Test Todo", "Testing complete workflow", null);
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/todoitems", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Step 4: Read the created todo item
        var getResponse = await authenticatedClient.GetAsync($"/api/todoitems/{todoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var todoItem = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        todoItem.Should().NotBeNull();
        todoItem!.Title.Should().Be("Workflow Test Todo");

        // Step 5: Update the todo item
        var updateRequest = new UpdateTodoRequest("Updated Workflow Todo", "Updated description", null);
        var updateResponse = await authenticatedClient.PutAsJsonAsync($"/api/todoitems/{todoId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Delete the todo item
        var deleteResponse = await authenticatedClient.DeleteAsync($"/api/todoitems/{todoId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 7: Verify deletion
        var getAfterDeleteResponse = await authenticatedClient.GetAsync($"/api/todoitems/{todoId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_SameEmail_ShouldReturnConsistentUserId()
    {
        // Arrange
        var email = "consistent@example.com";
        var request1 = new LoginRequest(email, "password1");
        var request2 = new LoginRequest(email, "password2");

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/login", request1);
        var response2 = await _client.PostAsJsonAsync("/api/auth/login", request2);

        // Assert
        var loginResult1 = await response1.Content.ReadFromJsonAsync<LoginResponse>();
        var loginResult2 = await response2.Content.ReadFromJsonAsync<LoginResponse>();

        loginResult1!.UserId.Should().Be(loginResult2!.UserId, 
            "same email should always generate the same userId");
    }
}
