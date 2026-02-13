using FluentValidation;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Validator for GetTodoItemsQuery.
/// Validates the contract TodoStatus value.
/// </summary>
public class GetTodoItemsQueryValidator : AbstractValidator<GetTodoItemsQuery>
{
    public GetTodoItemsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidEnumValue)
            .WithMessage("Status must be a valid TodoStatus value (0 = Pending, 1 = Completed)")
            .When(x => x.Status.HasValue);
    }

    private static bool BeValidEnumValue(TodoStatus? status)
    {
        return Enum.IsDefined(typeof(TodoStatus), status!.Value);
    }
}
