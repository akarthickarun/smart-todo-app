using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Shared.Contracts.TodoItems;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Handler for GetTodoItemsQuery that retrieves a filtered list of todo items.
/// Maps contract TodoStatus to domain TodoStatus internally to keep controllers thin.
/// </summary>
public class GetTodoItemsQueryHandler : IRequestHandler<GetTodoItemsQuery, List<TodoItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemsQueryHandler> _logger;

    public GetTodoItemsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTodoItemsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves todo items, optionally filtered by status.
    /// Converts contract TodoStatus to domain TodoStatus for filtering.
    /// </summary>
    public async Task<List<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching todo items with filter Status: {Status}", request.Status);

        var query = _context.TodoItems.AsNoTracking();

        if (request.Status.HasValue)
        {
            // Map contract TodoStatus to domain TodoStatus
            var domainStatus = (DomainTodoStatus)request.Status.Value;
            query = query.Where(x => x.Status == domainStatus);
        }

        var todoItems = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} todo items", todoItems.Count);

        return todoItems;
    }
}
