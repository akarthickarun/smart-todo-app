using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Application.TodoItems.Commands.DeleteTodoItem;

/// <summary>
/// Handler for DeleteTodoItemCommand that deletes a todo item.
/// </summary>
public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeleteTodoItemCommandHandler> _logger;

    public DeleteTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<DeleteTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Deletes a todo item by ID.
    /// </summary>
    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting todo item with ID: {TodoItemId}",
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

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Todo item deleted successfully with ID: {TodoItemId}",
            request.Id);
    }
}
