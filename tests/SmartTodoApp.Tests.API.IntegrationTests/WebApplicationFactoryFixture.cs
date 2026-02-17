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
            // Register in-memory database for testing - use the same database name for all requests
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

            // Replace authentication with test authentication that always succeeds
            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            services.AddAuthorization();
        });
    }
}

/// <summary>
/// Test authentication handler that always authenticates successfully
/// </summary>
public class TestAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("sub", Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
