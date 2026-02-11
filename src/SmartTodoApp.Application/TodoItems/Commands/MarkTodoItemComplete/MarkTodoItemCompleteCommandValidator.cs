using FluentValidation;

namespace SmartTodoApp.Application.TodoItems.Commands.MarkTodoItemComplete;

/// <summary>
/// Validator for MarkTodoItemCompleteCommand.
/// </summary>
public class MarkTodoItemCompleteCommandValidator : AbstractValidator<MarkTodoItemCompleteCommand>
{
    public MarkTodoItemCompleteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");
    }
}
