using Microsoft.EntityFrameworkCore;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Application.Common.Interfaces;

/// <summary>
/// Interface for the application's database context.
/// Provides data access to domain entities.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the DbSet for todo items.
    /// </summary>
    DbSet<TodoItem> TodoItems { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
