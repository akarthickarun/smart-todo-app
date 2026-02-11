# GitHub Copilot Instructions - Smart Todo App

## Technology Stack Summary

### Backend
- **Runtime**: ASP.NET Core 10
- **ORM**: Entity Framework Core 10
- **Database**: SQL Server
- **CQRS**: MediatR 12.x
- **Validation**: FluentValidation 11.x
- **Resilience**: Polly 8.x
- **Logging**: Serilog 3.x
- **Testing**: xUnit 2.x, Moq 4.x, FluentAssertions 6.x

### Frontend
- **Framework**: React 19
- **Language**: TypeScript 5.7
- **Build Tool**: Vite 6
- **Styling**: TailwindCSS 4
- **UI Components**: shadcn/ui
- **Router**: React Router 7
- **HTTP Client**: Axios 1.x
- **State Management**: TanStack Query v5, Zustand 4.x
- **Validation**: Zod 3.x
- **Forms**: React Hook Form 7.x
- **Testing**: Vitest 1.x, React Testing Library 14.x

---

## ⚠️ CRITICAL WORKFLOW RULES - READ FIRST

**These rules MUST be followed for every ticket implementation:**

### Branching Workflow
1. **ALWAYS create a dedicated feature branch BEFORE starting any ticket**
   - Branch naming convention: `feature/ticket-{number}-{brief-kebab-case-description}`
   - Example: `feature/ticket-7-database-migration`
   - Create from main/develop branch depending on project setup

2. **Branch creation steps:**
   ```bash
   git checkout main  # or develop
   git pull origin "$(git rev-parse --abbrev-ref HEAD)"
   git checkout -b feature/ticket-{number}-{description}
   git push -u origin feature/ticket-{number}-{description}
   ```

3. **Commit conventions:**
   - Use conventional commit format: `feat:`, `fix:`, `chore:`, `docs:`, `test:`
   - Include ticket number in commit message
   - Example: `feat: ticket #7 - Add initial database migration`
   - Reference GitHub issue in commit body: `Closes #10`

4. **Commit and push workflow:**
   ```bash
   git add .
   git commit -m "feat: ticket #{number} - {brief-description}" -m "{detailed changes}" -m "Closes #{issue-number}"
   git push
   ```
   - Use multiple `-m` flags for multi-line commit messages (one per paragraph)
   - Immediately push branch to GitHub after creation
   - Create PR for review before merging to main
   - Update TICKETS.md status to ✅ Completed in the same commit

5. **NEVER work directly on main/develop branch**
6. **NEVER push directly to main/develop**
7. **NEVER merge without creating a PR first**

### Ticket Execution Checklist
- [ ] Check TICKETS.md for next ticket to implement
- [ ] Verify all dependencies are completed
- [ ] Create feature branch following naming convention
- [ ] Push branch to GitHub
- [ ] Implement changes following acceptance criteria
- [ ] Update TICKETS.md status to ✅ Completed
- [ ] Commit with conventional commit format
- [ ] Push changes
- [ ] Ready for PR review

---

## 1. Golden Rules

### 1.1 CQRS Separation
- **Commands** change state, return no data (void or ID only)
- **Queries** read data, never modify state
- One handler per command/query in `Application` layer
- Commands/Queries are immutable records

### 1.2 Clean Architecture Layers
- **Domain**: Entities, Value Objects, Domain Events (No dependencies)
- **Application**: Commands, Queries, Handlers, Validators, Interfaces
- **Infrastructure**: DbContext, Repositories, External Services
- **API**: Controllers, Middleware, Filters
- **Shared.Contracts**: DTOs shared between frontend and backend

### 1.3 Validation Everywhere
- **Backend**: FluentValidation for all Commands/Queries
- **Frontend**: Zod schemas for all forms and API responses
- Fail fast: validate at API boundary and application layer

### 1.4 Error Handling
- Always return RFC 7807 Problem Details
- Use custom exceptions with clear messages
- Log errors with CorrelationId
- Never expose stack traces to clients in production

### 1.5 Testing Requirements
- Unit tests for all Handlers and Validators
- Integration tests for API endpoints
- Frontend tests for all components with user interactions
- Minimum 80% code coverage

### 1.6 Security First
- JWT Bearer authentication for all protected endpoints
- Role-based authorization with policies
- HTTPS only in production
- Rate limiting on public endpoints

### 1.7 Logging with CorrelationId
- Every request must have a CorrelationId
- Log at entry and exit of handlers
- Use structured logging (Serilog)
- Include UserId in log context when authenticated

### 1.8 Async All the Way
- All I/O operations must be async
- Use `ConfigureAwait(false)` in library code
- Cancel operations with CancellationToken

### 1.9 Dependency Injection
- Register services in appropriate layer (Application, Infrastructure)
- Use interface abstractions
- Prefer scoped lifetime for DbContext and services

### 1.10 API Contracts
- DTOs in `Shared.Contracts` project
- Use records for immutability
- Include data annotations for OpenAPI documentation

### 1.11 Database Access
- Use EF Core for all data access
- Apply migrations in order
- Use AsNoTracking() for read-only queries
- Explicit transaction boundaries for multi-step operations

### 1.12 Frontend State Management
- TanStack Query for server state (API data)
- Zustand for client state (UI, preferences)
- Keep components stateless when possible

### 1.13 Branching for Tickets
- See **CRITICAL WORKFLOW RULES** at the top of this document
- ALWAYS create a dedicated branch before starting any ticket
- Follow the branching workflow checklist for every implementation

---

## 2. Layer Responsibilities

### 2.1 Domain Layer
**Owns:**
- Entity classes with business logic
- Value Objects
- Domain Events
- Domain Exceptions
- Enumerations

**Forbidden:**
- Database concerns (EF configuration)
- Infrastructure dependencies
- HTTP/API concerns
- External service calls

