using FluentValidation.Results;

namespace SmartTodoApp.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails for a command or query.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(List<ValidationFailure> failures)
        : base("Validation failed")
    {
        Errors = failures
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray());
    }

    /// <summary>
    /// Gets a dictionary of validation errors, grouped by property name.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }
}
