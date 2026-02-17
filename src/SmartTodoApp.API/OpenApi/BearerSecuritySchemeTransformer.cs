using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SmartTodoApp.API.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();

        // Only proceed if Bearer authentication is configured
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Define the Bearer security scheme
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            };

            // Add it to components
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = bearerScheme;

            // Reference the scheme in security requirements
            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
            };

            // Set security requirement at document level so it's global
            document.Security ??= [];
            document.Security.Add(requirement);
        }
    }
}
