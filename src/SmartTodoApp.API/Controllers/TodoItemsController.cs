using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;
using SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;
using SmartTodoApp.Shared.Contracts.TodoItems;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;
using ContractTodoStatus = SmartTodoApp.Shared.Contracts.TodoItems.TodoStatus;

namespace SmartTodoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

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

    [HttpGet]
    [ProducesResponseType(typeof(List<TodoItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TodoItemDto>>> GetAll(
        [FromQuery] ContractTodoStatus? status,
        CancellationToken cancellationToken)
    {
        DomainTodoStatus? domainStatus = status.HasValue
            ? (DomainTodoStatus)status.Value
            : null;

        var query = new GetTodoItemsQuery(domainStatus);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
