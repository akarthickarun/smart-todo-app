using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SmartTodoApp.Application.Common.Behaviors;

namespace SmartTodoApp.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR with the application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register all validators from the application assembly
        services.AddValidatorsFromAssembly(assembly);

        // Register MediatR pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}