```csharp
// Domain/Entities/TodoItem.cs
namespace SmartTodoApp.Domain.Entities;

public class TodoItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsComplete { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private TodoItem() { } // EF Core

    public static TodoItem Create(string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        return new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsComplete()
    {
        if (IsComplete)
            throw new InvalidOperationException("Todo item is already complete");

        IsComplete = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        Description = description;
    }
}
```

### 2.2 Application Layer
**Owns:**
- Commands and Command Handlers
- Queries and Query Handlers
- FluentValidation Validators
- Application-level interfaces (IApplicationDbContext, etc.)
- AutoMapper Profiles
- Application Exceptions

**Forbidden:**
- Direct database implementation
- HTTP concerns
- UI logic

```csharp
// Application/TodoItems/Commands/CreateTodoItem/CreateTodoItemCommand.cs
namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoItemCommand(string Title, string? Description) : IRequest<Guid>;
```

### 2.3 Infrastructure Layer
**Owns:**
- DbContext implementation
- Entity configurations (Fluent API)
- Repository implementations
- External service integrations (email, storage, etc.)
- Authentication/Authorization implementation

**Forbidden:**
- Business logic
- Direct HTTP response handling

```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
namespace SmartTodoApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### 2.4 API Layer
**Owns:**
- Controllers (thin, delegate to MediatR)
- Middleware
- Filters
- Program.cs configuration
- API-specific error handling

**Forbidden:**
- Business logic
- Database access
- Complex validation

```csharp
// API/Controllers/TodoItemsController.cs
namespace SmartTodoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateTodoItemCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTodoItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
```

### 2.5 Shared.Contracts
**Owns:**
- DTOs (Data Transfer Objects)
- API request/response models
- Shared enums
- Constants

**Forbidden:**
- Business logic
- Validation logic (define in Application layer)
- Database concerns

```csharp
// Shared.Contracts/TodoItems/TodoItemDto.cs
namespace SmartTodoApp.Shared.Contracts.TodoItems;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsComplete,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
```

---

## 3. Naming & Structure Conventions

### 3.1 Backend Folder Structure

```
src/
├── SmartTodoApp.Domain/
│   ├── Entities/
│   │   └── TodoItem.cs
│   ├── Exceptions/
│   │   └── DomainException.cs
│   └── Common/
│       └── BaseEntity.cs
├── SmartTodoApp.Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   └── IApplicationDbContext.cs
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs
│   │   ├── Exceptions/
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   └── Behaviors/
│   │       ├── ValidationBehavior.cs
│   │       └── LoggingBehavior.cs
│   └── TodoItems/
│       ├── Commands/
│       │   ├── CreateTodoItem/
│       │   │   ├── CreateTodoItemCommand.cs
│       │   │   ├── CreateTodoItemCommandHandler.cs
│       │   │   └── CreateTodoItemCommandValidator.cs
│       │   └── UpdateTodoItem/
│       │       ├── UpdateTodoItemCommand.cs
│       │       ├── UpdateTodoItemCommandHandler.cs
│       │       └── UpdateTodoItemCommandValidator.cs
│       └── Queries/
│           ├── GetTodoItemById/
│           │   ├── GetTodoItemByIdQuery.cs
│           │   └── GetTodoItemByIdQueryHandler.cs
│           └── GetTodoItems/
│               ├── GetTodoItemsQuery.cs
│               └── GetTodoItemsQueryHandler.cs
├── SmartTodoApp.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/
│   │   │   └── TodoItemConfiguration.cs
│   │   └── Migrations/
│   ├── Services/
│   │   └── DateTimeService.cs
│   └── DependencyInjection.cs
├── SmartTodoApp.API/
│   ├── Controllers/
│   │   └── TodoItemsController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── CorrelationIdMiddleware.cs
│   ├── Filters/
│   │   └── ValidateModelStateFilter.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
└── SmartTodoApp.Shared.Contracts/
    └── TodoItems/
        ├── TodoItemDto.cs
        ├── CreateTodoItemRequest.cs
        └── UpdateTodoItemRequest.cs
```

### 3.2 Frontend Folder Structure

```
src/
├── api/
│   ├── axios-instance.ts         # Configured Axios client
│   ├── todo-api.ts               # Todo API calls
│   └── auth-api.ts               # Auth API calls
├── components/
│   ├── ui/                       # shadcn/ui components
│   │   ├── button.tsx
│   │   ├── card.tsx
│   │   ├── dialog.tsx
│   │   └── form.tsx
│   ├── todos/
│   │   ├── TodoList.tsx
│   │   ├── TodoItem.tsx
│   │   ├── CreateTodoDialog.tsx
│   │   └── TodoFilters.tsx
│   └── layout/
│       ├── Header.tsx
│       ├── Footer.tsx
│       └── MainLayout.tsx
├── features/
│   ├── todos/
│   │   ├── hooks/
│   │   │   ├── useTodos.ts
│   │   │   ├── useCreateTodo.ts
│   │   │   └── useUpdateTodo.ts
│   │   ├── schemas/
│   │   │   └── todoSchemas.ts   # Zod schemas
│   │   └── types/
│   │       └── todo.types.ts
│   └── auth/
│       ├── hooks/
│       │   └── useAuth.ts
│       └── stores/
│           └── authStore.ts     # Zustand store
├── lib/
│   ├── query-client.ts          # TanStack Query setup
│   └── utils.ts                 # Utility functions
├── pages/
│   ├── TodosPage.tsx
│   ├── LoginPage.tsx
│   └── NotFoundPage.tsx
├── router/
│   └── index.tsx                # React Router setup
├── styles/
│   └── globals.css              # Tailwind imports
├── App.tsx
└── main.tsx
```

### 3.3 Naming Patterns

**Backend:**
- **Commands**: `{Verb}{Entity}Command` (e.g., `CreateTodoItemCommand`, `UpdateTodoItemCommand`)
- **Queries**: `Get{Entity}{Criteria}Query` (e.g., `GetTodoItemByIdQuery`, `GetTodoItemsQuery`)
- **Handlers**: `{CommandOrQuery}Handler` (e.g., `CreateTodoItemCommandHandler`)
- **Validators**: `{CommandOrQuery}Validator` (e.g., `CreateTodoItemCommandValidator`)
- **DTOs**: `{Entity}Dto` (e.g., `TodoItemDto`)
- **Exceptions**: `{Purpose}Exception` (e.g., `NotFoundException`, `ValidationException`)

**Frontend:**
- **Components**: PascalCase (e.g., `TodoList`, `CreateTodoDialog`)
- **Hooks**: `use{Purpose}` (e.g., `useTodos`, `useCreateTodo`)
- **Stores**: `{feature}Store` (e.g., `authStore`, `uiStore`)
- **API files**: kebab-case (e.g., `todo-api.ts`, `auth-api.ts`)
- **Types**: `{Entity}.types.ts` (e.g., `todo.types.ts`)
- **Schemas**: `{entity}Schemas.ts` (e.g., `todoSchemas.ts`)

---

## 4. Standard CQRS Templates

### 4.1 Command Example: Create Operation

```csharp
// Application/TodoItems/Commands/CreateTodoItem/CreateTodoItemCommand.cs
using MediatR;

namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoItemCommand(string Title, string? Description) : IRequest<Guid>;
```

```csharp
// Application/TodoItems/Commands/CreateTodoItem/CreateTodoItemCommandValidator.cs
using FluentValidation;

namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}
```

```csharp
// Application/TodoItems/Commands/CreateTodoItem/CreateTodoItemCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;

    public CreateTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating todo item with title: {Title}", request.Title);

        var todoItem = TodoItem.Create(request.Title, request.Description);

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Todo item created with ID: {TodoItemId}", todoItem.Id);

        return todoItem.Id;
    }
}
```

### 4.2 Query Example: Get by ID

```csharp
// Application/TodoItems/Queries/GetTodoItemById/GetTodoItemByIdQuery.cs
using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

public record GetTodoItemByIdQuery(Guid Id) : IRequest<TodoItemDto>;
```

```csharp
// Application/TodoItems/Queries/GetTodoItemById/GetTodoItemByIdQueryHandler.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Exceptions;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItemById;

public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, TodoItemDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;

    public GetTodoItemByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTodoItemByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TodoItemDto> Handle(GetTodoItemByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching todo item with ID: {TodoItemId}", request.Id);

        var todoItem = await _context.TodoItems
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (todoItem == null)
        {
            _logger.LogWarning("Todo item not found with ID: {TodoItemId}", request.Id);
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        return todoItem;
    }
}
```

### 4.3 List Query Example

```csharp
// Application/TodoItems/Queries/GetTodoItems/GetTodoItemsQuery.cs
using MediatR;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

public record GetTodoItemsQuery(bool? IsComplete = null) : IRequest<List<TodoItemDto>>;
```

```csharp
// Application/TodoItems/Queries/GetTodoItems/GetTodoItemsQueryHandler.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.TodoItems.Queries.GetTodoItems;

public class GetTodoItemsQueryHandler : IRequestHandler<GetTodoItemsQuery, List<TodoItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemsQueryHandler> _logger;

    public GetTodoItemsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetTodoItemsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<TodoItemDto>> Handle(GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching todo items with filter IsComplete: {IsComplete}", request.IsComplete);

        var query = _context.TodoItems.AsNoTracking();

        if (request.IsComplete.HasValue)
        {
            query = query.Where(x => x.IsComplete == request.IsComplete.Value);
        }

        var todoItems = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} todo items", todoItems.Count);

        return todoItems;
    }
}
```

---

## 5. AutoMapper Configuration

### 5.1 Mapping Profile

```csharp
// Application/Common/Mappings/MappingProfile.cs
using AutoMapper;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TodoItem, TodoItemDto>();
        
        // Additional mappings
        CreateMap<CreateTodoItemCommand, TodoItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsComplete, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());
    }
}
```

### 5.2 ProjectTo Usage

```csharp
// Efficient projection in queries
var todoItems = await _context.TodoItems
    .AsNoTracking()
    .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
    .ToListAsync(cancellationToken);
```

### 5.3 Registration in Program.cs

```csharp
// API/Program.cs
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// AutoMapper registration
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), 
    typeof(SmartTodoApp.Application.Common.Mappings.MappingProfile).Assembly);
```

---

## 6. Resilience with Polly v8

### 6.1 HTTP Client with Retry and Circuit Breaker

```csharp
// Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace SmartTodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // HTTP Client with resilience
        services.AddHttpClient("ExternalApi", client =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler(options =>
        {
            // Retry configuration
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(1);
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;

            // Circuit breaker configuration
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 10;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

            // Timeout configuration
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
```

### 6.2 Custom Retry Policy

```csharp
// Infrastructure/Resilience/ResiliencePolicies.cs
using Polly;
using Polly.Retry;

namespace SmartTodoApp.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static ResiliencePipeline<HttpResponseMessage> GetHttpRetryPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response => !response.IsSuccessStatusCode)
            })
            .Build();
    }
}
```

### 6.3 Timeout Strategy

```csharp
// Add timeout to specific operations
services.AddResiliencePipeline("timeout-pipeline", builder =>
{
    builder.AddTimeout(TimeSpan.FromSeconds(10));
});
```

---

## 7. Security Essentials

### 7.1 JWT Bearer Authentication

```csharp
// API/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT Authentication configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed");
                return Task.CompletedTask;
            }
        };
    });
```

**appsettings.json:**
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "SmartTodoApp",
    "Audience": "SmartTodoAppClient",
    "ExpiryMinutes": 60
  }
}
```

### 7.2 Authorization Policies

```csharp
// API/Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CanManageTodos", policy =>
        policy.RequireClaim("permission", "todos:write"));

    options.AddPolicy("RequireEmailVerified", policy =>
        policy.RequireClaim("email_verified", "true"));
});

// Usage in controller
[Authorize(Policy = "CanManageTodos")]
[HttpPost]
public async Task<ActionResult<Guid>> Create(CreateTodoItemCommand command)
{
    var id = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id }, id);
}
```

