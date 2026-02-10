using SmartTodoApp.Domain.Enums;

namespace SmartTodoApp.Domain.Entities;

public class TodoItem
{
    private const int TitleMaxLength = 200;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TodoItem()
    {
    }

    public static TodoItem Create(string title, string? description, DateOnly? dueDate)
    {
        var trimmedTitle = ValidateTitle(title);

        var now = DateTime.UtcNow;

        return new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = trimmedTitle,
            Description = description,
            Status = TodoStatus.Pending,
            DueDate = dueDate,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MarkAsComplete()
    {
        if (Status == TodoStatus.Completed)
        {
            throw new InvalidOperationException("Todo item is already completed.");
        }

        Status = TodoStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, DateOnly? dueDate)
    {
        var trimmedTitle = ValidateTitle(title);

        Title = trimmedTitle;
        Description = description;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        var trimmedTitle = title.Trim();

        if (trimmedTitle.Length > TitleMaxLength)
        {
            throw new ArgumentException($"Title must not exceed {TitleMaxLength} characters.", nameof(title));
        }

        return trimmedTitle;
    }
}
