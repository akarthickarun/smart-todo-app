using Microsoft.EntityFrameworkCore;
using SmartTodoApp.Application.Common.Interfaces;

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
    /// Gets or sets the DbSet for todos.
    /// </summary>
    /// TODO: Add TodoItem DbSet when TodoItem entity is created
    // public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // TODO: Apply entity configurations when created
        // modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