### 7.3 CORS Configuration

```csharp
// API/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
```

**appsettings.json:**
```json
{
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://yourdomain.com"
  ]
}
```

### 7.4 Rate Limiting

```csharp
// API/Program.cs
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

var app = builder.Build();

app.UseRateLimiter();
```

---

## 8. Input Validation

### 8.1 Backend: FluentValidation

```csharp
// Application/TodoItems/Commands/UpdateTodoItem/UpdateTodoItemCommandValidator.cs
using FluentValidation;

namespace SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;

public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}
```

### 8.2 Validation Behavior (MediatR Pipeline)

```csharp
// Application/Common/Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;

namespace SmartTodoApp.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
```

### 8.3 Register Validation in Application Layer

```csharp
// Application/DependencyInjection.cs
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SmartTodoApp.Application.Common.Behaviors;

namespace SmartTodoApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
```

### 8.4 Frontend: Zod Schemas

```typescript
// src/features/todos/schemas/todoSchemas.ts
import { z } from 'zod';

export const createTodoSchema = z.object({
  title: z
    .string()
    .min(3, 'Title must be at least 3 characters')
    .max(200, 'Title must not exceed 200 characters'),
  description: z
    .string()
    .max(1000, 'Description must not exceed 1000 characters')
    .optional(),
});

export const updateTodoSchema = z.object({
  id: z.string().uuid('Invalid ID format'),
  title: z
    .string()
    .min(3, 'Title must be at least 3 characters')
    .max(200, 'Title must not exceed 200 characters'),
  description: z
    .string()
    .max(1000, 'Description must not exceed 1000 characters')
    .optional(),
});

export const todoItemSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  description: z.string().nullable(),
  isComplete: z.boolean(),
  createdAt: z.string().datetime(),
  completedAt: z.string().datetime().nullable(),
});

export type CreateTodoInput = z.infer<typeof createTodoSchema>;
export type UpdateTodoInput = z.infer<typeof updateTodoSchema>;
export type TodoItem = z.infer<typeof todoItemSchema>;
```

### 8.5 React Hook Form Integration

```typescript
// src/components/todos/CreateTodoDialog.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createTodoSchema, CreateTodoInput } from '@/features/todos/schemas/todoSchemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { useCreateTodo } from '@/features/todos/hooks/useCreateTodo';

export function CreateTodoDialog() {
  const { mutate: createTodo, isPending } = useCreateTodo();

  const form = useForm<CreateTodoInput>({
    resolver: zodResolver(createTodoSchema),
    defaultValues: {
      title: '',
      description: '',
    },
  });

  const onSubmit = (data: CreateTodoInput) => {
    createTodo(data, {
      onSuccess: () => {
        form.reset();
      },
    });
  };

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button>Create Todo</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create New Todo</DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="title"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Title</FormLabel>
                  <FormControl>
                    <Input placeholder="Enter todo title" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <Textarea placeholder="Enter description (optional)" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Creating...' : 'Create'}
            </Button>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
```

---

## 9. Testing Strategy

### 9.1 Backend: Unit Test (Handler)

```csharp
// Tests/Application.UnitTests/TodoItems/Commands/CreateTodoItemCommandHandlerTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Domain.Entities;
using Xunit;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands;

public class CreateTodoItemCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<CreateTodoItemCommandHandler>> _loggerMock;
    private readonly CreateTodoItemCommandHandler _handler;

    public CreateTodoItemCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<CreateTodoItemCommandHandler>>();
        _handler = new CreateTodoItemCommandHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTodoItem()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Test Title", "Test Description");
        var todoItemsDbSetMock = new Mock<DbSet<TodoItem>>();
        _contextMock.Setup(x => x.TodoItems).Returns(todoItemsDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _contextMock.Verify(x => x.TodoItems.Add(It.IsAny<TodoItem>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldLogInformation()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Test Title", null);
        _contextMock.Setup(x => x.TodoItems).Returns(Mock.Of<DbSet<TodoItem>>());
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating todo item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

### 9.2 Backend: Validator Unit Test

```csharp
// Tests/Application.UnitTests/TodoItems/Commands/CreateTodoItemCommandValidatorTests.cs
using FluentAssertions;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using Xunit;

namespace SmartTodoApp.Tests.Application.UnitTests.TodoItems.Commands;

public class CreateTodoItemCommandValidatorTests
{
    private readonly CreateTodoItemCommandValidator _validator;

    public CreateTodoItemCommandValidatorTests()
    {
        _validator = new CreateTodoItemCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateTodoItemCommand("Valid Title", "Valid Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyTitle_ShouldHaveValidationError(string title)
    {
        // Arrange
        var command = new CreateTodoItemCommand(title, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Title));
    }

    [Fact]
    public void Validate_TitleExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTodoItemCommand(new string('a', 201), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => 
            x.PropertyName == nameof(command.Title) && 
            x.ErrorMessage.Contains("200"));
    }
}
```

### 9.3 Backend: Integration Test

```csharp
// Tests/Application.IntegrationTests/TodoItems/Commands/CreateTodoItemCommandTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using Xunit;

namespace SmartTodoApp.Tests.Application.IntegrationTests.TodoItems.Commands;

public class CreateTodoItemCommandTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CreateTodoItemCommandTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTodoItem_ValidCommand_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        var command = new CreateTodoItemCommand("Integration Test Todo", "Test Description");

        // Act
        var id = await mediator.Send(command);

        // Assert
        var todoItem = await context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        todoItem.Should().NotBeNull();
        todoItem!.Title.Should().Be("Integration Test Todo");
        todoItem.Description.Should().Be("Test Description");
        todoItem.IsComplete.Should().BeFalse();
    }
}
```

### 9.4 Frontend: Component Test (Vitest + React Testing Library)

```typescript
// src/components/todos/__tests__/TodoItem.test.tsx
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { TodoItem } from '../TodoItem';

