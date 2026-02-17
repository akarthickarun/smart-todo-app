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

            // Ensure components are initialized
            document.Components ??= new OpenApiComponents();

            // Add the scheme to the document components
            document.AddComponent("Bearer", bearerScheme);

            // Create a security requirement referencing the scheme
            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            };

            // Apply the requirement to all operations
            if (document.Paths != null)
            {
                foreach (var path in document.Paths.Values)
                {
                    if (path?.Operations != null)
                    {
                        foreach (var operation in path.Operations.Values)
                        {
                            operation.Security ??= new List<OpenApiSecurityRequirement>();
                            operation.Security.Add(securityRequirement);
                        }
                    }
                }
            }
        }
    }
}
