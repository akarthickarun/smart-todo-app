using FluentValidation;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

/// <summary>
/// Validator for GetTodoItemsQuery.
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

    private static bool BeValidEnumValue(DomainTodoStatus? status)
    {
        return Enum.IsDefined(typeof(DomainTodoStatus), status!.Value);
    }
}
