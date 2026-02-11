using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;

namespace SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

/// <summary>
/// Handler for UpdateTodoItemCommand that updates an existing todo item.
/// </summary>
public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateTodoItemCommandHandler> _logger;

    public UpdateTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<UpdateTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Updates an existing todo item.
    /// </summary>
    public async Task Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating todo item with ID: {TodoItemId}",
            request.Id);

        var todoItem = await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (todoItem == null)
        {
            _logger.LogWarning(
                "Todo item not found with ID: {TodoItemId}",
                request.Id);

            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        // Update details
        todoItem.UpdateDetails(request.Title, request.Description, request.DueDate);

        // Update status if changed to Completed
        if (request.Status == TodoStatus.Completed && todoItem.Status != TodoStatus.Completed)
        {
            todoItem.MarkAsComplete();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Todo item updated successfully with ID: {TodoItemId}",
            request.Id);
    }
}
