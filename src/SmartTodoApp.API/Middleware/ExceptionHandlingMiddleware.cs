using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Application.Common.Exceptions;
using System.Net;
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
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/problem+json";

        ProblemDetails problemDetails;
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails = new ValidationProblemDetails(validationException.Errors)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                };
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "The specified resource was not found.",
                    Detail = notFoundException.Message,
                    Status = StatusCodes.Status404NotFound,
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
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

        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId.ToString();
        }

        response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await response.WriteAsync(json);
    }
}