describe('TodoItem', () => {
  const mockTodo = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    title: 'Test Todo',
    description: 'Test Description',
    isComplete: false,
    createdAt: '2026-02-09T10:00:00Z',
    completedAt: null,
  };

  it('renders todo item with title and description', () => {
    render(<TodoItem todo={mockTodo} onToggle={vi.fn()} onDelete={vi.fn()} />);

    expect(screen.getByText('Test Todo')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
  });

  it('calls onToggle when checkbox is clicked', () => {
    const onToggleMock = vi.fn();
    render(<TodoItem todo={mockTodo} onToggle={onToggleMock} onDelete={vi.fn()} />);

    const checkbox = screen.getByRole('checkbox');
    fireEvent.click(checkbox);

    expect(onToggleMock).toHaveBeenCalledWith(mockTodo.id);
  });

  it('calls onDelete when delete button is clicked', () => {
    const onDeleteMock = vi.fn();
    render(<TodoItem todo={mockTodo} onToggle={vi.fn()} onDelete={onDeleteMock} />);

    const deleteButton = screen.getByRole('button', { name: /delete/i });
    fireEvent.click(deleteButton);

    expect(onDeleteMock).toHaveBeenCalledWith(mockTodo.id);
  });

  it('displays completed state correctly', () => {
    const completedTodo = { ...mockTodo, isComplete: true, completedAt: '2026-02-09T12:00:00Z' };
    render(<TodoItem todo={completedTodo} onToggle={vi.fn()} onDelete={vi.fn()} />);

    const checkbox = screen.getByRole('checkbox');
    expect(checkbox).toBeChecked();
  });
});
```

### 9.5 Frontend: Custom Hook Test

```typescript
// src/features/todos/hooks/__tests__/useTodos.test.ts
import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useTodos } from '../useTodos';
import { ReactNode } from 'react';

describe('useTodos', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    });
  });

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );

  it('should return loading state initially', () => {
    const { result } = renderHook(() => useTodos(), { wrapper });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.data).toBeUndefined();
  });

  it('should fetch todos successfully', async () => {
    const { result } = renderHook(() => useTodos(), { wrapper });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
    expect(Array.isArray(result.current.data)).toBe(true);
  });
});
```

---

## 10. Database Patterns

### 10.1 DbContext Configuration

```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Domain.Entities;
using System.Reflection;

namespace SmartTodoApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit fields, domain events, etc. here if needed
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### 10.2 Entity Configuration (Fluent API)

```csharp
// Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTodoApp.Domain.Entities;

namespace SmartTodoApp.Infrastructure.Persistence.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsComplete)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.HasIndex(x => x.IsComplete);
        builder.HasIndex(x => x.CreatedAt);
    }
}
```

### 10.3 Migration Commands

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API

# Update database
dotnet ef database update --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API

# Generate SQL script
dotnet ef migrations script --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API --output migration.sql
```

### 10.4 Query Optimization

```csharp
// Good: Use AsNoTracking for read-only queries
var todos = await _context.TodoItems
    .AsNoTracking()
    .Where(x => x.IsComplete == false)
    .OrderByDescending(x => x.CreatedAt)
    .Take(10)
    .ToListAsync(cancellationToken);

// Good: Use ProjectTo for DTOs (AutoMapper)
var todoDtos = await _context.TodoItems
    .AsNoTracking()
    .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
    .ToListAsync(cancellationToken);

// Good: Explicit loading with Include
var todoWithRelatedData = await _context.TodoItems
    .Include(x => x.Tags)
    .Include(x => x.Attachments)
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

// Avoid: N+1 queries
// Bad - this will execute a query for each todo item
foreach (var todo in todos)
{
    var tags = await _context.Tags.Where(t => t.TodoItemId == todo.Id).ToListAsync();
}

// Good - use Include to load related data upfront
var todosWithTags = await _context.TodoItems
    .Include(x => x.Tags)
    .ToListAsync(cancellationToken);
```

### 10.5 DbContext Registration

```csharp
// Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTodoApp.Application.Common.Interfaces;
using SmartTodoApp.Infrastructure.Persistence;

namespace SmartTodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
```

---

## 11. Frontend State Management

### 11.1 Axios Instance with Interceptors

```typescript
// src/api/axios-instance.ts
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { authStore } from '@/features/auth/stores/authStore';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7000/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Add auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = authStore.getState().token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add CorrelationId
    config.headers['X-Correlation-ID'] = crypto.randomUUID();

    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Handle errors globally
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear auth and redirect to login
      authStore.getState().logout();
      window.location.href = '/login';
    }

    if (error.response?.status === 403) {
      // Forbidden
      console.error('Access denied');
    }

    return Promise.reject(error);
  }
);

export default apiClient;
```

### 11.2 TanStack Query Setup

```typescript
// src/lib/query-client.ts
import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    },
    mutations: {
      retry: 0,
    },
  },
});
```

```typescript
// src/main.tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { queryClient } from './lib/query-client';
import App from './App';
import './styles/globals.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </React.StrictMode>
);
```

### 11.3 API Layer

```typescript
// src/api/todo-api.ts
import apiClient from './axios-instance';
import { TodoItem, CreateTodoInput, UpdateTodoInput } from '@/features/todos/schemas/todoSchemas';

