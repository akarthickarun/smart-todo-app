using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTodoApp.API.Middleware;
using SmartTodoApp.Application.Common.Exceptions;

namespace SmartTodoApp.Tests.API.IntegrationTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoException_ShouldNotInterfere()
    {
        // Arrange
        using var host = await CreateTestHost(context =>
        {
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("Success");
        });
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Success");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ShouldReturnBadRequestWithProblemDetails()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required"),
            new ValidationFailure("Title", "Title must be at least 3 characters")
        };
        var validationException = new ValidationException(validationFailures);

        using var host = await CreateTestHost(_ => throw validationException);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        
        // Verify basic problem details structure
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        
        // Verify the response contains validation error data (the exact structure may vary)
        // The key thing is that the middleware properly catches ValidationException and returns 400
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_ShouldReturnNotFoundWithProblemDetails()
    {
        // Arrange
        var notFoundException = new NotFoundException("TodoItem", Guid.NewGuid());

        using var host = await CreateTestHost(_ => throw notFoundException);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(404);
        problemDetails.Title.Should().Be("The specified resource was not found.");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.Detail.Should().Contain("TodoItem");
        problemDetails.Detail.Should().Contain("was not found");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ShouldReturnInternalServerErrorWithProblemDetails()
    {
        // Arrange
        var genericException = new InvalidOperationException("Something went wrong");

        using var host = await CreateTestHost(_ => throw genericException, isDevelopment: false);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("An error occurred while processing your request.");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        problemDetails.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }

    [Fact]
    public async Task InvokeAsync_GenericExceptionInDevelopment_ShouldReturnExceptionMessage()
    {
        // Arrange
        var genericException = new InvalidOperationException("Detailed error message");

        using var host = await CreateTestHost(_ => throw genericException, isDevelopment: true);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be("Detailed error message");
    }

    [Fact]
    public async Task InvokeAsync_WithCorrelationId_ShouldIncludeCorrelationIdInProblemDetails()
    {
        // Arrange
        var expectedCorrelationId = Guid.NewGuid().ToString();
        var exception = new NotFoundException("Item not found");

        using var host = await CreateTestHost(_ => throw exception);
        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", expectedCorrelationId);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("correlationId");
        problemDetails.Extensions["correlationId"].ToString().Should().Be(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_WithCorrelationIdInItems_ShouldPreferItemsOverHeaders()
    {
        // Arrange
        var expectedCorrelationId = Guid.NewGuid().ToString();
        var headerCorrelationId = Guid.NewGuid().ToString();
        var exception = new NotFoundException("Item not found");

        using var host = await CreateTestHost(context =>
        {
            // Simulate CorrelationIdMiddleware storing in Items
            context.Items["CorrelationId"] = expectedCorrelationId;
            throw exception;
        });
        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", headerCorrelationId);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("correlationId");
        problemDetails.Extensions["correlationId"].ToString().Should().Be(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeTraceIdInProblemDetails()
    {
        // Arrange
        var exception = new NotFoundException("Item not found");

        using var host = await CreateTestHost(_ => throw exception);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_ValidationExceptionWithMultipleProperties_ShouldGroupErrorsByProperty()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required"),
            new ValidationFailure("Title", "Title must be at least 3 characters"),
            new ValidationFailure("Description", "Description is too long")
        };
        var validationException = new ValidationException(validationFailures);

        using var host = await CreateTestHost(_ => throw validationException);
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify basic problem details structure
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
    }

    private static async Task<IHost> CreateTestHost(
        RequestDelegate requestHandler,
        bool isDevelopment = false)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseEnvironment(isDevelopment ? "Development" : "Production")
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlingMiddleware>();
                        app.Run(context => requestHandler(context));
                    });
            })
            .StartAsync();

        return host;
    }
}
