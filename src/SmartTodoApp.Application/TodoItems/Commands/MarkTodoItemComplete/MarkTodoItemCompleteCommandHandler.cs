using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;

/// <summary>
/// Handler for MarkTodoItemCompleteCommand that marks a todo item as complete.
/// </summary>
public class MarkTodoItemCompleteCommandHandler : IRequestHandler<MarkTodoItemCompleteCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MarkTodoItemCompleteCommandHandler> _logger;

    public MarkTodoItemCompleteCommandHandler(
        IApplicationDbContext context,
        ILogger<MarkTodoItemCompleteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Marks a todo item as complete.
    /// </summary>
    public async Task Handle(MarkTodoItemCompleteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Marking todo item as complete with ID: {TodoItemId}",
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

        todoItem.MarkAsComplete();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Todo item marked as complete successfully with ID: {TodoItemId}",
            request.Id);
    }
}