export const todoApi = {
  getAll: async (isComplete?: boolean): Promise<TodoItem[]> => {
    const params = isComplete !== undefined ? { isComplete } : {};
    const response = await apiClient.get<TodoItem[]>('/todoitems', { params });
    return response.data;
  },

  getById: async (id: string): Promise<TodoItem> => {
    const response = await apiClient.get<TodoItem>(`/todoitems/${id}`);
    return response.data;
  },

  create: async (data: CreateTodoInput): Promise<string> => {
    const response = await apiClient.post<string>('/todoitems', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTodoInput): Promise<void> => {
    await apiClient.put(`/todoitems/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/todoitems/${id}`);
  },

  toggleComplete: async (id: string): Promise<void> => {
    await apiClient.patch(`/todoitems/${id}/toggle`);
  },
};
```

### 11.4 TanStack Query Hooks

```typescript
// src/features/todos/hooks/useTodos.ts
import { useQuery } from '@tanstack/react-query';
import { todoApi } from '@/api/todo-api';

export const useTodos = (isComplete?: boolean) => {
  return useQuery({
    queryKey: ['todos', isComplete],
    queryFn: () => todoApi.getAll(isComplete),
  });
};
```

```typescript
// src/features/todos/hooks/useCreateTodo.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { todoApi } from '@/api/todo-api';
import { CreateTodoInput } from '@/features/todos/schemas/todoSchemas';
import { toast } from 'sonner';

export const useCreateTodo = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTodoInput) => todoApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos'] });
      toast.success('Todo created successfully');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to create todo');
    },
  });
};
```

```typescript
// src/features/todos/hooks/useUpdateTodo.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { todoApi } from '@/api/todo-api';
import { UpdateTodoInput } from '@/features/todos/schemas/todoSchemas';
import { toast } from 'sonner';

export const useUpdateTodo = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTodoInput }) => 
      todoApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos'] });
      toast.success('Todo updated successfully');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to update todo');
    },
  });
};
```

### 11.5 Zustand Store (Client State)

```typescript
// src/features/auth/stores/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthState {
  token: string | null;
  user: { id: string; email: string; name: string } | null;
  isAuthenticated: boolean;
  login: (token: string, user: AuthState['user']) => void;
  logout: () => void;
}

export const authStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,
      login: (token, user) => set({ token, user, isAuthenticated: true }),
      logout: () => set({ token: null, user: null, isAuthenticated: false }),
    }),
    {
      name: 'auth-storage',
    }
  )
);
```

---

## 12. React Router v7

### 12.1 Router Setup

```typescript
// src/router/index.tsx
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import MainLayout from '@/components/layout/MainLayout';
import TodosPage from '@/pages/TodosPage';
import LoginPage from '@/pages/LoginPage';
import NotFoundPage from '@/pages/NotFoundPage';
import { ProtectedRoute } from './ProtectedRoute';

const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        index: true,
        element: (
          <ProtectedRoute>
            <TodosPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'login',
        element: <LoginPage />,
      },
    ],
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
```

### 12.2 Main Layout with Outlet

```typescript
// src/components/layout/MainLayout.tsx
import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Footer } from './Footer';

export default function MainLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 container mx-auto py-6">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
```

### 12.3 Protected Route

```typescript
// src/router/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import { authStore } from '@/features/auth/stores/authStore';
import { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const isAuthenticated = authStore((state) => state.isAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
```

### 12.4 App.tsx

```typescript
// src/App.tsx
import { AppRouter } from './router';
import { Toaster } from 'sonner';

function App() {
  return (
    <>
      <AppRouter />
      <Toaster position="top-right" richColors />
    </>
  );
}

export default App;
```

---

## 13. shadcn/ui Integration

### 13.1 Installation

```bash
# Initialize shadcn/ui
npx shadcn@latest init

# Add components
npx shadcn@latest add button
npx shadcn@latest add card
npx shadcn@latest add dialog
npx shadcn@latest add form
npx shadcn@latest add input
npx shadcn@latest add textarea
npx shadcn@latest add toast
```

### 13.2 components.json Configuration

```json
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "new-york",
  "rsc": false,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.ts",
    "css": "src/styles/globals.css",
    "baseColor": "zinc",
    "cssVariables": true
  },
  "aliases": {
    "components": "@/components",
    "utils": "@/lib/utils",
    "ui": "@/components/ui",
    "lib": "@/lib",
    "hooks": "@/hooks"
  }
}
```

### 13.3 Card Component Usage

```typescript
// src/components/todos/TodoItem.tsx
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { TodoItem as TodoItemType } from '@/features/todos/schemas/todoSchemas';

interface TodoItemProps {
  todo: TodoItemType;
  onToggle: (id: string) => void;
  onDelete: (id: string) => void;
}

export function TodoItem({ todo, onToggle, onDelete }: TodoItemProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Checkbox
            checked={todo.isComplete}
            onCheckedChange={() => onToggle(todo.id)}
          />
          <span className={todo.isComplete ? 'line-through text-muted-foreground' : ''}>
            {todo.title}
          </span>
        </CardTitle>
      </CardHeader>
      {todo.description && (
        <CardContent>
          <p className="text-sm text-muted-foreground">{todo.description}</p>
        </CardContent>
      )}
      <CardFooter>
        <Button variant="destructive" size="sm" onClick={() => onDelete(todo.id)}>
          Delete
        </Button>
      </CardFooter>
    </Card>
  );
}
```

### 13.4 Toast Notifications

```typescript
// Using sonner (recommended by shadcn)
import { toast } from 'sonner';

// Success toast
toast.success('Todo created successfully');

// Error toast
toast.error('Failed to create todo');

// Custom toast
toast('Event has been created', {
  description: 'Sunday, December 03, 2023 at 9:00 AM',
  action: {
    label: 'Undo',
    onClick: () => console.log('Undo'),
  },
});
```

---

## 14. TailwindCSS v4

### 14.1 Vite Configuration

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

### 14.2 CSS Entry File

```css
/* src/styles/globals.css */
@import "tailwindcss";

@theme {
  --font-sans: 'Inter', system-ui, sans-serif;
  
  --color-primary: #3b82f6;
  --color-primary-foreground: #ffffff;
  
  --color-secondary: #6b7280;
  --color-secondary-foreground: #ffffff;
  
  --color-destructive: #ef4444;
  --color-destructive-foreground: #ffffff;
  
  --color-muted: #f3f4f6;
  --color-muted-foreground: #6b7280;
  
  --color-accent: #f59e0b;
  --color-accent-foreground: #ffffff;
  
  --color-border: #e5e7eb;
  --color-input: #e5e7eb;
  --color-ring: #3b82f6;
  
  --radius-sm: 0.25rem;
  --radius-md: 0.375rem;
  --radius-lg: 0.5rem;
  --radius-xl: 0.75rem;
}

