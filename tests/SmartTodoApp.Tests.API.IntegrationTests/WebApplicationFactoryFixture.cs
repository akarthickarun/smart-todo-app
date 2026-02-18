using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Infrastructure.Persistence;
using SmartTodoApp.Infrastructure.Security;
using System.Security.Claims;

namespace SmartTodoApp.Tests.API.IntegrationTests;

/// <summary>
/// WebApplicationFactory fixture for integration tests.
/// Creates a test server with in-memory database to test API endpoints.
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to skip SQL Server registration in Program.cs
        builder.UseEnvironment("Testing");

        builder.ConfigureServices((context, services) =>
        {
            // Register in-memory database for testing
            var databaseName = _databaseName;
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            // Register IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Register TokenGenerator (required by Infrastructure)
            services.AddSingleton<ITokenGenerator>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return TokenGenerator.FromConfiguration(config);
            });

            // Remove existing authentication services registered in Program.cs (JWT Bearer)
            // This ensures a clean test authentication setup without conflicts
            var authenticationServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAuthenticationSchemeProvider));
            if (authenticationServiceDescriptor != null)
            {
                services.Remove(authenticationServiceDescriptor);
            }

            // Register test authentication scheme that always succeeds for authenticated requests
            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            services.AddAuthorization();
        });
    }

    /// <summary>
    /// Creates an authenticated HTTP client with the test auth header.
    /// By default, creates a new random user identity for each request.
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "test-token");
        return client;
    }

    /// <summary>
    /// Creates an authenticated HTTP client with a specific user ID.
    /// This maintains consistent identity across multiple requests, useful for testing user-specific operations.
    /// </summary>
    /// <param name="userId">The user ID to use for authentication. If null, a default test user ID is used.</param>
    public HttpClient CreateAuthenticatedClient(Guid? userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestAuthHeader, "test-token");
        
        if (userId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.TestUserIdHeader, userId.Value.ToString());
        }
        
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated HTTP client without the test auth header
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Resets the database to ensure test isolation.
    /// Should be called before each test to clear any data from previous tests.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Delete and recreate the database to ensure a clean state
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}

/// <summary>
/// Test authentication handler that always authenticates successfully.
/// Supports both random user IDs (default) and fixed user IDs (for user-specific testing).
/// </summary>
public class TestAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TestAuthHeader = "X-Test-Auth";
    public const string TestUserIdHeader = "X-Test-UserId";

    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the test auth header is present
        if (!Request.Headers.ContainsKey(TestAuthHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing authentication header"));
        }

        // Check if a specific user ID is provided, otherwise generate a new one
        string userId;
        if (Request.Headers.TryGetValue(TestUserIdHeader, out var userIdHeader) && 
            !string.IsNullOrWhiteSpace(userIdHeader))
        {
            userId = userIdHeader.ToString();
        }
        else
        {
            // Default behavior: create a new random user ID for each request
            userId = Guid.NewGuid().ToString();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId)
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
