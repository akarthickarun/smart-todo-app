using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Application.Common.Exceptions;
using System.Text.Json;

namespace SmartTodoApp.API.Middleware;

/// <summary>
/// Middleware that handles all exceptions and returns RFC 7807 Problem Details responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions and return standardized error responses.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception and writes a Problem Details response to the HTTP response.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // If the response has already started, we cannot modify headers or write to the body
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write error response - response has already started");
            return;
        }

        var response = context.Response;
        response.ContentType = "application/problem+json";

        ProblemDetails problemDetails;

        switch (exception)
        {
            case ValidationException validationException:
                _logger.LogWarning(exception, "Validation failed: {Message}", exception.Message);
                problemDetails = new ValidationProblemDetails(validationException.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                };
                break;

            case NotFoundException notFoundException:
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "The specified resource was not found.",
                    Detail = notFoundException.Message,
                    Status = StatusCodes.Status404NotFound,
                };
                break;

            case InvalidOperationException invalidOperationException:
                _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Invalid operation.",
                    Detail = invalidOperationException.Message,
                    Status = StatusCodes.Status400BadRequest,
                };
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
                var detail = environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.";

                problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "An error occurred while processing your request.",
                    Detail = detail,
                    Status = StatusCodes.Status500InternalServerError,
                };
                break;
        }

        // Add trace ID and correlation ID to Problem Details
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Try to get correlation ID from HttpContext.Items (set by CorrelationIdMiddleware)
        // Fall back to request headers if not found
        if (context.Items.TryGetValue("CorrelationId", out var correlationIdFromItems) && correlationIdFromItems is string correlationId)
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }
        else if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationIdFromHeader))
        {
            problemDetails.Extensions["correlationId"] = correlationIdFromHeader.ToString();
        }

        response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await response.WriteAsync(json);
    }
}