@layer base {
  * {
    @apply border-border;
  }
  
  body {
    @apply bg-background text-foreground font-sans;
  }
}
```

### 14.3 Basic Theme Setup

```typescript
// tailwind.config.ts
import type { Config } from 'tailwindcss';

export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
} satisfies Config;
```

---

## 15. Basic Observability

### 15.1 Serilog Configuration

```csharp
// API/Program.cs
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SmartTodoApp")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting SmartTodoApp API");
    
    // Build and run app
    var app = builder.Build();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### 15.2 Structured Logging in Handlers

```csharp
// Application/TodoItems/Commands/CreateTodoItem/CreateTodoItemCommandHandler.cs
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;

    public CreateTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CommandType"] = nameof(CreateTodoItemCommand),
            ["Title"] = request.Title
        });

        _logger.LogInformation("Creating todo item");

        try
        {
            var todoItem = TodoItem.Create(request.Title, request.Description);
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Todo item created successfully with ID: {TodoItemId}", todoItem.Id);

            return todoItem.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create todo item");
            throw;
        }
    }
}
```

### 15.3 CorrelationId Middleware

```csharp
// API/Middleware/CorrelationIdMiddleware.cs
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SmartTodoApp.API.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId!;
        }

        return Guid.NewGuid().ToString();
    }
}

// Register in Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
```

### 15.4 Logging Behavior

```csharp
// Application/Common/Behaviors/LoggingBehavior.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SmartTodoApp.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

### 15.5 Health Checks

```csharp
// API/Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

---

## 16. API Documentation

### 16.1 OpenAPI Setup (.NET 10)

```csharp
// API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Use built-in .NET 10 OpenAPI support
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Smart Todo API v1");
    });
}
```

### 16.2 Security Definitions

```csharp
// API/OpenApi/BearerSecuritySchemeTransformer.cs
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace SmartTodoApp.API.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", securityScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        document.SecurityRequirements.Add(securityRequirement);

        return Task.CompletedTask;
    }
}
```

### 16.3 XML Documentation

```csharp
// Enable XML documentation in .csproj
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

```csharp
// API/Controllers/TodoItemsController.cs
/// <summary>
/// Manages todo items
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    /// <summary>
    /// Creates a new todo item
    /// </summary>
    /// <param name="command">The create todo command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the created todo item</returns>
    /// <response code="201">Returns the newly created item ID</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateTodoItemCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
```

---

## 17. CI/CD Pipeline

### 17.1 GitHub Actions: Backend

```yaml
# .github/workflows/backend-ci.yml
name: Backend CI

