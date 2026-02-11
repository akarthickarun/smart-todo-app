using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Infrastructure.Persistence;

/// <summary>
/// Application database context for Entity Framework Core.
/// Provides data access to all domain entities.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet for todo items.
    /// </summary>
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
