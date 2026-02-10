using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SmartTodoApp.API.Middleware;

/// <summary>
/// Middleware that extracts or generates a correlation ID for tracking requests across the application.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to extract or generate a correlation ID and add it to the response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Store correlation ID in HttpContext.Items for downstream middleware
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        // Add correlation ID to Serilog context for structured logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug(
                "Request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await _next(context);

                _logger.LogDebug(
                    "Request completed: {Method} {Path} - Status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
            }
            catch (Exception)
            {
                _logger.LogError(
                    "Request failed: {Method} {Path} - Status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
                throw;
            }
        }
    }

    /// <summary>
    /// Extracts the correlation ID from the request header, or generates a new one if not present.
    /// </summary>
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId)
            && !string.IsNullOrWhiteSpace(correlationId.ToString()))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
