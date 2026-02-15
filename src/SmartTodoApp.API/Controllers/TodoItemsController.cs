using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.API.Controllers;

/// <summary>
/// Manages todo items
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new todo item
    /// </summary>
    /// <param name="request">The create todo request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the created todo item</returns>
    /// <response code="201">Returns the newly created item ID</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoItemCommand(
            request.Title,
            request.Description,
            request.DueDate);

        var id = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Gets a todo item by its ID
    /// </summary>
    /// <param name="id">The ID of the todo item</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The todo item</returns>
    /// <response code="200">Returns the todo item</response>
    /// <response code="404">If the todo item is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTodoItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing todo item
    /// </summary>
    /// <param name="id">The ID of the todo item to update</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">If the update was successful</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the todo item is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoItemCommand(
            id,
            request.Title,
            request.Description,
            request.DueDate);

        await _mediator.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Deletes a todo item
    /// </summary>
    /// <param name="id">The ID of the todo item to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">If the deletion was successful</response>
    /// <response code="404">If the todo item is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTodoItemCommand(id);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Marks a todo item as complete
    /// </summary>
    /// <param name="id">The ID of the todo item to mark as complete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">If the operation was successful</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the todo item is not found</response>
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkComplete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkTodoItemCompleteCommand(id);
        await _mediator.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Gets all todo items, optionally filtered by status
    /// </summary>
    /// <param name="status">Optional filter by todo status (All, Active, Completed)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of todo items</returns>
    /// <response code="200">Returns the list of todo items</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TodoItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TodoItemDto>>> GetAll(
        [FromQuery] TodoStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetTodoItemsQuery(status);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
