using FluentValidation;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

/// <summary>
/// Validator for GetTodoItemByIdQuery.
/// </summary>
public class GetTodoItemByIdQueryValidator : AbstractValidator<GetTodoItemByIdQuery>
{
    public GetTodoItemByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required and must not be empty");
    }
}
