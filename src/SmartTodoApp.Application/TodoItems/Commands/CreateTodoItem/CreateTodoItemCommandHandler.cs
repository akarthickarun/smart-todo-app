using MediatR;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

/// <summary>
/// Handler for CreateTodoItemCommand that creates a new todo item.
/// </summary>
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;

    public CreateTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new todo item and returns its ID.
    /// </summary>
    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating todo item with title: {Title}",
            request.Title);

        var todoItem = TodoItem.Create(request.Title, request.Description, request.DueDate);

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Todo item created successfully with ID: {TodoItemId}",
            todoItem.Id);

        return todoItem.Id;
    }
}
