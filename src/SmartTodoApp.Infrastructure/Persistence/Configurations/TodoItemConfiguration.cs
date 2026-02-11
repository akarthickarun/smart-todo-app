using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Domain.Enums;

namespace SmartTodoApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for TodoItem entity.
/// </summary>
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");

        // Primary key - GUIDs generated in application for immediate availability
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever(); // Application generates GUIDs

        // Title configuration
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        // Description configuration
        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Status configuration
        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(TodoStatus.Pending)
            .HasConversion<int>();

        // DueDate configuration - Map DateOnly to SQL Server date type
        builder.Property(x => x.DueDate)
            .HasColumnType("date");

        // CreatedAt configuration with automatic UTC timestamp
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt configuration with automatic UTC timestamp
        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes for query performance
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_TodoItems_Status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_TodoItems_CreatedAt");

        // Composite index for querying status with due date (e.g., overdue incomplete items)
        builder.HasIndex(x => new { x.Status, x.DueDate })
            .HasDatabaseName("IX_TodoItems_Status_DueDate");
    }
}
