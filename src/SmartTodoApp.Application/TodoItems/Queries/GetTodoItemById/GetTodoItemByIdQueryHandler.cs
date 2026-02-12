using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

/// <summary>
/// Handler for GetTodoItemByIdQuery that retrieves a single todo item.
/// </summary>
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, TodoItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;

    public GetTodoItemByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTodoItemByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a todo item by ID.
    /// </summary>
    public async Task<TodoItemDto> Handle(GetTodoItemByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching todo item with ID: {TodoItemId}", request.Id);

        var todoItem = await _context.TodoItems
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (todoItem == null)
        {
            _logger.LogWarning("Todo item not found with ID: {TodoItemId}", request.Id);
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        return todoItem;
    }
}