on:
  push:
    branches: [main, develop]
    paths:
      - 'src/SmartTodoApp.API/**'
      - 'src/SmartTodoApp.Application/**'
      - 'src/SmartTodoApp.Domain/**'
      - 'src/SmartTodoApp.Infrastructure/**'
  pull_request:
    branches: [main, develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run tests
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"

      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          files: '**/coverage.cobertura.xml'
```

### 17.2 GitHub Actions: Frontend

```yaml
# .github/workflows/frontend-ci.yml
name: Frontend CI

on:
  push:
    branches: [main, develop]
    paths:
      - 'src/frontend/**'
  pull_request:
    branches: [main, develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: 'src/frontend/package-lock.json'

      - name: Install dependencies
        working-directory: src/frontend
        run: npm ci

      - name: Lint
        working-directory: src/frontend
        run: npm run lint

      - name: Build
        working-directory: src/frontend
        run: npm run build

      - name: Run tests
        working-directory: src/frontend
        run: npm run test:ci

      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          files: src/frontend/coverage/coverage-final.json
```

### 17.3 GitHub Actions: Complete Pipeline

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main]

jobs:
  backend:
    name: Build & Test Backend
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release
      - run: dotnet test --configuration Release

  frontend:
    name: Build & Test Frontend
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
        working-directory: src/frontend
      - run: npm run build
        working-directory: src/frontend
      - run: npm test
        working-directory: src/frontend

  deploy:
    name: Deploy
    needs: [backend, frontend]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - name: Deploy to Azure
        run: echo "Deploy to Azure App Service"
        # Add actual deployment steps
```

---

## 18. Docker Setup

### 18.1 Backend Dockerfile

```dockerfile
# src/SmartTodoApp.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SmartTodoApp.API/SmartTodoApp.API.csproj", "SmartTodoApp.API/"]
COPY ["src/SmartTodoApp.Application/SmartTodoApp.Application.csproj", "SmartTodoApp.Application/"]
COPY ["src/SmartTodoApp.Domain/SmartTodoApp.Domain.csproj", "SmartTodoApp.Domain/"]
COPY ["src/SmartTodoApp.Infrastructure/SmartTodoApp.Infrastructure.csproj", "SmartTodoApp.Infrastructure/"]
COPY ["src/SmartTodoApp.Shared.Contracts/SmartTodoApp.Shared.Contracts.csproj", "SmartTodoApp.Shared.Contracts/"]
RUN dotnet restore "SmartTodoApp.API/SmartTodoApp.API.csproj"
COPY src/ .
WORKDIR "/src/SmartTodoApp.API"
RUN dotnet build "SmartTodoApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartTodoApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartTodoApp.API.dll"]
```

### 18.2 Frontend Dockerfile

```dockerfile
# src/frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**nginx.conf:**
```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://backend:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 18.3 docker-compose.yml

```yaml
# docker-compose.yml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: smart-todo-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrong@Passw0rd", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: .
      dockerfile: src/SmartTodoApp.API/Dockerfile
    container_name: smart-todo-backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=SmartTodoDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
      - Jwt__Key=your-secret-key-min-32-characters-long
      - Jwt__Issuer=SmartTodoApp
      - Jwt__Audience=SmartTodoAppClient
    ports:
      - "5000:8080"
    depends_on:
      sqlserver:
        condition: service_healthy

  frontend:
    build:
      context: src/frontend
      dockerfile: Dockerfile
    container_name: smart-todo-frontend
    environment:
      - VITE_API_BASE_URL=http://localhost:5000/api
    ports:
      - "3000:80"
    depends_on:
      - backend

volumes:
  sqlserver-data:
```

### 18.4 Docker Commands

```bash
# Build and run all services
docker-compose up --build

# Run in detached mode
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# Rebuild specific service
docker-compose up --build backend

# Remove volumes
docker-compose down -v
```

---

## 19. Example Error Responses

### 19.1 Validation Error (400)

```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": [
      "Title is required",
      "Title must not exceed 200 characters"
    ],
    "Description": [
      "Description must not exceed 1000 characters"
    ]
  },
  "traceId": "00-3e8f6c7d9a2b4e1f-9c8b7a6d5e4f3g2h-00",
  "correlationId": "3e8f6c7d-9a2b-4e1f-9c8b-7a6d5e4f3g2h"
}
```

### 19.2 Not Found (404)

```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.4",
  "title": "The specified resource was not found.",
  "status": 404,
  "detail": "Todo item with ID '123e4567-e89b-12d3-a456-426614174000' was not found",
  "traceId": "00-4f9g7d8e0b3c5f2a-0d9c8b7a6e5f4g3h-00",
  "correlationId": "4f9g7d8e-0b3c-5f2a-0d9c-8b7a6e5f4g3h"
}
```

### 19.3 Internal Server Error (500)

```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "traceId": "00-5g0h8e9f1c4d6g3b-1e0d9c8b7f6g5h4i-00",
  "correlationId": "5g0h8e9f-1c4d-6g3b-1e0d-9c8b7f6g5h4i"
}
```

### 19.4 Exception Handling Middleware

```csharp
// API/Middleware/ExceptionHandlingMiddleware.cs
using Microsoft.AspNetCore.Mvc;
using SmartTodoApp.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace SmartTodoApp.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(validationEx.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7807#section-6.5.1"
                }),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "The specified resource was not found.",
                    Detail = notFoundEx.Message,
                    Type = "https://tools.ietf.org/html/rfc7807#section-6.5.4"
                }),
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Detail = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred. Please try again later.",
                    Type = "https://tools.ietf.org/html/rfc7807#section-6.6.1"
                })
        };

        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId.ToString();
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
```

---

## 20. Do/Avoid Table

| ✅ DO | ❌ AVOID |
|------|----------|
| Use CQRS: separate Commands and Queries | Mix read and write logic in single handler |
| Apply FluentValidation to all Commands/Queries | Skip validation and rely on model annotations only |
| Use `AsNoTracking()` for read-only queries | Track entities unnecessarily in queries |
| Return Problem Details (RFC 7807) for errors | Return raw exception messages to clients |
| Log with CorrelationId in every handler | Log without context or correlation |
| Use AutoMapper with `ProjectTo` for DTOs | Manually map entities in queries |
| Implement async/await throughout | Use `.Result` or `.Wait()` |
| Use CancellationToken in all async methods | Ignore cancellation tokens |
| Keep Controllers thin (delegate to MediatR) | Put business logic in controllers |
| Use Polly for resilience (retry, circuit breaker) | Retry failed operations manually |
| Authenticate with JWT Bearer tokens | Use session-based auth for APIs |
| Apply authorization policies consistently | Check permissions manually in handlers |
| Use TanStack Query for server state | Mix server state with client state |
| Use Zustand for client-only state | Use TanStack Query for UI state |
| Validate with Zod on frontend | Trust backend validation only |
| Use shadcn/ui components consistently | Mix multiple UI libraries |
| Configure CORS properly | Allow all origins in production |
| Write unit tests for handlers and validators | Skip testing business logic |
| Write integration tests for API endpoints | Test only with Postman manually |
| Use structured logging (Serilog) | Use `Console.WriteLine` for logging |
| Apply migrations in order | Modify migration files after applying |
| Use EF Core conventions and Fluent API | Rely on attributes for complex mappings |
| Configure health checks | Deploy without monitoring readiness |
| Use `record` types for DTOs and Commands | Use `class` types for immutable data |
| Return specific exceptions (NotFoundException) | Throw generic exceptions everywhere |
| Configure rate limiting on public endpoints | Leave APIs unprotected from abuse |
| Use dependency injection | Instantiate services with `new` |
| Follow Clean Architecture layer boundaries | Reference Infrastructure from Domain |
| Use docker-compose for local development | Require manual setup of dependencies |

---

## 21. Quick Reference

### 21.1 Common Commands

**Backend:**
```bash
# Create new migration
dotnet ef migrations add <MigrationName> --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API

# Update database
dotnet ef database update --project src/SmartTodoApp.Infrastructure --startup-project src/SmartTodoApp.API

# Run API
dotnet run --project src/SmartTodoApp.API

# Run tests
dotnet test

# Build solution
dotnet build

# Clean solution
dotnet clean
```

**Frontend:**
```bash
# Install dependencies
npm install

# Run dev server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run tests
npm test

# Run tests with coverage
npm run test:coverage

# Lint
npm run lint

# Add shadcn component
npx shadcn@latest add <component-name>
```

### 21.2 File Templates Quick Access

- **Command**: `Application/{Feature}/Commands/{Action}/{ActionCommand.cs, Handler.cs, Validator.cs}`
- **Query**: `Application/{Feature}/Queries/{Action}/{ActionQuery.cs, Handler.cs}`
- **Controller**: `API/Controllers/{Feature}Controller.cs`
- **Entity**: `Domain/Entities/{EntityName}.cs`
- **DTO**: `Shared.Contracts/{Feature}/{EntityName}Dto.cs`
- **Component**: `src/components/{feature}/{ComponentName}.tsx`
- **Hook**: `src/features/{feature}/hooks/use{HookName}.ts`
- **Schema**: `src/features/{feature}/schemas/{entity}Schemas.ts`

---

**End of GitHub Copilot Instructions**
