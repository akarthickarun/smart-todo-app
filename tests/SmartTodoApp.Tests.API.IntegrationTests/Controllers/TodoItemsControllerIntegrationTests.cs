using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Tests.API.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for TodoItemsController API endpoints.
/// Tests the full HTTP stack with in-memory database.
/// </summary>
public class TodoItemsControllerIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public TodoItemsControllerIntegrationTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST /api/todoitems Tests

    [Fact]
    public async Task CreateTodoItem_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var futureDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var request = new CreateTodoRequest(
            "Test Todo Item",
            "This is a test description",
            futureDueDate
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var todoId = Guid.Parse(response.Headers.Location!.ToString().Split("/").Last());
        todoId.Should().NotBeEmpty();

        var body = await response.Content.ReadFromJsonAsync<Guid>();
        body.Should().Be(todoId);
    }

    [Fact]
    public async Task CreateTodoItem_WithEmptyTitle_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateTodoRequest(
            "",
            "Description without title",
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task CreateTodoItem_WithTitleTooShort_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateTodoRequest(
            "ab",
            "Too short title",
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoItem_WithTitleTooLong_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateTodoRequest(
            new string('a', 201),
            "Too long title",
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoItem_WithPastDueDate_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateTodoRequest(
            "Valid Title",
            null,
            new DateOnly(2020, 1, 1) // Past date
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/todoitems/{id} Tests

    [Fact]
    public async Task GetTodoItemById_WithValidId_ShouldReturn200Ok()
    {
        // Arrange - First create a todo
        var createRequest = new CreateTodoRequest("Get Test Todo", "Description", null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var todo = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        todo.Should().NotBeNull();
        todo!.Id.Should().Be(createdId);
        todo.Title.Should().Be("Get Test Todo");
        todo.Description.Should().Be("Description");
        todo.Status.Should().Be(TodoStatus.Pending);
    }

    [Fact]
    public async Task GetTodoItemById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/todoitems/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTodoItemById_WithInvalidId_ShouldReturn404NotFound()
    {
        // Act - Invalid GUID format in route doesn't match the {id:guid} constraint
        var response = await _client.GetAsync("/api/todoitems/invalid-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/todoitems Tests

    [Fact]
    public async Task GetAllTodoItems_ShouldReturn200Ok()
    {
        // Arrange - Create multiple todos
        var todo1 = new CreateTodoRequest("Todo 1", null, null);
        var todo2 = new CreateTodoRequest("Todo 2", null, null);
        
        await _client.PostAsJsonAsync("/api/todoitems", todo1);
        await _client.PostAsJsonAsync("/api/todoitems", todo2);

        // Act
        var response = await _client.GetAsync("/api/todoitems");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        todos.Should().NotBeNull();
        todos!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllTodoItems_FilterByPendingStatus_ShouldReturn200Ok()
    {
        // Arrange - Create pending and completed todos
        var pendingTodo = new CreateTodoRequest("Pending Task", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", pendingTodo);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Mark one as complete
        await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Act
        var response = await _client.GetAsync("/api/todoitems?status=Pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        todos.Should().NotBeNull();
        todos!.Should().AllSatisfy(t => t.Status.Should().Be(TodoStatus.Pending));
    }

    [Fact]
    public async Task GetAllTodoItems_FilterByCompletedStatus_ShouldReturn200Ok()
    {
        // Arrange - Create and complete a todo
        var todo = new CreateTodoRequest("Completed Task", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", todo);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Mark as complete
        await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Act
        var response = await _client.GetAsync("/api/todoitems?status=Completed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItemDto>>();
        todos.Should().NotBeNull();
        todos!.Should().Contain(t => t.Id == todoId && t.Status == TodoStatus.Completed);
    }

    #endregion

    #region PUT /api/todoitems/{id} Tests

    [Fact]
    public async Task UpdateTodoItem_WithValidRequest_ShouldReturn200Ok()
    {
        // Arrange - Create a todo
        var createRequest = new CreateTodoRequest("Original Title", "Original Description", null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - Update it
        var updateRequest = new UpdateTodoRequest(
            "Updated Title",
            "Updated Description",
            new DateOnly(2026, 12, 25)
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/todoitems/{todoId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/todoitems/{todoId}");
        var updatedTodo = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        updatedTodo!.Title.Should().Be("Updated Title");
        updatedTodo.Description.Should().Be("Updated Description");
        updatedTodo.DueDate.Should().Be(new DateOnly(2026, 12, 25));
    }

    [Fact]
    public async Task UpdateTodoItem_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateTodoRequest("Title", "Description", null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodoItem_WithEmptyTitle_ShouldReturn400BadRequest()
    {
        // Arrange - Create a todo first
        var createRequest = new CreateTodoRequest("Title", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - Try to update with empty title
        var updateRequest = new UpdateTodoRequest("", null, null);
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{todoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/todoitems/{id} Tests

    [Fact]
    public async Task DeleteTodoItem_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange - Create a todo
        var createRequest = new CreateTodoRequest("To Delete", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/todoitems/{todoId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/todoitems/{todoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoItem_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/todoitems/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PATCH /api/todoitems/{id}/complete Tests

    [Fact]
    public async Task MarkTodoItemComplete_WithValidId_ShouldReturn200Ok()
    {
        // Arrange - Create a todo
        var createRequest = new CreateTodoRequest("To Complete", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's marked complete
        var getResponse = await _client.GetAsync($"/api/todoitems/{todoId}");
        var completedTodo = await getResponse.Content.ReadFromJsonAsync<TodoItemDto>();
        completedTodo!.Status.Should().Be(TodoStatus.Completed);
    }

    [Fact]
    public async Task MarkTodoItemComplete_WithNonExistentId_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PatchAsync($"/api/todoitems/{nonExistentId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkTodoItemComplete_WhenAlreadyComplete_ShouldReturnBadRequest()
    {
        // Arrange - Create and complete a todo
        var createRequest = new CreateTodoRequest("Already Complete", null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", createRequest);
        var todoId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Act - Try to complete again
        var response = await _client.PatchAsync($"/api/todoitems/{todoId}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task BadRequest_ShouldReturnProblemDetails()
    {
        // Arrange
        var invalidRequest = new CreateTodoRequest("", null, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problemDetails.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        problemDetails.GetProperty("type").GetString().Should().Contain("rfc");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
    }

    #endregion
}
