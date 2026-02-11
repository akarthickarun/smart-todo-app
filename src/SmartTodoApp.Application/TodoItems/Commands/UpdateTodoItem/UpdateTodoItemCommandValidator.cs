using FluentValidation;

namespace SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

/// <summary>
/// Validator for UpdateTodoItemCommand.
/// </summary>
public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Title is required")
            .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage("Title cannot be empty or whitespace")
            .Must(title => title.Trim().Length >= 3)
                .WithMessage("Title must be at least 3 characters")
            .Must(title => title.Trim().Length <= 200)
                .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Due date must be today or in the future")
            .When(x => x.DueDate.HasValue);
    }
}
